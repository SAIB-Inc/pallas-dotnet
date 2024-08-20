using PallasDotnet.Models;
using NextResponseRs = PallasDotnetN2c.PallasDotnetN2c.NextResponse;

namespace PallasDotnet;

public class ChainSyncNextResponseEventArgs(NextResponse nextResponse) : EventArgs
{
    public NextResponse NextResponse { get; } = nextResponse;
}

public class NodeClient
{
    private PallasDotnetN2c.PallasDotnetN2c.NodeToClientWrapper? _n2cClient;
    private string _socketPath = string.Empty;
    private ulong _magicNumber = 0;
    private byte[] _lastHash = [];
    private ulong _lastSlot = 0;

    public bool IsConnected => _n2cClient != null;
    public bool IsSyncing { get; private set; }
    public bool ShouldReconnect { get; set; } = true;

    public event EventHandler<ChainSyncNextResponseEventArgs>? ChainSyncNextResponse;
    public event EventHandler? Disconnected;
    public event EventHandler? Reconnected;

    public async Task<Point> ConnectAsync(string socketPath, ulong magicNumber)
    {
        _magicNumber = magicNumber;
        _socketPath = socketPath;

        return await Task.Run(() =>
        {
            _n2cClient = PallasDotnetN2c.PallasDotnetN2c.Connect(socketPath, magicNumber);

            if (_n2cClient is null)
            {
                throw new Exception("Failed to connect to node");
            }

            var pallasPoint = PallasDotnetN2c.PallasDotnetN2c.GetTip(_n2cClient.Value);
            return Utils.MapPallasPoint(pallasPoint);
        });
    }

    public async Task StartChainSyncAsync(Point? intersection = null)
    {
        if (_n2cClient is null)
        {
            throw new Exception("Not connected to node");
        }

        if (intersection is not null)
        {
            await Task.Run(() =>
            {
                PallasDotnetN2c.PallasDotnetN2c.FindIntersect(_n2cClient.Value, new PallasDotnetN2c.PallasDotnetN2c.Point
                {
                    slot = intersection.Slot,
                    hash = new List<byte>(intersection.Hash.Bytes)
                });
            });
        }

        _= Task.Run(() =>
        {
            IsSyncing = true;
            while (IsSyncing)
            {
                NextResponseRs nextResponseRs = PallasDotnetN2c.PallasDotnetN2c.ChainSyncNext(_n2cClient.Value);

                if ((NextResponseAction)nextResponseRs.action == NextResponseAction.Error)
                {
                    if (ShouldReconnect)
                    {
                        _n2cClient = PallasDotnetN2c.PallasDotnetN2c.Connect(_socketPath, _magicNumber);
                        PallasDotnetN2c.PallasDotnetN2c.FindIntersect(_n2cClient.Value, new PallasDotnetN2c.PallasDotnetN2c.Point
                        {
                            slot = _lastSlot,
                            hash = [.. _lastHash]
                        });
                        Reconnected?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        IsSyncing = false;
                        Disconnected?.Invoke(this, EventArgs.Empty);
                    }
                }
                else if ((NextResponseAction)nextResponseRs.action == NextResponseAction.Await)
                {
                    ChainSyncNextResponse?.Invoke(this, new(new(
                        NextResponseAction.Await,
                        default!,
                        default!
                    )));
                }
                else
                {
                    NextResponseAction nextResponseAction = (NextResponseAction)nextResponseRs.action;
                    Point tip = new(nextResponseRs.tip.slot, new([.. nextResponseRs.tip.hash]));

                    NextResponse nextResponse = nextResponseAction switch
                    {
                        NextResponseAction.RollForward => new(nextResponseAction, tip, [.. nextResponseRs.blockCbor]),
                        NextResponseAction.RollBack => new(nextResponseAction, tip, default!),
                        _ => default!
                    };
                    ChainSyncNextResponse?.Invoke(this, new(nextResponse));
                }
            }
        });
    }

    public async Task<List<byte[]>> GetUtxoByAddressCborAsync(string address)
    {
        if (_n2cClient is null)
        {
            throw new Exception("Not connected to node");
        }
        var utxoByAddress = await Task.Run(() => PallasDotnetN2c.PallasDotnetN2c.GetUtxoByAddressCbor(_n2cClient.Value, address));
        return utxoByAddress?.Select(utxo => utxo.ToArray()).ToList() ?? [];
    }

    public void StopSync()
    {
        IsSyncing = false;
    }

    public Task DisconnectAsync()
    {
        if (_n2cClient is null)
        {
            throw new Exception("Not connected to node");
        }
        return Task.Run(() => PallasDotnetN2c.PallasDotnetN2c.Disconnect(_n2cClient.Value));
    }
}
