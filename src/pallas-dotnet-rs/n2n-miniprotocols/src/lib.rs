use lazy_static::lazy_static;
use rnet::{net, Net};
use tokio::runtime::Runtime;

use pallas::{
    ledger::traverse::MultiEraTx,
    network::{
        facades::PeerClient,
        miniprotocols::{
            chainsync::{self}, 
            txsubmission::{self, EraTxBody, TxIdAndSize}, 
            Point as PallasPoint 
        }
    },
};

rnet::root!();

lazy_static! {
    static ref RT: Runtime = Runtime::new().expect("Failed to create Tokio runtime");
}

#[derive(Net)]
pub struct Point {
    slot: u64,
    hash: Vec<u8>,
}

#[derive(Net)]
pub struct NextResponse {
    action: u8,
    tip: Option<Point>,
    block_cbor: Option<Vec<u8>>,
}

#[derive(Net)]
pub struct NodeToNodeWrapper {
    client_ptr: usize,
}

impl NodeToNodeWrapper {    
    #[net]
    pub fn connect(server: String, network_magic: u64) -> NodeToNodeWrapper {
        NodeToNodeWrapper::connect(server, network_magic)
    }

    pub fn connect(server: String, network_magic: u64) -> NodeToNodeWrapper {
        let client = RT.block_on(async {
            PeerClient::connect(server, network_magic)
            .await
            .unwrap()
        });

        let client_box = Box::new(client);
        let client_ptr = Box::into_raw(client_box) as usize;

        NodeToNodeWrapper { client_ptr }
    }

    #[net]
    pub fn chain_sync_next(client_wrapper: NodeToNodeWrapper) -> NextResponse {
        unsafe {
            let client_ptr = client_wrapper.client_ptr as *mut PeerClient;
            let mut client = Box::from_raw(client_ptr);

            // Get the next block
            let result = RT.block_on(async {
                if client.chainsync().has_agency() {
                    // When the client has the agency, send a request for the next block
                    client.chainsync().request_next().await
                } else {
                    // When the client does not have the agency, wait for the server's response
                    client.chainsync().recv_while_must_reply().await
                }
            });

            let next_response = match result {
                Ok(next) => match next {
                    chainsync::NextResponse::RollForward(header, tip) => NextResponse {
                        action: 1,
                        tip: match tip.0 {
                            PallasPoint::Origin => Some(Point {
                                slot: 0,
                                hash: vec![],
                            }),
                            PallasPoint::Specific(slot, hash) => Some(Point { slot, hash }),
                        },
                        block_cbor: Some(header.cbor),
                    },
                    chainsync::NextResponse::RollBackward(point, tip) => NextResponse {
                        action: 2,
                        tip: match tip.0 {
                            PallasPoint::Origin => Some(Point {
                                slot: 0,
                                hash: vec![],
                            }),
                            PallasPoint::Specific(slot, hash) => Some(Point { slot, hash}),
                        },
                        block_cbor: match point {
                            PallasPoint::Origin => NodeToNodeWrapper::fetch_block(client_wrapper, Point { slot: 0, hash: vec![] }),
                            PallasPoint::Specific(slot, hash) => NodeToNodeWrapper::fetch_block(client_wrapper, Point { slot, hash })
                        }
                    },
                    chainsync::NextResponse::Await => NextResponse {
                        action: 3,
                        tip: None,
                        block_cbor: None
                    }
                },
                Err(e) => {
                    println!("chain_sync_next error: {:?}", e);
                    NextResponse {
                        action: 0,
                        tip: None,
                        block_cbor: None,
                    }
                }
            };

            let _ = Box::into_raw(client);

            next_response
        }
    }

    #[net]
    pub fn fetch_block(client_wrapper: NodeToNodeWrapper, point: Point) -> Option<Vec<u8>> {
        NodeToNodeWrapper::fetch_block(client_wrapper, point)
    }

    pub fn fetch_block(client_wrapper: NodeToNodeWrapper, point: Point) -> Option<Vec<u8>> {
        unsafe {
            let client_ptr = client_wrapper.client_ptr as *mut PeerClient;
            let mut client = Box::from_raw(client_ptr);

            let block = RT.block_on(async {
                client.blockfetch.fetch_single(PallasPoint::new(point.slot, point.hash))
                    .await
                    .unwrap()
            });

            let _ = Box::into_raw(client);

            Some(block)
        }
    }

