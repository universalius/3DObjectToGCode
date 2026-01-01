using GCodes.Interfaces;

namespace GCodes;

public class G53MachineOriginPoint : IGCode
{
    public double? X { get; set; }
    public double? Y { get; set; }
    public double? Z { get; set; }

    public G53MachineOriginPoint(double? x, double? y, double? z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        var parts = new List<string> { "G53" };
        if (X.HasValue)
            parts.Add($"X{X.Value:F3}");
        if (Y.HasValue)
            parts.Add($"Y{Y.Value:F3}");
        if (Z.HasValue)
            parts.Add($"Z{Z.Value:F3}");

        parts.Add("; Rapid positioning by machine coords");

        return string.Join(" ", parts);
    }
}
