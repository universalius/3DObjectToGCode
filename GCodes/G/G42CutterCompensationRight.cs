using GCodes.Interfaces;
using System.Diagnostics.Metrics;
using System.Threading.Channels;
namespace GCodes;

public class G42CutterCompensationRight : IGCode
{
    public override string ToString()
    {
        return "G42 ; Enable cutter radius compensation right";
    }
}
