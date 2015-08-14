using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Business;
using DAL;
using DAL.Models;
using DAL.Models.Solution;
using DAL.Models.Solution.Timetabling;
using Heuristics;
using Heuristics.Hill_Climbing.Timetable;
using Heuristics.SimulatedAnnealing;
using Heuristics.SimulatedAnnealing.Timetable;
using Tools;
using Tools.EvaluationFunction.Timetable;
using Tools.Loader.Timetable;
using Tools.NeighborSelection.Timetable;

namespace Tests.SimulatedAnnealingTest
{
    class Main1
    {
        static void Main()
        {
            int SET;

            Solution solution = null;
            Solution SA_Solution = null;
            int repeats_count = 5;

            OutputFormatting.StartNew("..//..//results.txt");

            for (SET = 1; SET <= 1; SET++)
            {
                if (SET == 4)
                    continue;

                OutputFormatting.Write("..//..//results.txt", "SET " + SET);

                double TMax = 0.1;
                double TMin = 1e-06;
                int reps = 5;
                double rate = -1;

                for (int repeats = 0; repeats < repeats_count; repeats++)
                {
                    //Set2

                    
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

                    Console.WriteLine("Loader: " + watch.ElapsedMilliseconds);

                    StaticMatrix.examinations = StaticMatrix.examinations ?? Examinations.Instance().GetAll().OrderByDescending(exam => exam.conflict).ToList().ConvertAll(exam => exam.id);
                    StaticMatrix.static_matrix = StaticMatrix.static_matrix ?? new int[repeats_count*2, StaticMatrix.examinations.Count];
                    StaticMatrix.run = repeats;

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

                    rate = (rate != -1) ? rate : ComputeRate(TMax, TMin, reps, solution);
                    Console.WriteLine(rate);

                    SimulatedAnnealingTimetable sa = new SimulatedAnnealingTimetable();
                    Console.WriteLine("supposed generated_neighbors: " + sa.GetSANumberEvaluations(TMax, rate, reps, TMin));
                    watch.Restart();
                    sa.Exec2(solution, TMax, TMin, reps, rate, SimulatedAnnealingTimetable.type_random, true);
                    long sa_time = watch.ElapsedMilliseconds;
                    int sa_fitness = solution.fitness;
                    int sa_feas_neighbors = sa.generated_neighbors;
                    int sa_nonfeas_neighbors = NeighborSelectionTimetable.non_feasibles;

                    Console.WriteLine("SA Time: " + sa_time);
                    Console.WriteLine("SA Random Fitness: " + sa_fitness);
                    Console.WriteLine("SA Random Fitness: " + evaluation.Fitness(solution));
                    Console.WriteLine("SA Feasible Neighbors: " + sa_feas_neighbors);
                    Console.WriteLine("SA Non-Feasible Neighbors: " + sa_nonfeas_neighbors);

                    HillClimbingTimetable hc = new HillClimbingTimetable();
                    hc.Exec(solution, 221000 - watch2.ElapsedMilliseconds, SimulatedAnnealingTimetable.type_random, true);
                    long total_time = watch2.ElapsedMilliseconds;
                    long hc_time = total_time - sa_time;
                    int hc_fitness = solution.fitness;
                    int hc_feas_neighbors = hc.generated_neighbors;
                    int hc_nonfeas_neighbors = NeighborSelectionTimetable.non_feasibles;

                    Console.WriteLine("HC Total Time: " + hc_time);
                    Console.WriteLine("HC Random Fitness: " + hc_fitness);
                    Console.WriteLine("HC Random Fitness: " + evaluation.Fitness(solution));
                    Console.WriteLine("HC Feasible Neighbors: " + hc_feas_neighbors);
                    Console.WriteLine("HC Non-Feasible Neighbors: " + hc_nonfeas_neighbors);

                    OutputFormatting.Write("..//..//results.txt", "SA: " + sa_fitness + " " + sa_time + " " + sa_feas_neighbors + " " + sa_nonfeas_neighbors + " " + rate + ", HC: " + +hc_fitness + " " + hc_time + " " + hc_feas_neighbors + " " + hc_nonfeas_neighbors);
                    PrintToFile("..//..//output" + SET + ".txt", solution);

                    //Console.ReadKey();
                }
            }
            Console.WriteLine("PRESS 7 ON THE NUMPAD TO CONTINUE..........");
            while (Console.ReadKey().Key != ConsoleKey.NumPad7) ;

            for (int j = 0; j < StaticMatrix.examinations.Count; j++)
            {
                for (int i = 0; i < repeats_count; i++)
                {
                    OutputFormatting.Write("..//..//SAResults.dat", i + "\t" + j + "\t" + StaticMatrix.static_matrix[i * 2, j]);
                    OutputFormatting.Write("..//..//SAResults.dat", i + "\t" + j + "\t" + StaticMatrix.static_matrix[i * 2 + 1, j]);
                }
                OutputFormatting.Write("..//..//SAResults.dat", "");
            }

            //PrintToFile("..//..//output.txt", solution);
        }

