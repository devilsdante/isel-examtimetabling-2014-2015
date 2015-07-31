using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Solution;
using Tools.EvaluationFunction;
using Tools.Neighborhood;

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

                if (DeltaE <= 0)
                {
                    //if (watch2.ElapsedMilliseconds > 1000)
                    //{
                    //    Console.WriteLine("fitness: " + neighbor.fitness);
                    //    watch2.Restart();
                    //}
                    solution = neighbor.Accept();
                    solution.fitness = neighbor.fitness;
                }
            }
            return solution;
        }

        protected abstract INeighbor GenerateNeighbor(ISolution solution, int type);

    }
}
