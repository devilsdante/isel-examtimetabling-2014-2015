using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using Heuristics.SimulatedAnnealing;
using Tools;
using Tools.Neighborhood;

namespace ToDelete
{
    public abstract class SA
    {
        protected abstract IEFunction evaluation { get; set; }
        private ICoolingSchedule cooling_schedule;


        public ISolution Exec(ISolution solution, int TMax, int TMin, int loops, int type)
        {
            cooling_schedule = new CoolingScheduleLinear(TMax, TMin, 1);
            InitVals(type);

            for (double T = TMax; T > TMin; T = cooling_schedule.G(T))
            {
                for (int loop = loops; loop > 0; --loop)
                {
                    INeighbor neighbor = GenerateNeighbor(solution, type);

                    neighbor.fitness = (neighbor.fitness == -1) ? evaluation.Fitness(neighbor) : neighbor.fitness;
                    solution.fitness = (solution.fitness == -1) ? evaluation.Fitness(solution) : solution.fitness;

                    double DeltaE = neighbor.fitness - solution.fitness;

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
                    int dtf = evaluation.DistanceToFeasibility(solution);
                    if (dtf != 0)
                    {
                        throw new Exception("Distance to feasibility is not zero! DTF: " + dtf);
                    }
                }
            }
            return solution;
        }

        public Solution ExecTimer(Solution solution, long miliseconds, int type)
        {
            Stopwatch watch = Stopwatch.StartNew();
            InitVals(type);

            while (watch.ElapsedMilliseconds < miliseconds)
            {
                INeighbor neighbor = GenerateNeighbor(solution, type);

                neighbor.fitness = (neighbor.fitness == -1) ? evaluation.Fitness(neighbor) : neighbor.fitness;
                solution.fitness = (solution.fitness == -1) ? evaluation.Fitness(solution) : solution.fitness;

                int DeltaE = neighbor.fitness - solution.fitness;

                if (DeltaE <= 0)
                {
                    solution = neighbor.Accept();
                    solution.fitness = neighbor.fitness;
                }
                else
                {
                    double acceptance_probability = Math.Pow(Math.E, (-(float)DeltaE*miliseconds) / (watch.ElapsedMilliseconds));
                    double random = new Random((int)DateTime.Now.Ticks).NextDouble();

                    if (random <= acceptance_probability)
                    {
                        solution = neighbor.Accept();
                        solution.fitness = neighbor.fitness;
                    }

                    else
                        continue;
                }
                int dtf = evaluation.DistanceToFeasibility(solution);
                if (dtf != 0)
                {
                    throw new Exception("Distance to feasibility is not zero! DTF: " + dtf);
                }
            }
            return solution;
        }

        public Solution ExecLinearTimer(Solution solution, int TMax, int TMin, long miliseconds, int type)
        {
            Stopwatch watch = Stopwatch.StartNew();
            InitVals(type);

            for (int T = TMax; T > TMin; T = TMax - (int)((watch.ElapsedMilliseconds * (TMax - TMin) / miliseconds) + TMin))
            {
                INeighbor neighbor = GenerateNeighbor(solution, type);
                
                neighbor.fitness = (neighbor.fitness == -1) ? evaluation.Fitness(neighbor) : neighbor.fitness;
                solution.fitness = (solution.fitness == -1) ? evaluation.Fitness(solution) : solution.fitness;

                int DeltaE = neighbor.fitness - solution.fitness;

                if (DeltaE <= 0)
                {
                    solution = neighbor.Accept();
                    solution.fitness = neighbor.fitness;
                }
                else
                {
                    double acceptance_probability = Math.Pow(Math.E, (-(float)DeltaE) / T);
                    double random = new Random((int)DateTime.Now.Ticks).NextDouble();

                    if (random <= acceptance_probability)
                    {
                        solution = neighbor.Accept();
                        solution.fitness = neighbor.fitness;
                    }
                        
                    else
                        continue;
                }
                int dtf = evaluation.DistanceToFeasibility(solution);
                if (dtf != 0)
                {
                    throw new Exception("Distance to feasibility is not zero! DTF: " + dtf);
                }
            }
            return solution;
        }

        protected abstract INeighbor GenerateNeighbor(ISolution solution, int type);

        protected abstract void InitVals(int type);
    }
}
