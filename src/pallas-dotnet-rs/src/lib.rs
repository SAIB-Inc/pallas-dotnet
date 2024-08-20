use std::{
    collections::HashMap,
    hash::Hash,
    ops::Deref, vec,
};
use lazy_static::lazy_static;
use pallas::{
    ledger::{
        addresses::{Address, ByronAddress},
        traverse::MultiEraTx,
    },
    network::{
        facades::{NodeClient, PeerClient},
        miniprotocols::{
            chainsync::{self}, localstate::queries_v16::{self, Addr}, 
            txsubmission::{self, EraTxBody, TxIdAndSize}, Point as PallasPoint, 
            MAINNET_MAGIC, PREVIEW_MAGIC, PRE_PRODUCTION_MAGIC, TESTNET_MAGIC
        }
    },
};
use rnet::{net, Net};
use tokio::runtime::Runtime;

rnet::root!();

lazy_static! {
    static ref RT: Runtime = Runtime::new().expect("Failed to create Tokio runtime");
}

#[derive(Net)]
pub struct NetworkMagic {}

impl NetworkMagic {
    #[net]
    pub fn mainnet_magic() -> u64 {
        MAINNET_MAGIC
    }

    #[net]
    pub fn testnet_magic() -> u64 {
        TESTNET_MAGIC
    }

    #[net]
    pub fn preview_magic() -> u64 {
        PREVIEW_MAGIC
    }

    #[net]
    pub fn pre_production_magic() -> u64 {
        PRE_PRODUCTION_MAGIC
    }
}

#[derive(Net)]
pub struct Point {
    slot: u64,
    hash: Vec<u8>,
}

#[derive(Net, Debug, Eq, PartialEq, Hash)]
pub struct TransactionInput {
    id: Vec<u8>,
    index: u64,
}

#[derive(Net)]
struct Datum {
    datum_type: u8,
    data: Option<Vec<u8>>,
}

#[derive(Net)]
pub struct TransactionOutput {
    address: Vec<u8>,
    amount: Value,
    index: usize,
    datum: Option<Datum>,
    raw: Vec<u8>,
}

#[derive(Net)]
pub struct Value {
    coin: Coin,
    multi_asset: HashMap<PolicyId, HashMap<AssetName, Coin>>,
}

pub type Coin = u64;
pub type MintCoin = i64;
pub type PolicyId = Vec<u8>;
pub type AssetName = Vec<u8>;
pub type RedeemerTag = u8;
pub type UtxoByAddress = HashMap<TransactionInput, TransactionOutput>;

#[derive(Net)]
pub struct NextResponse {
    action: u8,
    tip: Option<Point>,
    block_cbor: Option<Vec<u8>>,
}

#[derive(Net)]
pub struct NodeClientWrapper {
    client_ptr: usize,
    socket_path: String,
}

impl NodeClientWrapper {
    #[net]
    pub fn connect(socket_path: String, network_magic: u64) -> NodeClientWrapper {
        NodeClientWrapper::connect(socket_path, network_magic)
    }

    pub fn connect(socket_path: String, network_magic: u64) -> NodeClientWrapper {
        let client = RT.block_on(async {
            NodeClient::connect(&socket_path, network_magic)
                .await
                .unwrap()
        });

        let client_box = Box::new(client);
        let client_ptr = Box::into_raw(client_box) as usize;

        NodeClientWrapper {
            client_ptr,
            socket_path,
        }
    }

    #[net]
    pub fn get_utxo_by_address_cbor(
        client_wrapper: NodeClientWrapper,
        address: String,
    ) -> Vec<Vec<u8>> {
        unsafe {
            let client_ptr = client_wrapper.client_ptr as *mut NodeClient;
            let mut client = Box::from_raw(client_ptr);

            // Query Utxo by address cbor
            let utxos_by_address_cbor = RT.block_on(async {
                let client = client.statequery();

                client.send_reacquire(None).await.unwrap();
                client.recv_while_acquiring().await.unwrap();

                let era = queries_v16::get_current_era(client).await.unwrap();
                let addrz: Address = Address::from_bech32(&address).unwrap();
                let addrz: Addr = addrz.to_vec().into();
                let query = queries_v16::BlockQuery::GetUTxOByAddress(vec![addrz]);
                queries_v16::get_cbor(client, era, query).await.unwrap()
            });

            // Convert client back to a raw pointer for future use
            let _ = Box::into_raw(client);

            utxos_by_address_cbor
                .into_iter()
                .map(|tag_wrap_instance| tag_wrap_instance.0.deref().clone())
                .collect()
        }
    }

