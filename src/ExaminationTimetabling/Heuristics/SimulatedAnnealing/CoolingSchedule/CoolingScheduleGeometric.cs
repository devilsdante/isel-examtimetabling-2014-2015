﻿namespace Heuristics.SimulatedAnnealing
{
    public class CoolingScheduleGeometric : ICoolingSchedule
    {
        public double TMax { get; set; }

        public double TMin { get; set; }

        public double rate { get; set; }

        public int span { get; set; }

        public double G(double T)
        {
            span++;
            return T*rate;
        }
    }
}
