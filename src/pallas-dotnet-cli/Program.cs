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

NodeClient? nodeClient = new();
Point? tip = await nodeClient.ConnectAsync("/tmp/node.socket", NetworkMagic.PREVIEW);

nodeClient.Disconnected += (sender, args) =>
{
    ConsoleHelper.WriteLine($"Disconnected ", ConsoleColor.DarkRed);
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

        Console.WriteLine(action);
        Console.WriteLine($"Slot: {nextResponse.Tip.Slot} Hash: {nextResponse.Tip.Hash}");
        
        if (nextResponse.Action == NextResponseAction.RollForward)
        {
            string cborHex = Convert.ToHexString(nextResponse.BlockCbor);
            Console.WriteLine(cborHex);
        }

        Console.WriteLine("----------------------------------------");
    }
};

await nodeClient.StartChainSyncAsync(new Point(
    57222760,
    new Hash("7bed2d5f3c473bc43d9b9597b50ee7dc29511da2830b34278e12f7b6cf70b49e")
));

while (true)
{
    await Task.Delay(1000);
}