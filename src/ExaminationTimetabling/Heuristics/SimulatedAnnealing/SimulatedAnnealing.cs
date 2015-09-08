using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Permissions;
using DAL.Models;
using DAL.Models.Solution;
using DAL.Models.Solution.BitFlip;
using Heuristics.SimulatedAnnealing.CoolingSchedule;
using Tools;
using Tools.EvaluationFunction;
using Tools.Neighborhood;
using Tools.Neighborhood.Timetable;

namespace Heuristics.SimulatedAnnealing
{
    public abstract class SimulatedAnnealing
    {
        protected abstract IEvaluationFunction evaluation_function { get; set; }
        private ICoolingSchedule cooling_schedule;
        private readonly Random random = new Random((int)DateTime.Now.Ticks);

        public ISolution Exec(ISolution solution, double TMax, double TMin, int loops, int type, bool minimize, long time_limit)

        {
            cooling_schedule = new CoolingScheduleGeometric(0.9);
            Stopwatch watch = Stopwatch.StartNew();
            InitVals(type);

            for (double T = TMax; T > TMin; T = cooling_schedule.G(T))
            {
                for (int loop = loops; loop > 0; --loop)
                {
                    if (time_limit != -1 && time_limit <= watch.ElapsedMilliseconds)
                        return solution;

                    INeighbor neighbor = GenerateNeighbor(solution, type);

                    neighbor.fitness = (neighbor.fitness == -1) ? evaluation_function.Fitness(neighbor) : neighbor.fitness;
                    solution.fitness = (solution.fitness == -1) ? evaluation_function.Fitness(solution) : solution.fitness;

                    double DeltaE = minimize ? neighbor.fitness - solution.fitness : solution.fitness - neighbor.fitness;

                    if (DeltaE <= 0)
                    {
                        solution = neighbor.Accept();
                        solution.fitness = neighbor.fitness;
                    }
                    else
                    {
                        double acceptance_probability = Math.Pow(Math.E, (-DeltaE) / T);
                        double chance = random.NextDouble();

                        if (chance <= acceptance_probability)
                        {
                            solution = neighbor.Accept();
                            solution.fitness = neighbor.fitness;
                        }

                        else
                            continue;
                    }
                    int dtf = evaluation_function.DistanceToFeasibility(solution);
                    if (dtf != 0)
                    {
                        throw new Exception("Distance to feasibility is not zero! DTF: " + dtf);
                    }
                }
            }
            return solution;
        }

