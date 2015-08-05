using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Business;
using DAL;
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

            Solution solution = null;
            Solution SA_Solution = null;
            int repeats_count = 1;

            OutputFormatting.StartNew("..//..//results.txt");

            for (SET = 4; SET <= 4; SET++)
            {
                //if (SET == 4)
                //    continue;
                OutputFormatting.Write("..//..//results.txt", "SET " + SET);

                for (int repeats = 0; repeats < repeats_count; repeats++)
                {
                    //Set2
                    double TMax = 0.1;
                    double TMin = 1e-06;
                    int reps = 5;
                    double rate;
                    //double rate = 2.6e-05;

                    //Set1
                    //double TMax = 0.1;
                    //double TMin = 1e-06;
                    //int reps = 5;
                    //double rate = 3.5e-05;

                    //double TMax = 0.01;
                    //double TMin = Math.Pow(Math.E, -18);
                    //int reps = 5;
                    //double rate = 0.001;

                    Stopwatch watch = new Stopwatch();
                    Stopwatch watch2 = new Stopwatch();


                    Console.WriteLine("**SET** " + SET);
                    LoaderTimetable loader = new LoaderTimetable("..//..//exam_comp_set" + SET + ".exam");
                    loader.Unload();

                    watch.Start();
                    watch2.Start();
                    loader.Load();
                    
                    StaticMatrix.examinations = StaticMatrix.examinations ?? Examinations.Instance().GetAll().OrderBy(exam => exam.conflict).ToList().ConvertAll(exam => exam.id);
                    StaticMatrix.static_matrix = StaticMatrix.static_matrix ?? new int[repeats_count, StaticMatrix.examinations.Count];

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
                    for (int exam_id = 0; exam_id < solution.ExaminationCount(); exam_id++)
                    {
                        StaticMatrix.static_matrix[repeats, StaticMatrix.examinations.IndexOf(exam_id)] =
                            solution.GetPeriodFrom(exam_id);
                    }
                    

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
