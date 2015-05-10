using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using Tests.SimulatedAnnealingTest;
using Tools;
using Tools.Neighborhood;

namespace Tests
{
    class SimulatedAnnealingSimple
    {
        private readonly EvaluationFunctionSimple evaluation;
        private readonly NeighborSelectionSimple neighbor_selection;
        public int maximum;

        public SimulatedAnnealingSimple()
        {
            evaluation = new EvaluationFunctionSimple();
            neighbor_selection = new NeighborSelectionSimple();
        }

        public SolutionSimple Exec(SolutionSimple solution, int TMax, int TMin, int loops)
        {
            solution.fitness = (solution.fitness == -1) ? evaluation.Fitness(solution) : solution.fitness;
            maximum = solution.fitness;
            for (int T = TMax; T > TMin; --T)
            {
                for (int loop = loops; loop > 0; --loop)
                {
                    INeighborSimple neighbor = GenerateNeighbor(solution);


                    neighbor.fitness = (neighbor.fitness == -1) ? evaluation.Fitness(neighbor) : neighbor.fitness;
                    solution.fitness = (solution.fitness == -1) ? evaluation.Fitness(solution) : solution.fitness;

                    //Console.WriteLine("Sol: " + solution.fitness + " " + solution.solution);
                    //Console.WriteLine("Nei: " + neighbor.fitness + " " + neighbor.Accept().solution);
                    //neighbor.Reverse();
                    //Console.ReadKey();

                    int DeltaE = neighbor.fitness - solution.fitness;

                    if (DeltaE >= 0)
                    {
                        solution = neighbor.Accept();
                        solution.fitness = neighbor.fitness;
                        if (maximum < solution.fitness)
                            maximum = solution.fitness;
                    }
                    else
                    {
                        double acceptance_probability = Math.Pow(Math.E, ((float)DeltaE) / T);
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

        public SolutionSimple ExecTimer(SolutionSimple solution, int TMax, int TMin, long miliseconds)
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int T = TMax; T > TMin; T = TMax - (int)((watch.ElapsedMilliseconds * (TMax - TMin) / miliseconds) + TMin))
            {
                INeighborSimple neighbor = GenerateNeighbor(solution);
                
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

        private INeighborSimple GenerateNeighbor(SolutionSimple solution)
        {
            return neighbor_selection.BitSwap(solution);
        }
    }
}
