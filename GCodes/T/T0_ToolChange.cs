using GCodes.Interfaces;

namespace GCodes.T;

public class T0_ToolChange : IGCode
{
    public int ToolNumber { get; set; }
    public int CorrectionNumber { get; set; }


    public T0_ToolChange(int toolNumber, int correctionNumber)
    {
        ToolNumber = toolNumber;
        CorrectionNumber = correctionNumber;
    }

    public override string ToString()
    {
        return $"T0{ToolNumber}0{CorrectionNumber} ; (Tool change)";
    }
}
