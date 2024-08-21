using PallasDotnet.Models;
using NextResponseRs = PallasDotnetN2n.PallasDotnetN2n.NextResponse;
using PallasDotnet.EventArguments;

namespace PallasDotnet;

public class N2nClient
{
    private PallasDotnetN2n.PallasDotnetN2n.NodeToNodeWrapper? _n2nClient;
    private string _server = string.Empty;
    private ulong _magicNumber = 0;
    private bool IsSyncing { get; set; }
    private bool IsConnected => _n2nClient != null;
    public bool ShouldReconnect { get; set; } = true; 
    private ulong _lastSlot = 0;   
    private byte[] _lastHash = [];
    public event EventHandler<ChainSyncNextResponseEventArgs>? ChainSyncNextResponse;
    public event EventHandler? Disconnected;
    public event EventHandler? Reconnected;

    public async Task<Point> ConnectAsync(string server, ulong magicNumber)
    {
        _n2nClient = PallasDotnetN2n.PallasDotnetN2n.Connect(server, magicNumber);

        if (_n2nClient is null)
        {
            throw new Exception("Failed to connect to node");
        }

        _server = server;
        _magicNumber = magicNumber;  

        return await GetTipAsync();
    }

    public async Task StartChainSyncAsync(Point? intersection = null)
    {
        if (_n2nClient is null)
        {
            throw new Exception("Not connected to node");
        }

        if (intersection is not null)
        {
            await Task.Run(() =>
            {
                PallasDotnetN2n.PallasDotnetN2n.FindIntersect(_n2nClient.Value, new PallasDotnetN2n.PallasDotnetN2n.Point
                {
                    slot = intersection.Slot,
                    hash = new List<byte>(intersection.Hash.Bytes)
                });
            });
        }

        _ = Task.Run(() =>{
            IsSyncing = true;
            
            while (IsSyncing)
            {
                NextResponseRs nextResponseRs = PallasDotnetN2n.PallasDotnetN2n.ChainSyncNext(_n2nClient.Value);

                if ((NextResponseAction)nextResponseRs.action == NextResponseAction.Error)
                {
                    if (ShouldReconnect)
                    {
                        _n2nClient = PallasDotnetN2n.PallasDotnetN2n.Connect(_server, _magicNumber);

                        PallasDotnetN2n.PallasDotnetN2n.FindIntersect(_n2nClient.Value, new PallasDotnetN2n.PallasDotnetN2n.Point
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

                    _lastSlot = nextResponse.Tip.Slot;
                    _lastHash = nextResponse.Tip.Hash.Bytes;

                    ChainSyncNextResponse?.Invoke(this, new(nextResponse));
                }
            }
        });
    }

    public async Task<byte[]> FetchBlockAsync(Point? intersection = null)
    {
        if (_n2nClient is null)
        {
            throw new Exception("Not connected to node");
        }

        if (intersection is null)
        {
            throw new Exception("Intersection not provided");
        }

        return await Task.Run(() => {
            return PallasDotnetN2n.PallasDotnetN2n.FetchBlock(_n2nClient.Value, new PallasDotnetN2n.PallasDotnetN2n.Point
            {
                slot = intersection.Slot,
                hash = new List<byte>(intersection.Hash.Bytes)
            }).ToArray();
        });
    }

    public async Task<Point> GetTipAsync()
    {
        if (_n2nClient is null)
        {
            throw new Exception("Not connected to node");
        }

        PallasDotnetN2n.PallasDotnetN2n.Point tip = PallasDotnetN2n.PallasDotnetN2n.GetTip(_n2nClient.Value);
        
        return await Task.Run(() => {
            return new Point(tip.slot, new([..tip.hash]));
        });
    }
}