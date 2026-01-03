using GCodes.Interfaces;

namespace GCodes.N;

public class NLine : IGCode
{
    public int LineNumber { get; set; }
    public NLine(int lineNumber)
    {
        LineNumber = lineNumber;
    }
    public override string ToString()
    {
        return $"N{LineNumber}{(LineNumber == 1 ? " ;" : string.Empty)}";
    }
}
