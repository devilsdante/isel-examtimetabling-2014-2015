using System;
using System.Collections.Generic;
using System.Diagnostics;
using Business;
using DAL.Models.Solution.Timetabling;
using Heuristics;
using Heuristics.SimulatedAnnealing.Timetable;
using Tools;
using Tools.EvaluationFunction.Timetable;
using Tools.Loader.Timetable;

namespace Tests2
{
    class Main1
    {
        static void Main()
        {
            //int SET;

            //Solution solution = null;
            //Solution SA_Solution = null;

            //OutputFormatting.StartNew("..//..//results1.txt");

            //for (SET = 1; SET <= 1; SET++)
            //{
            //    if (SET == 4)
            //        continue;
            //    OutputFormatting.Write("..//..//results1.txt", "SET " + SET);

            //    for (int repeats = 0; repeats < 1; repeats++)
            //    {
            //        double TMax = 0.1;
            //        double TMin = Math.Pow(Math.E, -7);
            //        int reps = 5;
            //        double rate = Math.Pow(Math.E, -7);

            //        Stopwatch watch = new Stopwatch();
            //        Stopwatch watch2 = new Stopwatch();
            //        watch2.Start();

            //        Console.WriteLine("**SET** " + SET);
            //        LoaderTimetable loader = new LoaderTimetable("..//..//exam_comp_set" + SET + ".exam");
            //        loader.Unload();

            //        watch.Start();
            //        loader.Load();

            //        Console.WriteLine("Loader: " + watch.ElapsedMilliseconds);

            //        var evaluation = new EvaluationFunctionTimetable();

            //        GraphColoring gc = new GraphColoring();
            //        watch.Restart();
            //        solution = gc.Exec();
            //        Console.WriteLine("GraphColoring Time: " + watch.ElapsedMilliseconds);
            //        //Console.WriteLine("GC DTF: " + evaluation.DistanceToFeasibility(solution));
            //        watch.Restart();
            //        solution.fitness = evaluation.Fitness(solution);
            //        Console.WriteLine("Fitness Time: " + watch.ElapsedMilliseconds);
            //        Console.WriteLine("GC Fitness: " + solution.fitness);

            //        SimulatedAnnealingTimetable sa = new SimulatedAnnealingTimetable();
            //        watch.Restart();
            //        sa.Exec2(solution, TMax, TMin, reps, rate, SimulatedAnnealingTimetable.type_random, true);
            //        Console.WriteLine("SA Time: " + watch.ElapsedMilliseconds);
            //        Console.WriteLine("SA Random Fitness: " + solution.fitness);
            //        Console.WriteLine("generated_neighbors: " + sa.generated_neighbors);
            //        sa.generated_neighbors = 0;

            //        //sa.OnlyBetter(solution, 221000 - watch2.ElapsedMilliseconds, SimulatedAnnealingTimetable.type_random, true);
            //        //Console.WriteLine("HC Total Time: " + watch2.ElapsedMilliseconds);
            //        //Console.WriteLine("HC Random Fitness: " + solution.fitness);
            //        //Console.WriteLine("generated_neighbors: " + sa.generated_neighbors);
            //        ////Console.WriteLine("SA Guided1 Fitness: " + SA_Solution.fitness);
            //        ////Console.WriteLine("generated_neighbors: " + sa.generated_neighbors);
            //        ////sa.generated_neighbors = 0;

            //        //////221000
            //        ////SA_Solution = (Solution)solution.Copy();
            //        ////sa.OnlyBetter(SA_Solution, 5000, SimulatedAnnealingTimetable.type_guided2, true);
            //        ////Console.WriteLine("SA Guided2 Fitness: " + SA_Solution.fitness);
            //        ////Console.WriteLine("generated_neighbors: " + sa.generated_neighbors);
            //        ////sa.generated_neighbors = 0;
            //        OutputFormatting.Write("..//..//results1.txt", "" + solution.fitness + " " + watch2.ElapsedMilliseconds);
            //        PrintToFile("..//..//output" + SET + ".txt", solution);

            //        //Console.ReadKey();
            //    }
            //}
            //while (Console.ReadKey().Key != ConsoleKey.NumPad7) ;

            ////PrintToFile("..//..//output.txt", solution);

            for (int SET = 1; SET <= 12; SET++)
            {
                if (SET == 4)
                    continue;
                Console.WriteLine("** SET "+SET+" **");
                Stopwatch watch = new Stopwatch();

                
                

                for(int i = 0; i < 10; i++)
                {
                    LoaderTimetable loader = new LoaderTimetable("..//..//..//Tests//exam_comp_set" + SET + ".exam");
                    loader.Load();
                    watch.Restart();
                    GraphColoring gc = new GraphColoring();
                    gc.Exec();
                    Console.WriteLine(watch.ElapsedMilliseconds);
                    //Console.WriteLine("Examinations: " + Examinations.Instance().EntryCount());
                    //Console.WriteLine("Periods: " + Periods.Instance().EntryCount());
                    //Console.WriteLine("Rooms: " + Rooms.Instance().EntryCount());
                    loader.Unload();
                }
               
            }
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }

        private static void PrintToFile(string output_txt, Solution solution)
        {
            OutputFormatting.Format(output_txt, solution);
        }

    }
}
