namespace PallasDotnet.Models;

public class Address(byte[] addressBytes)
{
    public byte[] Raw => addressBytes;
    
    public string ToBech32()
        => PallasDotnetN2c.PallasDotnetN2c
                .AddressBytesToBech32(addressBytes);
}