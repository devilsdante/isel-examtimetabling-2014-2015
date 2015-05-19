namespace Heuristics.SimulatedAnnealing.CoolingSchedule
{
    public class CoolingScheduleLinear : ICoolingSchedule
    {
        public double TMax { get; set; }

        public double TMin { get; set; }

        public double rate { get; set; }

        public int span { get; set; }

        public CoolingScheduleLinear(double TMax, double TMin, double rate)
        {
            this.TMax = TMax;
            this.TMin = TMin;
            this.rate = rate;
            this.span = 0;
        }

        public double G(double T)
        {
            return TMax - span++ * rate;
            //return --T;
        }
    }
}
