using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.SimulatedAnnealingTest
{
    public interface INeighborSimple
    {
        int fitness { get; set; }
        SolutionSimple Accept();
        SolutionSimple Reverse();
    }
}
