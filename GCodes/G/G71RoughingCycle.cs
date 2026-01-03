using GCodes.Interfaces;

namespace GCodes.G
{
    public class G71RoughingCycle : IGCode
    {
        public double DepthOfCut { get; set; }
        public double RetractValue { get; set; }
        public int FirstLineOfSubProgramm { get; set; }
        public int LastLineOfSubProgramm { get; set; }
        public double FinishCutReserveX { get; set; }
        public double FinishCutReserveZ { get; set; }
        public double FeedRate { get; set; }

        public G71RoughingCycle(double depthOfCut, double retractValue, int firstLineOfSubProgramm, int lastLineOfSubProgramm, 
            double finishCutReserveX, double finishCutReserveZ, double feedRate)
        {
            DepthOfCut = depthOfCut;
            RetractValue = retractValue;
            FirstLineOfSubProgramm = firstLineOfSubProgramm;
            LastLineOfSubProgramm = lastLineOfSubProgramm;
            FinishCutReserveX = finishCutReserveX;
            FinishCutReserveZ = finishCutReserveZ;
            FeedRate = feedRate;
        }

        public override string ToString()
        {
            return $"G71 U{DepthOfCut:F3} R{RetractValue:F3} ;\n" +
                   $"G71 P{FirstLineOfSubProgramm} Q{LastLineOfSubProgramm} " +
                   $"U{FinishCutReserveX:F3} W{FinishCutReserveZ:F3} " +
                   $"F{FeedRate:F5} ; (Roughing cycle)";
        }
    }
}