        public ISolution Exec2(ISolution solution, double TMax, double TMin, int loops, double rate, int type, bool minimize, long time_limit)
        {
            InitVals(type);
            cooling_schedule = new CoolingScheduleExponential(rate, TMax);
            int t = 1;
            Stopwatch watch = Stopwatch.StartNew();
            watch.Start();

            //TODO Tests only
            //OutputFormatting.StartNew("..//..//..//../..//doc//Latex Project//sa_plot_data.dat");
            //int i = 0;
            //int j = 0;

            for (double T = TMax; T > TMin; T = cooling_schedule.G(t++))
            {
                //Console.WriteLine(T);
                for (int loop = loops; loop > 0; --loop)
                {
                    if (time_limit != -1 && time_limit <= watch.ElapsedMilliseconds)
                        return solution;

                    INeighbor neighbor = GenerateNeighbor(solution, type);

                    Stopwatch watch2= Stopwatch.StartNew();
                    watch2.Start();
                    neighbor.fitness = (neighbor.fitness == -1) ? evaluation_function.Fitness(neighbor) : neighbor.fitness;

                    solution.fitness = (solution.fitness == -1) ? evaluation_function.Fitness(solution) : solution.fitness;

                    double DeltaE = minimize ? neighbor.fitness - solution.fitness : solution.fitness - neighbor.fitness;

                    //TODO Tests only
                    //*********
                    //int exam1 = -1;
                    //int exam2 = -1;
                    //if (neighbor.type == 4)
                    //{
                    //    RoomChangeNeighbor n = (RoomChangeNeighbor)neighbor;
                    //    exam1 = n.examination_id;
                    //}
                    //if (neighbor.type == 5)
                    //{
                    //    RoomSwapNeighbor n = (RoomSwapNeighbor)neighbor;
                    //    exam1 = n.examination1_id;
                    //    exam2 = n.examination2_id;
                    //}
                    //if (neighbor.type == 0)
                    //{
                    //    PeriodChangeNeighbor n = (PeriodChangeNeighbor)neighbor;
                    //    exam1 = n.examination_id;
                    //}
                    //if (neighbor.type == 3)
                    //{
                    //    PeriodSwapNeighbor n = (PeriodSwapNeighbor)neighbor;
                    //    exam1 = n.examination1_id;
                    //    exam2 = n.examination2_id;
                    //}
                    //if (neighbor.type == 1)
                    //{
                    //    PeriodRoomChangeNeighbor n = (PeriodRoomChangeNeighbor)neighbor;
                    //    exam1 = n.examination_id;
                    //}
                    //if (neighbor.type == 2)
                    //{
                    //    PeriodRoomSwapNeighbor n = (PeriodRoomSwapNeighbor)neighbor;
                    //    exam1 = n.examination1_id;
                    //    exam2 = n.examination2_id;
                    //}
                    //*******

                    if (DeltaE <= 0)
                    {
                        //TODO Tests only
                        //StaticMatrix.static_matrix[
                        //        StaticMatrix.run * 2, StaticMatrix.examinations.IndexOf(exam1)]++;

                        //if (neighbor.type == 2 || neighbor.type == 3 || neighbor.type == 5)
                        //    StaticMatrix.static_matrix[
                        //        StaticMatrix.run * 2, StaticMatrix.examinations.IndexOf(exam2)]++;  

                        solution = neighbor.Accept();
                        solution.fitness = neighbor.fitness;

                        //TODO Tests only
                        //i++;
                        //if(i % 40 == 0)
                        //    OutputFormatting.Write("..//..//..//../..//doc//Latex Project//sa_plot_data.dat", ""+j++ +" "+solution.fitness);
                    }
                    else
                    {
                        double acceptance_probability = Math.Pow(Math.E, (-DeltaE) / (T * solution.fitness));
                        double chance = random.NextDouble();

                        if (chance <= acceptance_probability)
                        {
                            //TODO Tests only
                            //StaticMatrix.static_matrix[
                            //        StaticMatrix.run * 2, StaticMatrix.examinations.IndexOf(exam1)]++;

                            //if (neighbor.type == 2 || neighbor.type == 3 || neighbor.type == 5)
                            //    StaticMatrix.static_matrix[
                            //        StaticMatrix.run * 2, StaticMatrix.examinations.IndexOf(exam2)]++;  

                            solution = neighbor.Accept();
                            solution.fitness = neighbor.fitness;

                            //TODO Tests only
                            //i++;
                            //if (i % 40 == 0)
                            //    OutputFormatting.Write("..//..//..//../..//doc//Latex Project//sa_plot_data.dat", "" + j++ + " " + solution.fitness);
                        }
                        else
                        {
                            //TODO Tests only
                            //StaticMatrix.static_matrix[
                            //    StaticMatrix.run * 2 + 1, StaticMatrix.examinations.IndexOf(exam1)]++;

                            //if (neighbor.type == 2 || neighbor.type == 3 || neighbor.type == 5)
                            //    StaticMatrix.static_matrix[
                            //        StaticMatrix.run * 2 + 1, StaticMatrix.examinations.IndexOf(exam2)]++;  
                            continue;
                        }

                    }
                }
            }
            return solution;
        }

        public ISolution ExecDataPlot(ISolution solution, double TMax, double TMin, int loops, double rate, int type, bool minimize)
        {
            //TESTS ONLY
            InitVals(type);
            cooling_schedule = new CoolingScheduleExponential(rate, TMax);
            int t = 1;
            Stopwatch watch = Stopwatch.StartNew();
            watch.Start();

            OutputFormatting.StartNew("..//..//..//../..//doc//Latex Project//sa_plot_data.dat");
            int i = 0;
            int j = 0;

            for (double T = TMax; T > TMin; T = cooling_schedule.G(t++))
            {
                //Console.WriteLine(T);
                for (int loop = loops; loop > 0; --loop)
                {
                    INeighbor neighbor = GenerateNeighbor(solution, type);

                    Stopwatch watch2 = Stopwatch.StartNew();
                    watch2.Start();
                    neighbor.fitness = (neighbor.fitness == -1) ? evaluation_function.Fitness(neighbor) : neighbor.fitness;

                    solution.fitness = (solution.fitness == -1) ? evaluation_function.Fitness(solution) : solution.fitness;

                    double DeltaE = minimize ? neighbor.fitness - solution.fitness : solution.fitness - neighbor.fitness;

                    if (DeltaE <= 0)
                    {       
                        solution = neighbor.Accept();
                        solution.fitness = neighbor.fitness;

                        i++;
                        if (i % 40 == 0)
                            OutputFormatting.Write("..//..//..//../..//doc//Latex Project//sa_plot_data.dat", "" + j++ + " " + solution.fitness);
                    }
                    else
                    {
                        double acceptance_probability = Math.Pow(Math.E, (-DeltaE) / (T * solution.fitness));
                        double chance = random.NextDouble();

                        if (chance <= acceptance_probability)
                        {
                            solution = neighbor.Accept();
                            solution.fitness = neighbor.fitness;

                            i++;
                            if (i % 40 == 0)
                                OutputFormatting.Write("..//..//..//../..//doc//Latex Project//sa_plot_data.dat", "" + j++ + " " + solution.fitness);
                        }
                        else
                        {
                            continue;
                        }

                    }
                }
            }
            return solution;
        }

