using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Solution;
using Tools;
using Tools.EvaluationFunction;
using Tools.Neighborhood;
using Tools.Neighborhood.Timetable;

namespace Heuristics
{
    public abstract class HillClimbing
    {
        protected abstract IEvaluationFunction evaluation_function { get; set; }

        public ISolution Exec(ISolution solution, long miliseconds, int type, bool minimize)
        {
            Stopwatch watch = Stopwatch.StartNew();
            Stopwatch watch2 = Stopwatch.StartNew();
            //InitVals(type);

            while (watch.ElapsedMilliseconds < miliseconds)
            {
                //TimerPrinter(watch.ElapsedMilliseconds, miliseconds);
                //watch.Restart();
                INeighbor neighbor = GenerateNeighbor(solution, type);
                //Console.WriteLine("GenerateNeighbor: " + watch.ElapsedMilliseconds);

                neighbor.fitness = (neighbor.fitness == -1) ? evaluation_function.Fitness(neighbor) : neighbor.fitness;
                solution.fitness = (solution.fitness == -1) ? evaluation_function.Fitness(solution) : solution.fitness;

                double DeltaE = minimize ? neighbor.fitness - solution.fitness : solution.fitness - neighbor.fitness;

                //*********
                int exam1 = -1;
                int exam2 = -1;
                if (neighbor.type == 4)
                {
                    RoomChangeNeighbor n = (RoomChangeNeighbor)neighbor;
                    exam1 = n.examination_id;
                }
                if (neighbor.type == 5)
                {
                    RoomSwapNeighbor n = (RoomSwapNeighbor)neighbor;
                    exam1 = n.examination1_id;
                    exam2 = n.examination2_id;
                }
                if (neighbor.type == 0)
                {
                    PeriodChangeNeighbor n = (PeriodChangeNeighbor)neighbor;
                    exam1 = n.examination_id;
                }
                if (neighbor.type == 3)
                {
                    PeriodSwapNeighbor n = (PeriodSwapNeighbor)neighbor;
                    exam1 = n.examination1_id;
                    exam2 = n.examination2_id;
                }
                if (neighbor.type == 1)
                {
                    PeriodRoomChangeNeighbor n = (PeriodRoomChangeNeighbor)neighbor;
                    exam1 = n.examination_id;
                }
                if (neighbor.type == 2)
                {
                    PeriodRoomSwapNeighbor n = (PeriodRoomSwapNeighbor)neighbor;
                    exam1 = n.examination1_id;
                    exam2 = n.examination2_id;
                }
                //*******

                if (DeltaE <= 0)
                {
                    StaticMatrix.static_matrix[
                        StaticMatrix.run*2, StaticMatrix.examinations.IndexOf(exam1)]++;

                    if (neighbor.type == 2 || neighbor.type == 3 || neighbor.type == 5)
                        StaticMatrix.static_matrix[
                            StaticMatrix.run*2, StaticMatrix.examinations.IndexOf(exam2)]++;

                    solution = neighbor.Accept();
                    solution.fitness = neighbor.fitness;
                }
                else
                {
                    StaticMatrix.static_matrix[
                                StaticMatrix.run * 2 + 1, StaticMatrix.examinations.IndexOf(exam1)]++;

                    if (neighbor.type == 2 || neighbor.type == 3 || neighbor.type == 5)
                        StaticMatrix.static_matrix[
                            StaticMatrix.run * 2 + 1, StaticMatrix.examinations.IndexOf(exam2)]++;  
                    continue;
                }
            }
            return solution;
        }

        protected abstract INeighbor GenerateNeighbor(ISolution solution, int type);

    }
}
