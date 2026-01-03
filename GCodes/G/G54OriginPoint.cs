using GCodes.Interfaces;

namespace GCodes;

public class G54OriginPoint : IGCode
{
    public override string ToString()
    {
        return "G54 ; (Set work coordinate system to G54 origin point)";
    }
}
