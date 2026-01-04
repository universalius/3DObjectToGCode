using GCodes.Interfaces;

namespace GCodes.M;

public class M05SpindleStop : IGCode
{
    public M05SpindleStop()
    {
    }

    public override string ToString()
    {
        return "M05 ; (Stop spindle)";
    }
}
