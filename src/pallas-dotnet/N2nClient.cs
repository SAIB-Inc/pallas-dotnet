using PallasDotnet.Models;

namespace PallasDotnet;

public class N2nClient
{
    private PallasDotnetN2n.PallasDotnetN2n.NodeToNodeWrapper? _n2nClient;
    private string _server = string.Empty;
    private ulong _magicNumber = 0;

    public async Task<string> ConnectAsync(string server, ulong magicNumber)
    {
        return await Task.Run(() => {
            _n2nClient = PallasDotnetN2n.PallasDotnetN2n.Connect(server, magicNumber);

            if (_n2nClient is null)
            {
                throw new Exception("Failed to connect to node");
            }

            _server = server;
            _magicNumber = magicNumber;  

            return "Successfully connected to node";
        });
    }

    public async Task<byte[]> FetchBlockAsync(Point? intersection)
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
}