using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodes.M;

public static class M05SpindleStop
{
    public static string GetCode()
    {
        return "M05 ; Stop spindle";
    }
}