    #[net]
    pub fn get_tip(client_wrapper: NodeClientWrapper) -> Point {
        unsafe {
            let client_ptr = client_wrapper.client_ptr as *mut NodeClient;
            let mut client = Box::from_raw(client_ptr);

            // Get the tip
            let tip = RT.block_on(async {
                let state_query_client = client.statequery();

                state_query_client.acquire(None).await.unwrap();

                queries_v16::get_chain_point(state_query_client)
                    .await
                    .unwrap()
            });

            // Convert client back to a raw pointer for future use
            let _ = Box::into_raw(client);

            match tip {
                PallasPoint::Origin => Point {
                    slot: 0,
                    hash: vec![],
                },
                PallasPoint::Specific(slot, hash) => Point { slot, hash },
            }
        }
    }

    #[net]
    pub fn find_intersect(client_wrapper: NodeClientWrapper, known_point: Point) -> Option<Point> {
        NodeClientWrapper::find_intersect(client_wrapper, known_point)
    }

    pub fn find_intersect(client_wrapper: NodeClientWrapper, known_point: Point) -> Option<Point> {
        unsafe {
            let client_ptr = client_wrapper.client_ptr as *mut NodeClient;

            // Convert the raw pointer back to a Box to deallocate the memory
            let mut _client = Box::from_raw(client_ptr);
            let client = _client.chainsync();

            let known_points = vec![PallasPoint::Specific(known_point.slot, known_point.hash)];

            // Get the intersecting point and the tip
            let (intersect_point, _tip) =
                RT.block_on(async { client.find_intersect(known_points).await.unwrap() });

            // Convert client back to a raw pointer for future use
            let _ = Box::into_raw(_client);

            // Match on the intersecting point
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
    pub fn chain_sync_next(client_wrapper: NodeClientWrapper) -> NextResponse {
        unsafe {
            let client_ptr = client_wrapper.client_ptr as *mut NodeClient;
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
                    chainsync::NextResponse::RollForward(block, tip) => NextResponse {
                        action: 1,
                        tip: match tip.0 {
                            PallasPoint::Origin => Some(Point {
                                slot: 0,
                                hash: vec![],
                            }),
                            PallasPoint::Specific(slot, hash) => Some(Point { slot, hash }),
                        },
                        block_cbor: Some(block.0),
                    },
                    chainsync::NextResponse::RollBackward(_, tip) => NextResponse {
                        action: 2,
                        tip: match tip.0 {
                            PallasPoint::Origin => Some(Point {
                                slot: 0,
                                hash: vec![],
                            }),
                            PallasPoint::Specific(slot, hash) => Some(Point { slot, hash }),
                        },
                        block_cbor: None
                    },
                    chainsync::NextResponse::Await => NextResponse {
                        action: 3,
                        tip: None,
                        block_cbor: None,
                    },
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

            // Convert client back to a raw pointer for future use
            let _ = Box::into_raw(client);

            next_response
        }
    }

    #[net]
    pub fn chain_sync_has_agency(client_wrapper: NodeClientWrapper) -> bool {
        unsafe {
            let client_ptr = client_wrapper.client_ptr as *mut NodeClient;

            // Convert the raw pointer back to a Box to deallocate the memory
            let mut _client = Box::from_raw(client_ptr);

            let has_agency = _client.chainsync().has_agency();

            // Convert client back to a raw pointer for future use
            let _ = Box::into_raw(_client);

            has_agency
        }
    }

    #[net]
    pub fn disconnect(client_wrapper: NodeClientWrapper) {
        unsafe {
            let client_ptr = client_wrapper.client_ptr as *mut NodeClient;

            let mut _client = Box::from_raw(client_ptr);

            RT.block_on(async {
                _client.abort().await;
            });
        }
    }
}

#[derive(Net)]
pub struct PallasUtility {}

impl PallasUtility {
    #[net]
    pub fn address_bytes_to_bech32(address_bytes: Vec<u8>) -> String {
        match Address::from_bytes(&address_bytes).unwrap().to_bech32() {
            Ok(address) => address,
            Err(_) => ByronAddress::from_bytes(&address_bytes)
                .unwrap()
                .to_base58(),
        }
    }
}

#[derive(Net)]
pub struct TxSubmit {}

impl TxSubmit {
    #[net]
    pub fn submit_tx(server: String, magic: u64, tx: Vec<u8>) -> Vec<u8> {
        TxSubmit::submit_tx(server, magic, tx)
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
