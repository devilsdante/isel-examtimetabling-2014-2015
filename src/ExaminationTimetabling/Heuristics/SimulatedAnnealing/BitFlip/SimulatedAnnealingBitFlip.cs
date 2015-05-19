using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Solution;
using DAL.Models.Solution.BitFlip;
using Tools;
using Tools.EvaluationFunction;
using Tools.EvaluationFunction.BitFlip;
using Tools.Neighborhood;
using Tools.NeighborSelection.BitFlip;

namespace Heuristics.SimulatedAnnealing.BitFlip
{
    public class SimulatedAnnealingBitFlip : SimulatedAnnealing
    {
        protected override IEvaluationFunction evaluation_function { get; set; }
        private readonly NeighborSelectionBitFlip neighbor_selection;

        public int maximum = -1;

        public SimulatedAnnealingBitFlip()
        {
            evaluation_function = new EvaluationFunctionBitFlip();
            neighbor_selection = new NeighborSelectionBitFlip();
        }

        protected override INeighbor GenerateNeighbor(ISolution solution, int type)
        {
            //return neighbor_selection.BitSwap((SolutionBitFlip)solution);
            INeighbor neighbor = neighbor_selection.BitSwap((SolutionBitFlip)solution);
            int value = evaluation_function.Fitness(neighbor);
            maximum = maximum < value ? value : maximum;
            return neighbor;
        }

        protected override void InitVals(int type)
        {}
    }
}
