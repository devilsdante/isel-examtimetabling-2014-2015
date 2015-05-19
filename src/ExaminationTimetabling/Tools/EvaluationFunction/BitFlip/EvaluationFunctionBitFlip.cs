using System;
using DAL.Models.Solution;
using DAL.Models.Solution.BitFlip;
using Tools.Neighborhood;

namespace Tools.EvaluationFunction.BitFlip
{
    public class EvaluationFunctionBitFlip : IEvaluationFunction
    {
        public int Fitness(ISolution solution)
        {
            return Fitness((SolutionBitFlip) solution);
        }

        public bool IsValid(ISolution solution)
        {
            return true;
        }

        public int Fitness(INeighbor neighbor)
        {
            ISolution new_solution = neighbor.Accept();
            int fitness = Fitness(new_solution);
            neighbor.Reverse();
            return fitness;
        }

        public int Fitness(SolutionBitFlip solution)
        {
            int value = Convert.ToInt16(solution.bits_string,2);
            return value*value*value - 60*value*value + 900*value + 100;
        }

        public int DistanceToFeasibility(INeighbor neighbor)
        {
            return 0;
        }

        public int DistanceToFeasibility(ISolution solution)
        {
            return 0;
        }
    }
}
