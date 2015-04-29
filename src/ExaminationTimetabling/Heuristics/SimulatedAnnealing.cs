using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Business;
using DAL;
using DAL.Models;
using Tools;

namespace Heuristics
{
    public class SimulatedAnnealing
    {

        public FeasibilityTester feasibility_tester;
        private Solution solution;
        private readonly Examinations examinations;
        private readonly PeriodHardConstraints period_hard_constraints;
        private readonly Periods periods;
        private readonly RoomHardConstraints room_hard_constraints;
        private readonly Rooms rooms;
        private readonly ModelWeightings model_weightings;
        private readonly EvaluationFunction evaluation;
        private readonly NeighborSelection neighbor_selection;


        public SimulatedAnnealing(Examinations examinations, PeriodHardConstraints period_hard_constraints, Periods periods, RoomHardConstraints room_hard_constraints,
            Rooms rooms, ModelWeightings model_weightings)
        {
            this.examinations = examinations;
            this.period_hard_constraints = period_hard_constraints;
            this.periods = periods;
            this.room_hard_constraints = room_hard_constraints;
            this.rooms = rooms;
            this.model_weightings = model_weightings;
            evaluation = new EvaluationFunction(examinations, period_hard_constraints, room_hard_constraints, rooms, periods, model_weightings);
            neighbor_selection = new NeighborSelection(examinations, period_hard_constraints, room_hard_constraints, rooms, periods);
        }

        public Solution Exec(Solution solution, int TMax, int TMin)
        {
            int T = TMax;

            while (T > TMin)
            {
                int loops = 100;
                while (loops > 0)
                {
                    Solution neighbor = GenerateNeighbor(solution);

                    int DeltaE = evaluation.Fitness(neighbor) - evaluation.Fitness(solution);

                    if (DeltaE <= 0)
                        solution = neighbor;
                        
                    else
                    {
                        double acceptance_probability = Math.Pow(Math.E, (-(float)DeltaE)/T);
                        double random = new Random((int) DateTime.Now.Ticks).NextDouble();

                        if (T % 1 == 0 || T < 10)
                        {
                            //Console.WriteLine("so-----------------------" + evaluation.Fitness(solution));
                            //Console.WriteLine("acceptance_probability:--" + acceptance_probability*100);
                        }

                        if (random <= acceptance_probability)
                            solution = neighbor;
                    }
                    loops--;
                }
                T--;
            }
            return solution;
        }

        private Solution GenerateNeighbor(Solution solution)
        {
            Solution to_return = null;
            do
            {
                to_return = neighbor_selection.RoomMove(solution);
            } while (to_return == null);
            return to_return;
        }
    }
}
