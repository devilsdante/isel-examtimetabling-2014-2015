namespace Heuristics.SimulatedAnnealing.CoolingSchedule
{
    public class CoolingScheduleGeometric : ICoolingSchedule
    {
        public double TMax { get; set; }

        public double TMin { get; set; }

        public double rate { get; set; }

        public int span { get; set; }

        public CoolingScheduleGeometric(double rate)
        {
            this.rate = rate;
        }

        public double G(double T)
        {
            span++;
            return T*rate;
        }
    }
}
