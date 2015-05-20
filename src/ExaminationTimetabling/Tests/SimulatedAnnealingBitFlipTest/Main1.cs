using System;
using DAL.Models.Solution.BitFlip;
using Tools.EvaluationFunction.BitFlip;

namespace Tests.SimulatedAnnealingBitFlipTest
{
    class Main1
    {
        private static void Main_()
        {
            while (true)
            {

                EvaluationFunctionBitFlip efs = new EvaluationFunctionBitFlip();
                Heuristics.SimulatedAnnealing.BitFlip.SimulatedAnnealingBitFlip sa1 = new Heuristics.SimulatedAnnealing.BitFlip.SimulatedAnnealingBitFlip();

                SolutionBitFlip solution1 = new SolutionBitFlip { bits_string = "10011" };
                sa1.Exec(solution1, 500, 1, 1, -1, false);
                Console.WriteLine(solution1.bits_string + " " + efs.Fitness(solution1));
                Console.WriteLine("Max: " + sa1.maximum);

                solution1 = new SolutionBitFlip { bits_string = "10011" };
                sa1 = new Heuristics.SimulatedAnnealing.BitFlip.SimulatedAnnealingBitFlip();

                sa1.Exec(solution1, 100, 1, 1, -1, false);
                Console.WriteLine(solution1.bits_string + " " + efs.Fitness(solution1));
                Console.WriteLine("Max: " + sa1.maximum);
                Console.WriteLine();

                Console.ReadKey();
            }

            

            Console.ReadKey();
        }
    }
}
