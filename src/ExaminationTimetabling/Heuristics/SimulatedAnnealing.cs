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
using Tools.Neighborhood;

namespace Heuristics
{
    public class SimulatedAnnealing
    {

        public FeasibilityTester feasibility_tester;
        private Solution solution;
        private readonly EvaluationFunction evaluation;
        private readonly NeighborSelection neighbor_selection;


        public SimulatedAnnealing()
        {
            evaluation = new EvaluationFunction();
            neighbor_selection = new NeighborSelection();
        }

        public Solution Exec(Solution solution, int TMax, int TMin)
        {
            int T = TMax;
            
            while (T > TMin)
            {
                int loops = 1095;
                while (loops > 0)
                {
                    //Console.WriteLine("DTF " + evaluation.DistanceToFeasibility(solution));
                    INeighbor neighbor = GenerateNeighbor(solution);

                    int DeltaE = evaluation.Fitness(neighbor) - evaluation.Fitness(solution);

                    if (DeltaE <= 0)
                        solution = neighbor.Accept();
                        
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
                            solution = neighbor.Accept();
                    }
                    int dtf = evaluation.DistanceToFeasibility(solution);
                    if (dtf != 0)
                    {
                        throw new Exception("Distance to feasibility is not zero! DTF: "+dtf);
                    }
                    loops--;
                }
                T--;
            }
            return solution;
        }

        private INeighbor GenerateNeighbor(Solution solution)
        {
            INeighbor to_return;
            int random = new Random().Next(6);
            do
            {
                if(random == 0)
                    to_return = neighbor_selection.RoomChange(solution);
                else if(random == 1)
                    to_return = neighbor_selection.PeriodChange(solution);
                else if (random == 2)
                    to_return = neighbor_selection.PeriodRoomChange(solution);
                else if (random == 3)
                    to_return = neighbor_selection.RoomSwap(solution);
                else if (random == 4)
                    to_return = neighbor_selection.PeriodSwap(solution);
                else
                    to_return = neighbor_selection.PeriodRoomSwap(solution);
            } while (to_return == null);
            return to_return;
        }
    }
}
