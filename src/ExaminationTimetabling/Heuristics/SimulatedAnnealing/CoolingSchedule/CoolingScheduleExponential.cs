using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristics.SimulatedAnnealing.CoolingSchedule
{
    public class CoolingScheduleExponential : ICoolingSchedule
    {
        public double TMax { get; set; }

        public double TMin { get; set; }

        public double rate { get; set; }

        public int span { get; set; }

        public CoolingScheduleExponential(double rate, double TMax)
        {
            this.TMax = TMax;
            this.rate = rate;
            this.span = 0;
        }

        public double G(double T)
        {
            return TMax * Math.Pow(Math.E, -rate * span++);
        }
    }
}