        private static void PrintToFile(string output_txt, Solution solution)
        {
            OutputFormatting.Format(output_txt, solution);
        }

        private static void Main2()
        {
            
            int SET = 2;
            LoaderTimetable loader = new LoaderTimetable("..//..//exam_comp_set" + SET + ".exam");
            loader.Unload();
            loader.Load();

            while (true)
            {
                GraphColoring gc = new GraphColoring();
                Solution solution = gc.Exec();

                SimulatedAnnealingTimetable sa = new SimulatedAnnealingTimetable();
                Stopwatch watch = Stopwatch.StartNew();

                //**Calibration
                sa.EstimateTotalNumberOfNeighbors(2, 221000, solution);
                sa.EstimateTotalNumberOfNeighbors(2, 221000, solution);
                //**
                Console.WriteLine(watch.ElapsedMilliseconds);

                long total_neighbors = sa.EstimateTotalNumberOfNeighbors(500, 221000, solution);
                //Console.WriteLine(watch.ElapsedMilliseconds);
                //Console.WriteLine(total_neighbors);
                double offset = 0.18;
                total_neighbors = (long) (total_neighbors*(1 - offset));
                //Console.WriteLine("A usar: " + total_neighbors);

                double TMax = 0.1;
                double TMin = 1e-06;
                int reps = 5;
                double rate = 5.0e-05;

                while (sa.GetSANumberEvaluations(TMax, rate, reps, TMin) < total_neighbors)
                {
                    rate = rate - 0.2e-05;
                    //Console.WriteLine(rate);
                }
                if (sa.GetSANumberEvaluations(TMax, rate - 0.1e-05, reps, TMin) < total_neighbors)
                    rate = rate - 0.1e-05;
                Console.WriteLine(rate);
                //Console.WriteLine(watch.ElapsedMilliseconds);

                //while (Console.ReadKey().Key != ConsoleKey.NumPad7) ;
            }
        }

        public static double ComputeRate(double TMax, double TMin, int reps, Solution solution)
        {
            SimulatedAnnealingTimetable sa = new SimulatedAnnealingTimetable();
            //**Calibration
            sa.EstimateTotalNumberOfNeighbors(2, 221000, solution);
            sa.EstimateTotalNumberOfNeighbors(2, 221000, solution);
            //**

            long total_neighbors = sa.EstimateTotalNumberOfNeighbors(50, 221000, solution);
            double offset = 0.16;
            total_neighbors = (long)(total_neighbors * (1 - offset));
            double rate = 15.0e-05;

            while (sa.GetSANumberEvaluations(TMax, rate - 1e-05, reps, TMin) < total_neighbors)
            {
                if (rate - 1e-05 - 1e-05 < 0)
                    break;
                rate = rate - 1e-05;
            }
            while (sa.GetSANumberEvaluations(TMax, rate - 0.5e-05, reps, TMin) < total_neighbors)
            {
                if (rate - 0.5e-05 - 0.5e-05 < 0)
                    break;
                rate = rate - 0.5e-05;
            }
            while (sa.GetSANumberEvaluations(TMax, rate - 0.2e-05, reps, TMin) < total_neighbors)
            {
                if (rate - 0.2e-05 - 0.2e-05 < 0)
                    break;
                rate = rate - 0.2e-05;
            }
            while (sa.GetSANumberEvaluations(TMax, rate - 0.1e-05, reps, TMin) < total_neighbors)
            {
                if (rate - 1e-06 - 1e-06 < 0)
                    break;
                rate = rate - 1e-06;
            }
            while (sa.GetSANumberEvaluations(TMax, rate - 0.1e-06, reps, TMin) < total_neighbors)
            {
                if (rate - 1e-07 - 1e-07 < 0)
                    break;
                rate = rate - 1e-07;
            }
            //if (sa.GetSANumberEvaluations(TMax, rate - 0.1e-05, reps, TMin) < total_neighbors)
            //    rate = rate - 0.1e-05;

            return rate;
        }
    }
}
