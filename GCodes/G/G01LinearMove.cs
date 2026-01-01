using GCodes.Interfaces;

namespace GCodes.G
{
    public class G01LinearMove : IGCode
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }

        public double? FeedRate { get; set; }

        public G01LinearMove(double? x = null, double? y = null, double? z = null, double? feedRate)
        {
            X = x;
            Y = y;
            Z = z;
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

            parts.Add("; Rapid positioning");

            return string.Join(" ", parts);
        }
    }
}
