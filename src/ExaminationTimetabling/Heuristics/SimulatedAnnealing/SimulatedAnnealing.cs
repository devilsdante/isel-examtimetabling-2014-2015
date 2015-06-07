﻿using System;
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

namespace Heuristics.SimulatedAnnealing
{
    public abstract class SimulatedAnnealing
    {
        protected abstract IEvaluationFunction evaluation_function { get; set; }
        private ICoolingSchedule cooling_schedule;

        public ISolution Exec(ISolution solution, int TMax, int TMin, int loops, int type, bool minimize)
        {
            cooling_schedule = new CoolingScheduleGeometric(0.9);
            InitVals(type);

            for (double T = TMax; T > TMin; T = cooling_schedule.G(T))
            {
                for (int loop = loops; loop > 0; --loop)
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
                        double random = new Random((int)DateTime.Now.Ticks).NextDouble();

                        if (random <= acceptance_probability)
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

        public ISolution ExecTimer(ISolution solution, long miliseconds, int type, bool minimize)
        {
            Stopwatch watch = Stopwatch.StartNew();
            InitVals(type);

            while (watch.ElapsedMilliseconds < miliseconds)
            {
                TimerPrinter(watch.ElapsedMilliseconds, miliseconds);
                //watch.Restart();
                INeighbor neighbor = GenerateNeighbor(solution, type);
                //Console.WriteLine("GenerateNeighbor: " + watch.ElapsedMilliseconds);

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
                    double acceptance_probability = Math.Pow(Math.E, ((-DeltaE) * miliseconds) / (watch.ElapsedMilliseconds));
                    double random = new Random((int)DateTime.Now.Ticks).NextDouble();

                    if (random <= acceptance_probability)
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
            return solution;
        }

        public ISolution ExecLinearTimer(ISolution solution, int TMax, int TMin, long miliseconds, int type, bool minimize)
        {
            Stopwatch watch = Stopwatch.StartNew();
            InitVals(type);

            for (int T = TMax; T > TMin; T = TMax - (int)((watch.ElapsedMilliseconds * (TMax - TMin) / miliseconds) + TMin))
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
                    double random = new Random((int)DateTime.Now.Ticks).NextDouble();

                    if (random <= acceptance_probability)
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
            return solution;
        }

        protected abstract INeighbor GenerateNeighbor(ISolution solution, int type);

        protected abstract void InitVals(int type);


        private const int timer = 5;
        private int counter = 1;
        private bool loading_printed = false;

        private void TimerPrinter(long elapsed_milliseconds, long total)
        {
            if (elapsed_milliseconds < timer*1000)
            {
                if (!loading_printed)
                {
                    loading_printed = true;
                    Console.Write("Loading");
                }
                if(counter != -1)
                    counter = 1;
                return;
            }
            if (counter*timer*1000 < elapsed_milliseconds)
            {
                counter++;
                Console.Write(".");
                if (total - elapsed_milliseconds < timer * 1000)
                {
                    Console.WriteLine();
                    loading_printed = false;
                }
                    
            }
            
        }
    }
}