        public ISolution ExecStaticMatrix(ISolution solution, double TMax, double TMin, int loops, double rate, int type, bool minimize)
        {
            //TESTS ONLY
            InitVals(type);
            cooling_schedule = new CoolingScheduleExponential(rate, TMax);
            int t = 1;
            Stopwatch watch = Stopwatch.StartNew();
            watch.Start();

            for (double T = TMax; T > TMin; T = cooling_schedule.G(t++))
            {
                //Console.WriteLine(T);
                for (int loop = loops; loop > 0; --loop)
                {
                    INeighbor neighbor = GenerateNeighbor(solution, type);

                    Stopwatch watch2 = Stopwatch.StartNew();
                    watch2.Start();
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
                                StaticMatrix.run * 2, StaticMatrix.examinations.IndexOf(exam1)]++;

                        if (neighbor.type == 2 || neighbor.type == 3 || neighbor.type == 5)
                            StaticMatrix.static_matrix[
                                StaticMatrix.run * 2, StaticMatrix.examinations.IndexOf(exam2)]++;  

                        solution = neighbor.Accept();
                        solution.fitness = neighbor.fitness;
                    }
                    else
                    {
                        double acceptance_probability = Math.Pow(Math.E, (-DeltaE) / (T * solution.fitness));
                        double chance = random.NextDouble();

                        if (chance <= acceptance_probability)
                        {
                            StaticMatrix.static_matrix[
                                    StaticMatrix.run * 2, StaticMatrix.examinations.IndexOf(exam1)]++;

                            if (neighbor.type == 2 || neighbor.type == 3 || neighbor.type == 5)
                                StaticMatrix.static_matrix[
                                    StaticMatrix.run * 2, StaticMatrix.examinations.IndexOf(exam2)]++;  

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
                }
            }
            return solution;
        }
        
        public ISolution ExecLinearTimer(ISolution solution, double TMax, double TMin, long miliseconds, int type, bool minimize)
        {
            Stopwatch watch = Stopwatch.StartNew();
            InitVals(type);

            for (double T = TMax; T > TMin; T = TMax - ((watch.ElapsedMilliseconds * (TMax - TMin) / miliseconds) + TMin))
            {
                INeighbor neighbor = GenerateNeighbor(solution, type);

                neighbor.fitness = (neighbor.fitness == -1) ? evaluation_function.Fitness(neighbor) : neighbor.fitness;
                solution.fitness = (solution.fitness == -1) ? evaluation_function.Fitness(solution) : solution.fitness;

                double DeltaE = minimize ? neighbor.fitness - solution.fitness : solution.fitness - neighbor.fitness;

                if (DeltaE <= 0)
                {
                    solution = neighbor.Accept();
                    solution.fitness = neighbor.fitness;
                }
                else
                {
                    double acceptance_probability = Math.Pow(Math.E, (-DeltaE) / T);
                    double chance = random.NextDouble();

                    if (chance <= acceptance_probability)
                    {
                        Console.WriteLine("fitness: " + neighbor.fitness);
                        solution = neighbor.Accept();
                        solution.fitness = neighbor.fitness;
                    }

                    else
                        continue;
                }
                int dtf = evaluation_function.DistanceToFeasibility(solution);
                if (dtf != 0)
                {
                    throw new Exception("Distance to feasibility is not zero! DTF: " + dtf);
                }
            }
            return solution;
        }

        protected abstract INeighbor GenerateNeighbor(ISolution solution, int type);

        protected abstract void InitVals(int type);

        public long GetSANumberEvaluations(double TMax, double R, double K, double TMin)
        {
            double t = 0, temp = TMax;
            CoolingScheduleExponential cooling_schedule = new CoolingScheduleExponential(R, TMax);
            long numberEvaluations = 0;
            do
            {
                for (int i = 1; i <= K; ++i)
                    ++numberEvaluations;
                // Update temperature
                ++t;
                temp = cooling_schedule.G(t);
            } while (temp >= TMin);

            return numberEvaluations;
        }
    }
}
