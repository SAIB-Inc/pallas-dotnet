using System.Diagnostics;
using PallasDotnet;
using PallasDotnet.Models;
using Spectre.Console;

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

    string message = nextResponse.Action == NextResponseAction.RollBack ? "Rolling back." : "Rolling forward.";
    string cborHex = Convert.ToHexString(nextResponse.BlockCbor);
    Console.WriteLine(message);
    Console.WriteLine(cborHex);
};

await nodeClient.StartChainSyncAsync(new Point(
    57079142,
    new Hash("1b56ac57c008fa6f19a6b83c73cb415e2e04200ae9bc8ab74614a903b6d44504")
));

while (true)
{
    await Task.Delay(1000);
}