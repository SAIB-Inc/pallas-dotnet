using System.Diagnostics;
using PallasDotnet;
using PallasDotnet.Models;

static double GetCurrentMemoryUsageInMB()
{
    Process currentProcess = Process.GetCurrentProcess();

    // Getting the physical memory usage of the current process in bytes
    long memoryUsed = currentProcess.WorkingSet64;

    // Convert to megabytes for easier reading
    double memoryUsedMb = memoryUsed / 1024.0 / 1024.0;

    return memoryUsedMb;
}

// N2C Protocol Implementation
async void ExecuteN2cProtocol()
{
    NodeClient? nodeClient = new();
    Point? tip = await nodeClient.ConnectAsync("/tmp/node.socket", NetworkMagic.PREVIEW);

    nodeClient.Disconnected += (sender, args) =>
    {
        ConsoleHelper.WriteLine($"Disconnected ", ConsoleColor.DarkRed);
    };
    nodeClient.Disconnected += (sender, args) =>
    {
        ConsoleHelper.WriteLine($"Disconnected ", ConsoleColor.DarkRed);
    };

    nodeClient.Reconnected += (sender, args) =>
    {
        ConsoleHelper.WriteLine($"Reconnected ", ConsoleColor.DarkGreen);
    };
    nodeClient.Reconnected += (sender, args) =>
    {
        ConsoleHelper.WriteLine($"Reconnected ", ConsoleColor.DarkGreen);
    };

    nodeClient.ChainSyncNextResponse += (sender, args) =>
    {
        NextResponse nextResponse = args.NextResponse;
        
        if (nextResponse.Action == NextResponseAction.Await)
        {
            Console.WriteLine("Awaiting...");
        }
        else if (nextResponse.Action == NextResponseAction.RollForward || nextResponse.Action == NextResponseAction.RollBack)
        {
            string action = nextResponse.Action == NextResponseAction.RollBack ? "Rolling back..." : "Rolling forward...";
    nodeClient.ChainSyncNextResponse += (sender, args) =>
    {
        NextResponse nextResponse = args.NextResponse;
        
        if (nextResponse.Action == NextResponseAction.Await)
        {
            Console.WriteLine("Awaiting...");
        }
        else if (nextResponse.Action == NextResponseAction.RollForward || nextResponse.Action == NextResponseAction.RollBack)
        {
            string action = nextResponse.Action == NextResponseAction.RollBack ? "Rolling back..." : "Rolling forward...";

            Console.WriteLine(action);
            Console.WriteLine($"Slot: {nextResponse.Tip.Slot} Hash: {nextResponse.Tip.Hash}");
            
            if (nextResponse.Action == NextResponseAction.RollForward)
            {
                Console.WriteLine("Block:");
                string cborHex = Convert.ToHexString(nextResponse.BlockCbor);
                Console.WriteLine(cborHex);
            }
            Console.WriteLine(action);
            Console.WriteLine($"Slot: {nextResponse.Tip.Slot} Hash: {nextResponse.Tip.Hash}");
            
            if (nextResponse.Action == NextResponseAction.RollForward)
            {
                Console.WriteLine("Block:");
                string cborHex = Convert.ToHexString(nextResponse.BlockCbor);
                Console.WriteLine(cborHex);
            }

            Console.WriteLine("--------------------------------------------------------------------------------");
        }
    };
            Console.WriteLine("--------------------------------------------------------------------------------");
        }
    };

    await nodeClient.StartChainSyncAsync(new Point(
        57491927,
        new Hash("7f00f6f9d844f7ec5937fa7ec43fcce9f55a8b47fa3703a08cd50c7be6869735")
    ));
}

// N2N Protocol Implementation
async void ExecuteN2nProtocol()
{
    N2nClient? n2nClient = new();
    string? connectionStatus = await n2nClient.ConnectAsync("localhost:31000", NetworkMagic.PREVIEW);

    if (connectionStatus is not null)
    {
        Console.WriteLine(connectionStatus);
    }

    Console.WriteLine("Fetching block...");
    
    byte[] block_cbor = await n2nClient.FetchBlockAsync(new Point(
        57491927,
        new Hash("7f00f6f9d844f7ec5937fa7ec43fcce9f55a8b47fa3703a08cd50c7be6869735")
    ));

    Console.WriteLine(Convert.ToHexString(block_cbor));
}

await Task.Run(ExecuteN2nProtocol);

while (true)
{
    await Task.Delay(1000);
}