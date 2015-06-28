using System;
using System.Collections.Generic;
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
        static void Main_()
        {
            //int SET;
            //Stopwatch watch = Stopwatch.StartNew();
            //Solution solution = null;

            //for (SET = 1; SET <= 8; SET++)
            //{
            //    if (SET == 4)
            //        continue;

            //    Console.WriteLine("**SET** " + SET);
            //    LoaderTimetable loader = new LoaderTimetable("..//..//exam_comp_set" + SET + ".exam");
            //    loader.Unload();
            //    loader.Load();

            //    Console.WriteLine("Time LoaderTimetable: " + watch.ElapsedMilliseconds);
            //    watch.Restart();
            //    var evaluation = new EvaluationFunctionTimetable();
            //    for (int i = 0; i < 1; i++)
            //    {
            //        GraphColoring gc = new GraphColoring();
            //        watch.Restart();

            //        solution = gc.Exec();
            //        long time = watch.ElapsedMilliseconds;

            //        Console.WriteLine("Time GraphColoring: " + time);
            //        watch.Restart();
            //        //Console.WriteLine("GC DTF: " + evaluation.DistanceToFeasibility(solution));
            //        watch.Restart();
            //        Console.WriteLine("GC Fitness: " + evaluation.Fitness(solution));
            //        Console.WriteLine("Time Fitness: " + watch.ElapsedMilliseconds);
            //    }
            //    PrintToFile("..//..//output" + SET + ".txt", solution);
            //}

            Test1();

            Console.ReadKey();
        }

        private static void PrintToFile(string output_txt, Solution solution)
        {
            OutputFormatting.Format(output_txt, solution);
        }

        private static void Test1()
        {
            int SET;
            Stopwatch watch = Stopwatch.StartNew();
            Solution solution = null;

            List<int> list_scores = new List<int>(8) { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            List<long> list_timings = new List<long>(8) { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            

            for (SET = 8; SET <= 12; SET++)
            {
                if (SET == 4)
                    continue;

                Console.WriteLine("**SET** " + SET);
                LoaderTimetable loader = new LoaderTimetable("..//..//exam_comp_set" + SET + ".exam");
                loader.Unload();
                loader.Load();

                Console.WriteLine("Time LoaderTimetable: " + watch.ElapsedMilliseconds);
                watch.Restart();
                var evaluation = new EvaluationFunctionTimetable();

                List<long> list_timing_cicle = new List<long>(10);
                List<int> list_scores_cicle = new List<int>(10);

                for (int i = 0; i < 10; i++)
                {
                    GraphColoring gc = new GraphColoring();
                    watch.Restart();

                    solution = gc.Exec();
                    long time = watch.ElapsedMilliseconds;
                    list_timing_cicle.Add(time);
                    Console.WriteLine("Time GraphColoring: " + time);
                    watch.Restart();
                    int fitness = evaluation.Fitness(solution);
                    Console.WriteLine("GC Fitness: " + fitness);
                    list_scores_cicle.Add(fitness);
                }

                foreach (int value in list_scores_cicle)
                {
                    list_scores[SET - 1] += value;
                }
                list_scores[SET - 1] /= list_scores_cicle.Count;

                foreach (int value in list_timing_cicle)
                {
                    list_timings[SET - 1] += value;
                }
                list_timings[SET - 1] /= list_timing_cicle.Count;

                PrintToFile("..//..//output" + SET + ".txt", solution);
            }

            for (int idx = 0; idx < list_scores.Count; idx++)
            {
                Console.WriteLine("Score SET" + (idx + 1) + "is " + list_scores[idx]);
            }

            for (int idx = 0; idx < list_timings.Count; idx++)
            {
                Console.WriteLine("Timings SET" + (idx + 1) + "is " + list_timings[idx]);
            }

            Console.ReadKey();
        }
    }

}
