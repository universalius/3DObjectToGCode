using GCodes.Interfaces;
using System.Threading.Channels;
namespace GCodes;

public class G40CancelCutterCompensation : IGCode
{
    public override string ToString()
    {
        return "G40 ; Cancel cutter radius compensation";
    }
}
