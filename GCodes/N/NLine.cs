using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodes.N;

public class NLine
{
    public int LineNumber { get; set; }
    public NLine(int lineNumber)
    {
        LineNumber = lineNumber;
    }
    public override string ToString()
    {
        return $"N{LineNumber} ;";
    }
}
