using GCodes.Interfaces;

namespace GCodes.G
{
    public class G00RapidTravel: IGCode
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }

        public G00RapidTravel(double? x = null, double? y = null, double? z = null)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            var parts = new List<string> { "G00" };
            if (X.HasValue)
                parts.Add($"X{X.Value:F3}");
            if (Y.HasValue)
                parts.Add($"Y{Y.Value:F3}");
            if (Z.HasValue)
                parts.Add($"Z{Z.Value:F3}");

            parts.Add("; Rapid positioning");

            return string.Join(" ", parts);
        }
    }
}
