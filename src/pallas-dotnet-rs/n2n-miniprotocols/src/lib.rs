use lazy_static::lazy_static;
use rnet::{net, Net};
use tokio::runtime::Runtime;

use pallas::{
    ledger::traverse::MultiEraTx,
    network::{
        facades::PeerClient,
        miniprotocols::{
            txsubmission::{self, EraTxBody, TxIdAndSize},
            Point as PallasPoint,
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