    #[net]
    pub fn get_tip(client_wrapper: NodeToNodeWrapper) -> Point {
        unsafe {
            let cleint_ptr = client_wrapper.client_ptr as *mut PeerClient;
            let mut client = Box::from_raw(cleint_ptr);

            let tip = RT.block_on(async {
                client.chainsync().intersect_tip().await.unwrap()
            });

            let _ = Box::into_raw(client);

            match tip {
                PallasPoint::Origin => Point { slot: 0, hash: vec![] },
                PallasPoint::Specific(slot, hash) => Point { slot, hash }
            }
        }
    }

    #[net]
    pub fn find_intersect(client_wrapper: NodeToNodeWrapper, point: Point) -> Option<Point> {
        unsafe {
            let client_ptr = client_wrapper.client_ptr as *mut PeerClient;
            let mut client = Box::from_raw(client_ptr);

            let known_points = vec![PallasPoint::Specific(point.slot, point.hash)];

            let (intersect_point, _) = RT.block_on(async {
                client.chainsync().find_intersect(known_points).await.unwrap()
            });

            let _ = Box::into_raw(client);

            intersect_point.map(|pallas_point| match pallas_point {
                PallasPoint::Origin => Point {
                    slot: 0,
                    hash: vec![],
                },
                PallasPoint::Specific(slot, hash) => Point { slot, hash },
            })
        }
    }

    #[net]
    pub fn submit_tx(server: String, magic: u64, tx: Vec<u8>) -> Vec<u8> {
        NodeToNodeWrapper::submit_tx(server, magic, tx)
    }

    pub fn submit_tx(server: String, magic: u64, tx: Vec<u8>) -> Vec<u8> {
        let ids = RT.block_on(async {
            let tx_clone = tx.clone();
            let multi_era_tx = MultiEraTx::decode(&tx_clone).unwrap();
            let tx_era = multi_era_tx.era() as u16;
            let mempool = vec![(multi_era_tx.hash(), tx.clone())];
            let mut peer = PeerClient::connect(server, magic).await.unwrap();
            let client_txsub = peer.txsubmission();

            client_txsub.send_init().await.unwrap();

            let _ = match client_txsub.next_request().await.unwrap() {
                txsubmission::Request::TxIds(ack, _) => {
                    assert_eq!(*client_txsub.state(), txsubmission::State::TxIdsBlocking);
                    ack
                }
                txsubmission::Request::TxIdsNonBlocking(ack, _) => {
                    assert_eq!(*client_txsub.state(), txsubmission::State::TxIdsNonBlocking);
                    ack
                }
                _ => panic!("unexpected message"),
            };

            let to_send = mempool.clone();
            let ids_and_size = to_send
                .clone()
                .into_iter()
                .map(|(h, b)| {
                    TxIdAndSize(txsubmission::EraTxId(tx_era, h.to_vec()), b.len() as u32)
                })
                .collect();

            client_txsub.reply_tx_ids(ids_and_size).await.unwrap();

            let ids = match client_txsub.next_request().await.unwrap() {
                txsubmission::Request::Txs(ids) => ids,
                _ => panic!("unexpected message"),
            };

            let txs_to_send: Vec<_> = to_send
                .into_iter()
                .map(|(_, b)| EraTxBody(tx_era, b))
                .collect();
            client_txsub.reply_txs(txs_to_send).await.unwrap();

            match client_txsub.next_request().await.unwrap() {
                txsubmission::Request::TxIdsNonBlocking(_, _) => {
                    assert_eq!(*client_txsub.state(), txsubmission::State::TxIdsNonBlocking);
                }
                _ => panic!("unexpected message"),
            };

            client_txsub.reply_tx_ids(vec![]).await.unwrap();

            match client_txsub.next_request().await.unwrap() {
                txsubmission::Request::TxIds(ack, _) => {
                    assert_eq!(*client_txsub.state(), txsubmission::State::TxIdsBlocking);

                    client_txsub.send_done().await.unwrap();
                    assert_eq!(*client_txsub.state(), txsubmission::State::Done);

                    ack
                }
                txsubmission::Request::TxIdsNonBlocking(ack, _) => {
                    assert_eq!(*client_txsub.state(), txsubmission::State::TxIdsNonBlocking);

                    ack
                }
                _ => panic!("unexpected message"),
            };

            let id_bytes = ids
                .iter()
                .flat_map(|id| id.1.to_vec()) // Assuming `Hash<32>` is a tuple struct with the first element being an array `[u8; 32]`
                .collect();
            id_bytes
        });
        ids
    }
}