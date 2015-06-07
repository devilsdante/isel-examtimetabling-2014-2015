using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Business;
using DAL;
using DAL.Models;
using DAL.Models.Solution;
using DAL.Models.Solution.Timetabling;
using Heuristics;
using Heuristics.SimulatedAnnealing;
using Heuristics.SimulatedAnnealing.Timetable;
using Tools;
using Tools.EvaluationFunction.Timetable;
using Tools.Loader.Timetable;

namespace Tests.SimulatedAnnealingTest
{
    class Main1
    {
        static void Main()
        {
            int SET;

            Solution solution = null;
            Solution SA_Solution = null;

            for (SET = 1; SET <= 1; SET++)
            {
                if (SET == 4)
                    continue;

                Console.WriteLine("**SET** " + SET);
                LoaderTimetable loader = new LoaderTimetable("..//..//exam_comp_set" + SET + ".exam");
                loader.Unload();
                loader.Load();

                var evaluation = new EvaluationFunctionTimetable();

                GraphColoring gc = new GraphColoring();

                solution = gc.Exec();

                Console.WriteLine("GC DTF: " + evaluation.DistanceToFeasibility(solution));
                solution.fitness = evaluation.Fitness(solution);
                Console.WriteLine("GC Fitness: " + solution.fitness);

                SA_Solution = (Solution)solution.Copy();
                SimulatedAnnealingTimetable sa = new SimulatedAnnealingTimetable();
                //sa.ExecTimer(SA_Solution, 221000, SimulatedAnnealingTimetable.type_random, true);
                //Console.WriteLine("SA Random Fitness: " + SA_Solution.fitness);

                //SA_Solution = (Solution)solution.Copy();
                sa.ExecTimer(SA_Solution, 5000, SimulatedAnnealingTimetable.type_guided1, true);
                Console.WriteLine("SA Guided1 Fitness: " + SA_Solution.fitness);

                //SA_Solution = (Solution)solution.Copy();
                //sa.ExecTimer(SA_Solution, 221000, SimulatedAnnealingTimetable.type_guided2, true);
                //Console.WriteLine("SA Guided2 Fitness: " + SA_Solution.fitness);

                Console.ReadKey();
            }

            PrintToFile("..//..//output" + SET + ".txt", SA_Solution);

            Console.ReadKey();

            //PrintToFile("..//..//output.txt", solution);
        }

        private static void PrintToFile(string output_txt, Solution solution)
        {
            OutputFormatting.Format(output_txt, solution);
        }

    }
}
