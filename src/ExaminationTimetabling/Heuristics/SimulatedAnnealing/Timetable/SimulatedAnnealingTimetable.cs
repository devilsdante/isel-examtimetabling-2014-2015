using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using DAL.Models.Solution;
using DAL.Models.Solution.Timetabling;
using Tools;
using Tools.EvaluationFunction;
using Tools.EvaluationFunction.Timetable;
using Tools.Neighborhood;
using Tools.NeighborSelection.Timetable;

namespace Heuristics.SimulatedAnnealing.Timetable
{
    public class SimulatedAnnealingTimetable : SimulatedAnnealing
    {
        protected override IEvaluationFunction evaluation_function { get; set; }
        private readonly NeighborSelectionTimetable neighbor_selection_timetable;

        public static int type_random = 0;
        public static int type_guided1 = 1;
        public static int type_guided2 = 2;

        private int room_change;
        private int period_change;
        private int period_room_change;
        private int room_swap;
        private int period_swap;
        private int period_room_swap;

        public int generated_neighbors;

        private Random random;

        public SimulatedAnnealingTimetable()
        {
            evaluation_function = new EvaluationFunctionTimetable();
            neighbor_selection_timetable = new NeighborSelectionTimetable();
            random = new Random();
        }

        protected INeighbor GenerateNeighbor(Solution solution, int type)
        {
            generated_neighbors++;

            //if(generated_neighbors % 1000 == 0)
            //    Console.WriteLine(generated_neighbors);
            if (type == type_random)
                return GenerateRandomNeighbor(solution);
            if (type == type_guided1)
                return GenerateGuidedNeighbor1(solution);
            return GenerateGuidedNeighbor2(solution);

        }

        private INeighbor GenerateRandomNeighbor(Solution solution)
        {
            INeighbor to_return;
            int val = random.Next(6);
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

        //Succeeded neighbor moviments are more likely to be used
        private INeighbor GenerateGuidedNeighbor1(Solution solution)
        {
            INeighbor to_return;
            int total = room_change + period_change + period_room_change + room_swap + period_swap + period_room_swap;
            int random = new Random((int)DateTime.Now.Ticks).Next(total);
            solution.fitness = (solution.fitness == -1) ? evaluation_function.Fitness(solution) : solution.fitness;
            const int chance_incrementation = 10;

            while (true)
            {
                if (random < room_change)
                {
                    to_return = neighbor_selection_timetable.RoomChange(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            room_change = room_change <= 20 ? room_change + chance_incrementation : room_change;
                            //room_change += chance_incrementation;
                        else
                            //room_change = 1;
                            room_change = room_change > chance_incrementation ? room_change - chance_incrementation : room_change;
                        break;
                    }
                }
                else if (random < room_change + period_change)
                {
                    to_return = neighbor_selection_timetable.PeriodChange(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            period_change = period_change < 20 ? period_change + chance_incrementation : period_change;
                            //period_change += chance_incrementation;
                        else
                            //period_change = 1;
                            period_change = period_change > chance_incrementation ? period_change - chance_incrementation : period_change;
                        break;
                    }
                }
                else if (random < room_change + period_change + period_room_change)
                {
                    to_return = neighbor_selection_timetable.PeriodRoomChange(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            period_room_change = period_room_change < 20 ? period_room_change + chance_incrementation : period_room_change;
                            //period_room_change += chance_incrementation;
                        else
                            //period_room_change = 1;
                            period_room_change = period_room_change > chance_incrementation ? period_room_change - chance_incrementation : period_room_change;
                        break;
                    }
                }
                else if (random < room_change + period_change + period_room_change + room_swap)
                {
                    to_return = neighbor_selection_timetable.RoomSwap(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            room_swap = room_swap < 20 ? room_swap + chance_incrementation : room_swap;
                            //room_swap += chance_incrementation;
                        else
                            //room_swap = 1;
                            room_swap = room_swap > chance_incrementation ? room_swap - chance_incrementation : room_swap;
                        break;
                    }
                }
                else if (random < room_change + period_change + period_room_change + room_swap + period_swap)
                {
                    to_return = neighbor_selection_timetable.PeriodSwap(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            period_swap = period_swap < 20 ? period_swap + chance_incrementation : period_swap;
                            //period_swap += chance_incrementation;
                        else
                            //period_swap = 1;
                            period_swap = period_swap > chance_incrementation ? period_swap - chance_incrementation : period_swap;
                        break;
                    }
                }
                else
                {
                    to_return = neighbor_selection_timetable.PeriodRoomSwap(solution);
                    if (to_return != null)
                    {
                        to_return.fitness = (to_return.fitness == -1) ? evaluation_function.Fitness(to_return) : to_return.fitness;
                        if (to_return.fitness < solution.fitness)
                            period_room_swap = period_room_swap < 20 ? period_room_swap + chance_incrementation : period_room_swap;
                            //period_room_swap += chance_incrementation;
                        else
                            //period_room_swap = 1;
                            period_room_swap = period_room_swap > chance_incrementation ? period_room_swap - chance_incrementation : period_room_swap;
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
                    to_return = neighbor_selection_timetable.RoomChange(solution);
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
                    to_return = neighbor_selection_timetable.PeriodChange(solution);
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
                    to_return = neighbor_selection_timetable.PeriodRoomChange(solution);
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
                    to_return = neighbor_selection_timetable.RoomSwap(solution);
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
                    to_return = neighbor_selection_timetable.PeriodSwap(solution);
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
                    to_return = neighbor_selection_timetable.PeriodRoomSwap(solution);
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
