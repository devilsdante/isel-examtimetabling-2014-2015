using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Business;
using DAL;
using DAL.Models;
using DAL.Models.Solution;
using DAL.Models.Solution.Timetabling;
using Heuristics;
using Heuristics.SimulatedAnnealing;
using Heuristics.SimulatedAnnealing.Timetable;
using Tools;
using Tools.EvaluationFunction.Timetable;
using Tools.Loader.Timetable;

namespace Tests.SimulatedAnnealingTest
{
    class Main1
    {
        static void Main()
        {
            int SET;

            Solution solution = null;
            Solution SA_Solution = null;

            for (SET = 1; SET <= 8; SET++)
            {
                if (SET == 4)
                    continue;
                

                Console.WriteLine("**SET** " + SET);
                LoaderTimetable loader = new LoaderTimetable("..//..//exam_comp_set" + SET + ".exam");
                loader.Unload();
                Stopwatch watch = new Stopwatch();
                watch.Start();
                loader.Load();

                Console.WriteLine("Loader: " + watch.ElapsedMilliseconds);

                var evaluation = new EvaluationFunctionTimetable();

                GraphColoring gc = new GraphColoring();
                watch.Restart();
                solution = gc.Exec();
                Console.WriteLine("GraphColoring Time -------------: " + watch.ElapsedMilliseconds);
                Console.WriteLine("GC DTF: " + evaluation.DistanceToFeasibility(solution));
                watch.Restart();
                solution.fitness = evaluation.Fitness(solution);
                Console.WriteLine("Fitness Time: " + watch.ElapsedMilliseconds);
                Console.WriteLine("GC Fitness: " + solution.fitness);

                //SA_Solution = (Solution)solution.Copy();
                //SimulatedAnnealingTimetable sa = new SimulatedAnnealingTimetable();
                //sa.ExecTimer(SA_Solution, 30000, SimulatedAnnealingTimetable.type_random, true);
                //Console.WriteLine("SA Random Fitness: " + SA_Solution.fitness);
                //Console.WriteLine("generated_neighbors: " + sa.generated_neighbors);
                //sa.generated_neighbors = 0;

                ////SA_Solution = (Solution)solution.Copy();
                ////sa.ExecTimer(SA_Solution, 221000, SimulatedAnnealingTimetable.type_guided1, true);
                ////Console.WriteLine("SA Guided1 Fitness: " + SA_Solution.fitness);
                ////Console.WriteLine("generated_neighbors: " + sa.generated_neighbors);
                ////sa.generated_neighbors = 0;

                //////221000
                ////SA_Solution = (Solution)solution.Copy();
                ////sa.ExecTimer(SA_Solution, 5000, SimulatedAnnealingTimetable.type_guided2, true);
                ////Console.WriteLine("SA Guided2 Fitness: " + SA_Solution.fitness);
                ////Console.WriteLine("generated_neighbors: " + sa.generated_neighbors);
                ////sa.generated_neighbors = 0;

                PrintToFile("..//..//output" + SET + ".txt", solution);

                //Console.ReadKey();
            }
            Console.ReadKey();

            //PrintToFile("..//..//output.txt", solution);
        }

        private static void PrintToFile(string output_txt, Solution solution)
        {
            OutputFormatting.Format(output_txt, solution);
        }

    }
}
