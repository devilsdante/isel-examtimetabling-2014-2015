using System;

namespace Heuristics.SimulatedAnnealing.CoolingSchedule
{
    public class CoolingScheduleLogarithmic : ICoolingSchedule
    {
        public double TMax { get; set; }

        public double TMin { get; set; }

        public double rate { get; set; }

        public int span { get; set; }

        public double G(double T)
        {
            return TMax/Math.Log10(span++);
        }
    }
}
