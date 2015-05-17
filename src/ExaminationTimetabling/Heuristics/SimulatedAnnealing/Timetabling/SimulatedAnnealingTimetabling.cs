﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using Tools;
using Tools.Neighborhood;

namespace Heuristics.SimulatedAnnealing.Timetabling
{
    public class SimulatedAnnealingTimetabling : SimulatedAnnealing
    {
        protected override IEvaluationFunction evaluation_function { get; set; }
        private readonly NeighborSelection neighbor_selection;

        public static int type_random = 0;
        public static int type_guided1 = 1;
        public static int type_guided2 = 2;

        private int room_change;
        private int period_change;
        private int period_room_change;
        private int room_swap;
        private int period_swap;
        private int period_room_swap;

        public SimulatedAnnealingTimetabling()
        {
            evaluation_function = new EvaluationFunctionTimetabling();
            neighbor_selection = new NeighborSelection();
        }

        protected INeighbor GenerateNeighbor(Solution solution, int type)
        {
            if (type == type_random)
                return GenerateRandomNeighbor(solution);
            if (type == type_guided1)
                return GenerateGuidedNeighbor1(solution);
            return GenerateGuidedNeighbor2(solution);
        }

        private INeighbor GenerateRandomNeighbor(Solution solution)
        {
            INeighbor to_return = null;
            int random = new Random().Next(3);
            do
            {
                if (random == 0)
                    to_return = neighbor_selection.RoomChange(solution);
                else if (random == 1)
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
            solution.fitness = (solution.fitness == -1) ? evaluation_function.Fitness(solution) : solution.fitness;

            while (true)
            {
                if (random < room_change)
                {
                    to_return = neighbor_selection.RoomChange(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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
            solution.fitness = (solution.fitness == -1) ? evaluation_function.Fitness(solution) : solution.fitness;

            while (true)
            {
                if (random < room_change)
                {
                    to_return = neighbor_selection.RoomChange(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
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

        protected override INeighbor GenerateNeighbor(ISolution solution, int type)
        {
            return GenerateNeighbor((Solution) solution, type);
        }

        protected override void InitVals(int type)
        {
            if (type == type_guided1)
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
