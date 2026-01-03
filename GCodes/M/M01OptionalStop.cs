using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodes.M;

public static class M01OptionalStop
{
    public static string GetCode()
    {
        return "M01 ; (Optional stop)";
    }
}
