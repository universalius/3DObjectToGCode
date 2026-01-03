using GCodes.Interfaces;

namespace GCodes.G;

public record GCoordinate(double? X = null, double? Y = null, double? Z = null);

public class G01LinearMove : IGCode
{
    public double? X { get; set; }
    public double? Y { get; set; }
    public double? Z { get; set; }

    public double? FeedRate { get; set; }

    public G01LinearMove(double? x = null, double? y = null, double? z = null, double? feedRate = null)
    {
        X = x;
        Y = y;
        Z = z;
        FeedRate = feedRate;
    }

    public G01LinearMove(GCoordinate Coord, double? feedRate = null)
    {
        X = Coord.X;
        Y = Coord.Y;
        Z = Coord.Z;
        FeedRate = feedRate;
    }

    public override string ToString()
    {
        var parts = new List<string> { "G01" };
        if (X.HasValue)
            parts.Add($"X{X.Value:F3}");
        if (Y.HasValue)
            parts.Add($"Y{Y.Value:F3}");
        if (Z.HasValue)
            parts.Add($"Z{Z.Value:F3}");
        if (FeedRate.HasValue)
            parts.Add($"F{FeedRate.Value:F5}");

        parts.Add("; (Linear move)");

        return string.Join(" ", parts);
    }
}
