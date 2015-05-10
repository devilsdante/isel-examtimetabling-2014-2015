using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using Tools.Neighborhood;

namespace Tests.SimulatedAnnealingTest
{
    public class EvaluationFunctionSimple
    {

        public int Fitness(INeighborSimple neighbor)
        {
            SolutionSimple new_solution = neighbor.Accept();
            int fitness = Fitness(new_solution);
            neighbor.Reverse();
            return fitness;
        }


        public int Fitness(SolutionSimple solution)
        {
            int value = Convert.ToInt16(solution.solution,2);
            return value*value*value - 60*value*value + 900*value + 100;
        }

        public int DistanceToFeasibility(SolutionSimple solution)
        {
            return 0;
        }

        public int DistanceToFeasibility(INeighborSimple neighbor)
        {
            return 0;
        }
    }
}
