using System;
using System.Diagnostics;
using DAL.Models;
using Tools;
using Tools.Neighborhood;

namespace Heuristics.SimulatedAnnealing
{
    public class SimulatedAnnealing
    {
        private readonly EvaluationFunction evaluation;
        private readonly NeighborSelection neighbor_selection;
        private ICoolingSchedule cooling_schedule;

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
            cooling_schedule = new CoolingScheduleLinear(TMax, TMin, 1);
            InitVals(type);

            for (double T = TMax; T > TMin; T = cooling_schedule.G(T))
            {
                for (int loop = loops; loop > 0; --loop)
                {
                    INeighbor neighbor = GenerateNeighbor(solution, type);

                    neighbor.fitness = (neighbor.fitness == -1) ? evaluation.Fitness(neighbor) : neighbor.fitness;
                    solution.fitness = (solution.fitness == -1) ? evaluation.Fitness(solution) : solution.fitness;

                    double DeltaE = neighbor.fitness - solution.fitness;

                    if (DeltaE <= 0)
                    {
                        solution = neighbor.Accept();
                        solution.fitness = neighbor.fitness;
                    }
                    else
                    {
                        double acceptance_probability = Math.Pow(Math.E, (-DeltaE) / T);
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

        public Solution ExecTimer(Solution solution, long miliseconds, types type)
        {
            Stopwatch watch = Stopwatch.StartNew();
            InitVals(type);

            while (watch.ElapsedMilliseconds < miliseconds)
            {
                INeighbor neighbor = GenerateNeighbor(solution, type);

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
                    double acceptance_probability = Math.Pow(Math.E, (-(float)DeltaE*miliseconds) / (watch.ElapsedMilliseconds));
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

        public Solution ExecLinearTimer(Solution solution, int TMax, int TMin, long miliseconds, types type)
        {
            Stopwatch watch = Stopwatch.StartNew();
            InitVals(type);

            for (int T = TMax; T > TMin; T = TMax - (int)((watch.ElapsedMilliseconds * (TMax - TMin) / miliseconds) + TMin))
            {
                INeighbor neighbor = GenerateNeighbor(solution, type);
                
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

        private INeighbor GenerateNeighbor(Solution solution, types type)
        {
            if (type == types.RANDOM)
                return GenerateRandomNeighbor(solution);
            if (type == types.GUIDED1)
                return GenerateGuidedNeighbor1(solution);
            return GenerateGuidedNeighbor2(solution);
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
                            room_change = room_change < 10 ? room_change + 1 : room_change;
                        else
                            room_change = 1;
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
                            period_change = period_change < 10 ? period_change + 1 : period_change;
                        else
                            period_change = 1;
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
                            period_room_change = period_room_change < 10 ? period_room_change + 1 : period_room_change;
                        else
                            period_room_change = 1;
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
                            room_swap = room_swap < 10 ? room_swap + 1 : room_swap;
                        else
                            room_swap = 1;
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
                            period_swap = period_swap < 10 ? period_swap + 1 : period_swap;
                        else
                            period_swap = 1;
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
                            period_room_swap = period_room_swap < 10 ? period_room_swap + 1 : period_room_swap;
                        else
                            period_room_swap = 1;
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
                            room_change = room_change > 1 ? room_change + 1 : room_change;
                        else
                            room_change = 10;
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
                            period_change = period_change > 1 ? period_change + 1 : period_change;
                        else
                            period_change = 10;
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
                            period_room_change = period_room_change > 1 ? period_room_change + 1 : period_room_change;
                        else
                            period_room_change = 10;
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
                            room_swap = room_swap > 1 ? room_swap + 1 : room_swap;
                        else
                            room_swap = 10;
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
                            period_swap = period_swap > 1 ? period_swap + 1 : period_swap;
                        else
                            period_swap = 10;
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
                            period_room_swap = period_room_swap > 1 ? period_room_swap + 1 : period_room_swap;
                        else
                            period_room_swap = 10;
                        break;
                    }
                }
            }
            return to_return;
        }

        private void InitVals(types type)
        {
            if (type == types.GUIDED1)
            {
                room_change = 1;
                period_change = 1;
                period_room_change = 1;
                room_swap = 1;
                period_swap = 1;
                period_room_swap = 1;
            }
            else
            {
                room_change = 10;
                period_change = 10;
                period_room_change = 10;
                room_swap = 10;
                period_swap = 10;
                period_room_swap = 10;
            }
        }
    }
}
