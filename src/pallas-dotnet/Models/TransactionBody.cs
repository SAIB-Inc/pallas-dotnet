using System.Text.Json;

namespace PallasDotnet.Models;

public record TransactionBody(
    Hash Id,
    ulong Index,
    ushort Era,
    IEnumerable<TransactionInput> Inputs,
    IEnumerable<TransactionOutput> Outputs,
    Dictionary<Hash,Dictionary<Hash,long>> Mint,
    JsonElement? MetaData,
    IEnumerable<Redeemer>? Redeemers,
    byte[] Raw
);