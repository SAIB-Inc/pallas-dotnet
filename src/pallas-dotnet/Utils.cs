using System.Numerics;
using System.Text.Json;
using PallasDotnet.Models;

namespace PallasDotnet;

public class Utils
{
    public static Point MapPallasPoint(PallasDotnetN2c.PallasDotnetN2c.Point rsPoint)
        => new(rsPoint.slot, new Hash([.. rsPoint.hash]));
}