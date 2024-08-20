namespace PallasDotnet;

public record NetworkMagic
{
    public static ulong MAINNET => PallasDotnetN2c.PallasDotnetN2c.MainnetMagic();
    public static ulong TESTNET => PallasDotnetN2c.PallasDotnetN2c.TestnetMagic();
    public static ulong PREVIEW => PallasDotnetN2c.PallasDotnetN2c.PreviewMagic();
    public static ulong PREPRODUCTION => PallasDotnetN2c.PallasDotnetN2c.PreProductionMagic();
}