using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public enum types { RANDOM, GUIDED1, GUIDED2 };

        private int room_change;
        private int period_change;
        private int period_room_change;
        private int room_swap;
        private int period_swap;
        private int period_room_swap;


        public SimulatedAnnealing()
        {
            evaluation = new EvaluationFunction();
            neighbor_selection = new NeighborSelection();
        }

        public Solution Exec(Solution solution, int TMax, int TMin, int loops, types type)
        {
            InitVals(type);

            for (int T = TMax; T > TMin; --T)
            {
                for (int loop = loops; loop > 0; --loop)
                {
                    INeighbor neighbor = null;
                    if(type == types.RANDOM)
                        neighbor = GenerateRandomNeighbor(solution);
                    else if (type == types.GUIDED1)
                        neighbor = GenerateGuidedNeighbor1(solution);
                    else
                        neighbor = GenerateGuidedNeighbor2(solution);

                    neighbor.fitness = (neighbor.fitness == -1) ? evaluation.Fitness(neighbor) : neighbor.fitness;
                    solution.fitness = (solution.fitness == -1) ? evaluation.Fitness(solution) : solution.fitness;

                    int DeltaE = neighbor.fitness - solution.fitness;

                    if (DeltaE <= 0)
                    {
                        solution = neighbor.Accept();
                        solution.fitness = neighbor.fitness;
                    }
                    else
                    {
                        double acceptance_probability = Math.Pow(Math.E, (-(float)DeltaE) / T);
                        double random = new Random((int)DateTime.Now.Ticks).NextDouble();

                        if (random <= acceptance_probability)
                        {
                            solution = neighbor.Accept();
                            solution.fitness = neighbor.fitness;
                        }

                        else
                            continue;
                    }
                    int dtf = evaluation.DistanceToFeasibility(solution);
                    if (dtf != 0)
                    {
                        throw new Exception("Distance to feasibility is not zero! DTF: " + dtf);
                    }
                }
            }
            return solution;
        }

        public Solution ExecTimer(Solution solution, int TMax, int TMin, long miliseconds, types type)
        {
            Stopwatch watch = Stopwatch.StartNew();
            InitVals(type);

            for (int T = TMax; T > TMin; T = TMax - (int)((watch.ElapsedMilliseconds * (TMax - TMin) / miliseconds) + TMin))
            {
                INeighbor neighbor = null;
                if (type == types.RANDOM)
                    neighbor = GenerateRandomNeighbor(solution);
                else if (type == types.GUIDED1)
                    neighbor = GenerateGuidedNeighbor1(solution);
                else
                    neighbor = GenerateGuidedNeighbor2(solution);
                
                neighbor.fitness = (neighbor.fitness == -1) ? evaluation.Fitness(neighbor) : neighbor.fitness;
                solution.fitness = (solution.fitness == -1) ? evaluation.Fitness(solution) : solution.fitness;

                int DeltaE = neighbor.fitness - solution.fitness;

                if (DeltaE <= 0)
                {
                    solution = neighbor.Accept();
                    solution.fitness = neighbor.fitness;
                }
                else
                {
                    double acceptance_probability = Math.Pow(Math.E, (-(float)DeltaE) / T);
                    double random = new Random((int)DateTime.Now.Ticks).NextDouble();

                    if (random <= acceptance_probability)
                    {
                        solution = neighbor.Accept();
                        solution.fitness = neighbor.fitness;
                    }
                        
                    else
                        continue;
                }
                int dtf = evaluation.DistanceToFeasibility(solution);
                if (dtf != 0)
                {
                    throw new Exception("Distance to feasibility is not zero! DTF: " + dtf);
                }
            }
            return solution;
        }

        private INeighbor GenerateRandomNeighbor(Solution solution)
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

        //Succeeded neighbor moviments are more likely to be used
        private INeighbor GenerateGuidedNeighbor1(Solution solution)
        {
            INeighbor to_return;
            int total = room_change + period_change + period_room_change + room_swap + period_swap + period_room_swap;
            int random = new Random((int)DateTime.Now.Ticks).Next(total);
            solution.fitness = (solution.fitness == -1) ? evaluation.Fitness(solution) : solution.fitness;

            while (true)
            {
                if (random < room_change)
                {
                    to_return = neighbor_selection.RoomChange(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            ++room_change;
                        break;
                    }
                }
                else if (random < room_change + period_change)
                {
                    to_return = neighbor_selection.PeriodChange(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            ++period_change;
                        break;
                    }
                }
                else if (random < room_change + period_change + period_room_change)
                {
                    to_return = neighbor_selection.PeriodRoomChange(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            ++period_room_change;
                        break;
                    }
                }
                else if (random < room_change + period_change + period_room_change + room_swap)
                {
                    to_return = neighbor_selection.RoomSwap(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            ++room_swap;
                        break;
                    }
                }
                else if (random < room_change + period_change + period_room_change + room_swap + period_swap)
                {
                    to_return = neighbor_selection.PeriodSwap(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            ++period_swap;
                        break;
                    }
                }
                else
                {
                    to_return = neighbor_selection.PeriodRoomSwap(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            ++period_room_swap;
                        break;
                    }
                }
            }
            return to_return;
        }

        //Succeeded neighbor moviments are less likely to be used
        private INeighbor GenerateGuidedNeighbor2(Solution solution)
        {
            INeighbor to_return;
            int total = room_change + period_change + period_room_change + room_swap + period_swap + period_room_swap;
            int random = new Random((int)DateTime.Now.Ticks).Next(total);
            solution.fitness = (solution.fitness == -1) ? evaluation.Fitness(solution) : solution.fitness;

            while (true)
            {
                if (random < room_change)
                {
                    to_return = neighbor_selection.RoomChange(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            --room_change;
                        break;
                    }
                }
                else if (random < room_change + period_change)
                {
                    to_return = neighbor_selection.PeriodChange(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            --period_change;
                        break;
                    }
                }
                else if (random < room_change + period_change + period_room_change)
                {
                    to_return = neighbor_selection.PeriodRoomChange(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            --period_room_change;
                        break;
                    }
                }
                else if (random < room_change + period_change + period_room_change + room_swap)
                {
                    to_return = neighbor_selection.RoomSwap(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            --room_swap;
                        break;
                    }
                }
                else if (random < room_change + period_change + period_room_change + room_swap + period_swap)
                {
                    to_return = neighbor_selection.PeriodSwap(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            --period_swap;
                        break;
                    }
                }
                else
                {
                    to_return = neighbor_selection.PeriodRoomSwap(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            --period_room_swap;
                        break;
                    }
                }
            }
            return to_return;
        }

        private void InitVals(types type)
        {
            if (type == types.RANDOM)
            {
                room_change = 0;
                period_change = 0;
                period_room_change = 0;
                room_swap = 0;
                period_swap = 0;
                period_room_swap = 0;
            }
            else
            {
                room_change = 100000;
                period_change = 100000;
                period_room_change = 100000;
                room_swap = 100000;
                period_swap = 100000;
                period_room_swap = 100000;
            }
        }
    }
}
