using GCodes.Interfaces;

namespace GCodes.G;

public class G50MaxRpm : IGCode
{
    public int MaxSpindleSpeed { get; set; }

    public G50MaxRpm(int maxSpindleSpeed)
    {
        MaxSpindleSpeed = maxSpindleSpeed;
    }

    public override string ToString()
    {
        return $"G50 S{MaxSpindleSpeed} ; Set maximum spindle speed";
    }
}
