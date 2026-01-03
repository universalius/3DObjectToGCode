using GCodes.Interfaces;
using GCodes.N;
using System.Text;

namespace GCodes.Extensions;

public static class GCodes
{
    public static string ToText(this IEnumerable<IGCode> gCodes)
    {
        var sb = new StringBuilder();
        var codes = gCodes.ToList();
        for (int i = 0; i < codes.Count; i++)
        {
            var code = codes[i];
            if (code is NLine && (code as NLine).LineNumber != 1)
            {
                var nextCode = i + 1 < codes.Count ? codes[i + 1] : null;
                sb.AppendLine($"{code.ToString()} {(nextCode != null ? nextCode.ToString() : ";")}");
                i++;
                continue;
            }
            sb.AppendLine(code.ToString());
        }

        return sb.ToString();
    }
}