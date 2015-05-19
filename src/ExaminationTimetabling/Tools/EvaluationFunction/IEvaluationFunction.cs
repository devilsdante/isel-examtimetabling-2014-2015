using DAL.Models.Solution;
using Tools.Neighborhood;

namespace Tools.EvaluationFunction
{
    public interface IEvaluationFunction
    {
        int DistanceToFeasibility(ISolution solution);
        int Fitness(ISolution solution);
        bool IsValid(ISolution solution);
        int Fitness(INeighbor neighbor);
        int DistanceToFeasibility(INeighbor neighbor);
    }
}
