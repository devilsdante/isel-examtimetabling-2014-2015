using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Solution.BitFlip;
using Heuristics.SimulatedAnnealing;
using Heuristics.SimulatedAnnealing.BitFlip;
using Tests.SimulatedAnnealingTest;
using Tools.EvaluationFunction.BitFlip;

namespace Tests.SimulatedAnnealingSimpleTest
{
    class Main1
    {
        private static void Main()
        {
            while (true)
            {

                EvaluationFunctionBitFlip efs = new EvaluationFunctionBitFlip();
                SimulatedAnnealingBitFlip sa1 = new SimulatedAnnealingBitFlip();

                SolutionBitFlip solution1 = new SolutionBitFlip { bits_string = "10011" };
                sa1.Exec(solution1, 500, 1, 1, -1, false);
                Console.WriteLine(solution1.bits_string + " " + efs.Fitness(solution1));
                Console.WriteLine("Max: " + sa1.maximum);

                solution1 = new SolutionBitFlip { bits_string = "10011" };
                sa1 = new SimulatedAnnealingBitFlip();

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
