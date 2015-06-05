using System;
using System.Diagnostics;
using Business;
using DAL.Models.Solution.Timetabling;
using Heuristics;
using Tools;
using Tools.EvaluationFunction.Timetable;
using Tools.Loader.Timetable;

namespace Tests.GraphColoringTest
{
    class Main1
    {
        static void Main()
        {
            int SET;
            Stopwatch watch = Stopwatch.StartNew();
            Solution solution = null;

            for (SET = 2; SET <= 2; SET++)
            {
                if (SET == 4)
                    continue;

                Console.WriteLine("**SET** " + SET);
                LoaderTimetable loader = new LoaderTimetable("..//..//exam_comp_set"+SET+".exam");
                loader.Unload();
                loader.Load();

                //Console.WriteLine("Time LoaderTimetable: " + watch.ElapsedMilliseconds);
                watch.Restart();
                var evaluation = new EvaluationFunctionTimetable();
                for (int i = 0; i < 10; i++)
                {
                    GraphColoring gc = new GraphColoring();
                    watch.Restart();

                    solution = gc.Exec();
                    long time = watch.ElapsedMilliseconds;

                    Console.WriteLine("Time GraphColoring: " + time);
                    watch.Restart();
                    //Console.WriteLine("GC DTF: " + evaluation.DistanceToFeasibility(solution));
                    watch.Restart();
                    Console.WriteLine("GC Fitness: " + evaluation.Fitness(solution));
                    Console.WriteLine("Time Fitness: " + watch.ElapsedMilliseconds);
                }
                PrintToFile("..//..//output" + SET + ".txt", solution);
            }

            Console.ReadKey();
        }

        private static void PrintToFile(string output_txt, Solution solution)
        {
            OutputFormatting.Format(output_txt, solution);
        }

    }

}
