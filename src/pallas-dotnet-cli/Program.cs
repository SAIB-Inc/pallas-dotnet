﻿using System.Diagnostics;
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
static async void ExecuteN2cProtocol()
{
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

    await foreach (NextResponse? nextResponse in nodeClient.StartChainSyncAsync(new Point(
        57762827,
        new Hash("7063cb55f1e55fd80aca1ee582a7b489856d704b46e213e268bad14a56f09f35")
    )))
    {
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
    }
}

// N2N Protocol Implementation
static async void ExecuteN2nProtocol()
{
    N2nClient? n2nClient = new();
    Point? tip = await n2nClient.ConnectAsync("localhost:31000", NetworkMagic.PREVIEW);

    if (tip is not null)
    {
        Console.WriteLine($"Tip: {tip.HashHex}");
    }

    n2nClient.Disconnected += (sender, args) =>
    {
        ConsoleHelper.WriteLine($"Disconnected ", ConsoleColor.DarkRed);
    };

    n2nClient.Reconnected += (sender, args) =>
    {
        ConsoleHelper.WriteLine($"Reconnected ", ConsoleColor.DarkGreen);
    };

    await foreach (NextResponse? nextResponse in n2nClient.StartChainSyncAsync(new Point(
        57751092,
        new Hash("d924387268359420990f8e71b9e89f0e6e9fa640ccd69acc5bf410ea5911366d")
    )))
    {
        if (nextResponse.Action == NextResponseAction.Await)
        {
            Console.WriteLine("Awaiting...");
        }
        else if (nextResponse.Action == NextResponseAction.RollBack || nextResponse.Action == NextResponseAction.RollForward)
        {
            string action = nextResponse.Action == NextResponseAction.RollBack ? "Rolling back..." : "Rolling forward...";

            Console.WriteLine(action);
            Console.WriteLine($"Slot: {nextResponse.Tip.Slot} Hash: {nextResponse.Tip.Hash}");

            Console.WriteLine("Block:");
            string cborHex = Convert.ToHexString(nextResponse.BlockCbor);
            Console.WriteLine(cborHex);

            Console.WriteLine("--------------------------------------------------------------------------------");
        }
    }
}

await Task.Run(ExecuteN2cProtocol);
// await Task.Run(ExecuteN2nProtocol);

while (true)
{
    await Task.Delay(1000);
}