using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Neighborhood;

namespace ToDelete
{
    public interface IEFunction
    {
        int DistanceToFeasibility(ISolution solution);
        int Fitness(ISolution solution);
        bool IsValid(ISolution solution);
        int Fitness(INeighbor neighbor);
        int DistanceToFeasibility(INeighbor neighbor);
    }
}
