using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.SimulatedAnnealingTest.Neighborhood
{
    class FlipBitNeighbor : INeighborSimple
    {
        public int fitness{ get; set; }
        public SolutionSimple solution;
        public int bit;
        public char value;

        public FlipBitNeighbor(SolutionSimple solution, int bit)
        {
            this.solution = solution;
            this.value = solution.solution[bit];
            this.fitness = -1;
            this.bit = bit;
        }

        public SolutionSimple Accept()
        {
            StringBuilder sb = new StringBuilder(solution.solution);
            sb[bit] = value == '0' ? '1' : '0';
            solution.solution = sb.ToString();
            return solution;
        }

        public SolutionSimple Reverse()
        {
            StringBuilder sb = new StringBuilder(solution.solution);
            sb[bit] = value;
            solution.solution = sb.ToString();
            return solution;
        }
    }
}
