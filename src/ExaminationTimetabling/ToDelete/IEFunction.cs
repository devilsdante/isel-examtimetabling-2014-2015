using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Neighborhood;

namespace ToDelete
{
    public interface IEFunction <T> where T : class, ISolution
    {
        int DistanceToFeasibility(T solution);
        int Fitness(T solution);
        bool IsValid(T solution);
        int Fitness(INeighbor neighbor);
        int DistanceToFeasibility(INeighbor neighbor);
    }
}
