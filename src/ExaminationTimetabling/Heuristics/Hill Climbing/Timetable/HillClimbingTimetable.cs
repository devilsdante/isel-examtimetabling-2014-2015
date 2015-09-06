using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Solution;
using DAL.Models.Solution.Timetabling;
using Tools.EvaluationFunction;
using Tools.EvaluationFunction.Timetable;
using Tools.Neighborhood;
using Tools.NeighborSelection.Timetable;

namespace Heuristics.Hill_Climbing.Timetable
{
    public class HillClimbingTimetable : HillClimbing
    {
        protected override IEvaluationFunction evaluation_function { get; set; }
        private readonly NeighborSelectionTimetable neighbor_selection_timetable;

        public static int type_random = 0;

        private readonly Random random;
        public long generated_neighbors;
        private int total_neighbor_operators;

        public HillClimbingTimetable()
        {
            evaluation_function = new EvaluationFunctionTimetable();
            neighbor_selection_timetable = new NeighborSelectionTimetable();
            random = new Random(Guid.NewGuid().GetHashCode());
            total_neighbor_operators = 6;
        }

        protected INeighbor GenerateNeighbor(Solution solution, int type)
        {
            generated_neighbors++;

            //if(generated_neighbors % 1000 == 0)
            //    Console.WriteLine(generated_neighbors);
            if (type == type_random)
                return GenerateRandomNeighbor(solution);;
            return GenerateRandomNeighbor(solution);
        }

        private INeighbor GenerateRandomNeighbor(Solution solution)
        {
            INeighbor to_return;
            int val = random.Next(total_neighbor_operators);
            do
            {
                if (val == 0)
                    to_return = neighbor_selection_timetable.RoomChange(solution);
                else if (val == 1)
                    to_return = neighbor_selection_timetable.PeriodChange(solution);
                else if (val == 2)
                    to_return = neighbor_selection_timetable.PeriodRoomChange(solution);
                else if (val == 3)
                    to_return = neighbor_selection_timetable.RoomSwap(solution);
                else if (val == 4)
                    to_return = neighbor_selection_timetable.PeriodSwap(solution);
                else
                    to_return = neighbor_selection_timetable.PeriodRoomSwap(solution);
            } while (to_return == null);

            return to_return;
        }

        protected override INeighbor GenerateNeighbor(ISolution solution, int type)
        {
            return GenerateNeighbor((Solution)solution, type);
        }
    }
}
