using GCodes.Interfaces;

namespace GCodes.G;

public enum SpindleRotationDirection
{
    Clockwise,
    CounterClockwise
}

public class G96ConstantSurfaceSpeed : IGCode
{
    public int CuttingSpeed { get; set; }
    public SpindleRotationDirection SpindleRotationDirection { get; set; }

    public G96ConstantSurfaceSpeed(int cuttingSpeed, SpindleRotationDirection spindleRotationDirection)
    {
        CuttingSpeed = cuttingSpeed;
        SpindleRotationDirection = spindleRotationDirection;
    }

    public override string ToString()
    {
        var directionCode = SpindleRotationDirection == SpindleRotationDirection.Clockwise ? "M03" : "M04";
        return $"G96 S{CuttingSpeed} {directionCode} ; (Set constant surface speed)";
    }
}
