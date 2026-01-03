using GCodes.Interfaces;

namespace GCodes;

public class G21MetricUnits : IGCode
{
    public override string ToString()
    {
        return "G21 ; (Set units to millimeters)";
    }
}
