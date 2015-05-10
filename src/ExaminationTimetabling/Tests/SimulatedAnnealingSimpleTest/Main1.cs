using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.SimulatedAnnealingTest;

namespace Tests.SimulatedAnnealingSimpleTest
{
    class Main1
    {
        private static void Main_()
        {
            while (true)
            {

                EvaluationFunctionSimple efs = new EvaluationFunctionSimple();
                SolutionSimple solution1 = new SolutionSimple {solution = "10011"};
                SimulatedAnnealingSimple sa1 = new SimulatedAnnealingSimple();
                sa1.Exec(solution1, 500, 0, 10);
                Console.WriteLine(solution1.solution + " " + efs.Fitness(solution1));
                Console.WriteLine("Max: " + sa1.maximum);

                solution1 = new SolutionSimple { solution = "10011" };
                sa1 = new SimulatedAnnealingSimple();
                sa1.Exec(solution1, 100, 0, 10);
                Console.WriteLine(solution1.solution + " " + efs.Fitness(solution1));
                Console.WriteLine("Max: " + sa1.maximum);
                Console.WriteLine();

                Console.ReadKey();
            }

            

            Console.ReadKey();
        }
    }
}
