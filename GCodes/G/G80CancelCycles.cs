using GCodes.Interfaces;

namespace GCodes;

public class G80CancelCycles : IGCode
{
    public override string ToString()
    {
        return "G80 ; (Cancel canned cycle)";
    }
}
