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

            OutputFormatting.StartNew("..//..//results.txt");

            for (SET = 5; SET <= 11; SET++)
            {
                if (SET == 4 || SET ==6 || SET == 8 || SET == 9 || SET == 10)
                    continue;
                OutputFormatting.Write("..//..//results.txt", "SET " + SET);

                for (int repeats = 0; repeats < 2; repeats++)
                {
                    Stopwatch watch = new Stopwatch();
                    Stopwatch watch2 = new Stopwatch();
                    watch2.Start();

                    Console.WriteLine("**SET** " + SET);
                    LoaderTimetable loader = new LoaderTimetable("..//..//exam_comp_set" + SET + ".exam");
                    loader.Unload();
                
                    watch.Start();
                    loader.Load();

                    Console.WriteLine("Loader: " + watch.ElapsedMilliseconds);

                    var evaluation = new EvaluationFunctionTimetable();

                    GraphColoring gc = new GraphColoring();
                    watch.Restart();
                    solution = gc.Exec();
                    Console.WriteLine("GraphColoring Time: " + watch.ElapsedMilliseconds);
                    //Console.WriteLine("GC DTF: " + evaluation.DistanceToFeasibility(solution));
                    watch.Restart();
                    solution.fitness = evaluation.Fitness(solution);
                    Console.WriteLine("Fitness Time: " + watch.ElapsedMilliseconds);
                    Console.WriteLine("GC Fitness: " + solution.fitness);

                    SimulatedAnnealingTimetable sa = new SimulatedAnnealingTimetable();
                    watch.Restart();
                    sa.Exec2(solution, 0.01, Math.Pow(Math.E, -18), 5, 0.001, SimulatedAnnealingTimetable.type_random, true);
                    Console.WriteLine("SA Time: " + watch.ElapsedMilliseconds);
                    Console.WriteLine("SA Random Fitness: " + solution.fitness);
                    Console.WriteLine("generated_neighbors: " + sa.generated_neighbors);
                    sa.generated_neighbors = 0;

                    sa.OnlyBetter(solution, 221000 - watch2.ElapsedMilliseconds, SimulatedAnnealingTimetable.type_random, true);
                    Console.WriteLine("HC Total Time: " + watch2.ElapsedMilliseconds);
                    Console.WriteLine("HC Random Fitness: " + solution.fitness);
                    Console.WriteLine("generated_neighbors: " + sa.generated_neighbors);
                    ////Console.WriteLine("SA Guided1 Fitness: " + SA_Solution.fitness);
                    ////Console.WriteLine("generated_neighbors: " + sa.generated_neighbors);
                    ////sa.generated_neighbors = 0;

                    //////221000
                    ////SA_Solution = (Solution)solution.Copy();
                    ////sa.OnlyBetter(SA_Solution, 5000, SimulatedAnnealingTimetable.type_guided2, true);
                    ////Console.WriteLine("SA Guided2 Fitness: " + SA_Solution.fitness);
                    ////Console.WriteLine("generated_neighbors: " + sa.generated_neighbors);
                    ////sa.generated_neighbors = 0;
                    OutputFormatting.Write("..//..//results.txt", ""+solution.fitness+" "+watch2.ElapsedMilliseconds);
                    PrintToFile("..//..//output" + SET + ".txt", solution);

                    //Console.ReadKey();
                }
            }
            while (Console.ReadKey().Key != ConsoleKey.NumPad7) ;

            //PrintToFile("..//..//output.txt", solution);
        }

        private static void PrintToFile(string output_txt, Solution solution)
        {
            OutputFormatting.Format(output_txt, solution);
        }

    }
}
