using GCodes.Interfaces;
using System.Diagnostics.Metrics;
using System.Threading.Channels;
namespace GCodes;

public class G41CutterCompensationLeft : IGCode
{
    public override string ToString()
    {
        return "G41 ; (Enable cutter radius compensation left)";
    }
}
