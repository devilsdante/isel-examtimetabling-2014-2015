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
        static void Main_()
        {
            int SET;

            Solution solution = null;
            Solution SA_Solution = null;
            int repeats_count = 10;
            LoaderTimetable loader = null;

            OutputFormatting.StartNew("..//..//results.txt");
            OutputFormatting.StartNew("..//..//GCResults.dat");

            for (SET = 1; SET <= 1; SET++)
            {
                //if (SET == 4)
                //    continue;

                OutputFormatting.Write("..//..//results.txt", "SET " + SET);

                for (int repeats = 0; repeats < repeats_count; repeats++)
                {
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
                    loader = new LoaderTimetable("..//..//exam_comp_set" + SET + ".exam");
                    loader.Unload();

                    watch.Start();
                    watch2.Start();
                    loader.Load();
                    
                    StaticMatrix.examinations = StaticMatrix.examinations ?? Examinations.Instance().GetAll().OrderByDescending(exam => exam.conflict).ToList().ConvertAll(exam => exam.id);
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
            Console.WriteLine("PRESS 7 ON THE NUMPAD TO CONTINUE..........");
            while (Console.ReadKey().Key != ConsoleKey.NumPad7) ;

            for (int exam_id = 0; exam_id < StaticMatrix.examinations.Count; exam_id++)
            {

                for (int repeat_id = 0; repeat_id < repeats_count; repeat_id++)
                {
                    //string all_conflicts = "";
                    //foreach (int conflict_exam_id in loader.examination_examinations_conflicts[StaticMatrix.examinations[exam_id]])
                    //{
                    //    all_conflicts += conflict_exam_id + ", ";
                    //}
                    //if (all_conflicts.EndsWith(", "))
                    //{
                    //    all_conflicts =  all_conflicts.Substring(0, all_conflicts.Length - 2);
                    //}
                    OutputFormatting.Write("..//..//GCResults.dat", repeat_id + "\t" + exam_id + "\t" + StaticMatrix.static_matrix[repeat_id, StaticMatrix.examinations.IndexOf(exam_id)]);
                }
                OutputFormatting.Write("..//..//GCResults.dat", "");
            }
                
                    

            
            //PrintToFile("..//..//output.txt", solution);
        }

        private static void PrintToFile(string output_txt, Solution solution)
        {
            OutputFormatting.Format(output_txt, solution);
        }

    }

}
