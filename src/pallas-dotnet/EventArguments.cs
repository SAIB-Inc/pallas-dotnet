using PallasDotnet.Models;

namespace PallasDotnet.EventArguments;

public class ChainSyncNextResponseEventArgs(NextResponse nextResponse) : EventArgs
{
    public NextResponse NextResponse { get; } = nextResponse;
}

public class GetTipEventArgs(Point point) : EventArgs
{
    public Point Point { get; } = point;
}