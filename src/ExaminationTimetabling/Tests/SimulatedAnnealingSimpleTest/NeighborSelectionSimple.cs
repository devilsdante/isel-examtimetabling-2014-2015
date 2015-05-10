using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.SimulatedAnnealingTest.Neighborhood;

namespace Tests.SimulatedAnnealingTest
{
    class NeighborSelectionSimple
    {
        public INeighborSimple BitSwap(SolutionSimple solution)
        {
            return new FlipBitNeighbor(solution, new Random((int)DateTime.Now.Ticks).Next(5));
        }
    }
}
