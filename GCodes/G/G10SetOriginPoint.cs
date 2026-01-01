using GCodes.Interfaces;

namespace GCodes;

public enum G10OffsetP
{
    P1 = 1,// G54
    P2 = 2,// G55
    P3 = 3,
    P4 = 4,
    P5 = 5,
    P6 = 6,// G59
}

public class G10SetOriginPoint : IGCode
{
    public double? X { get; set; }
    public double? Y { get; set; }
    public double? Z { get; set; }
    public G10OffsetP P { get; set; }

    public G10SetOriginPoint(G10OffsetP p, double? x, double? y, double? z)
    {
        X = x;
        Y = y;
        Z = z;
        P = p;
    }

    public override string ToString()
    {
        var parts = new List<string> { $"G10 L2 {P.ToString()}" };
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
