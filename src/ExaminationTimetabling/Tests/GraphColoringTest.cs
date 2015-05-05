using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business;
using DAL;
using DAL.Models;
using Heuristics;
using Tools;

namespace Tests
{
    class GraphColoringTest
    {
        static void Main()
        {
            //testing
            Examinations examinations = Examinations.Instance(10);
            PeriodHardConstraints period_hard_constraints = PeriodHardConstraints.Instance(4);
            Periods periods = Periods.Instance(5);
            RoomHardConstraints room_hard_constraints = RoomHardConstraints.Instance(0);
            Rooms rooms = Rooms.Instance(3);
            Solutions solutions = Solutions.Instance(1);
            ModelWeightings model_weightings = ModelWeightings.Instance(new InstitutionalModelWeightings(5, 3, 2, new []{2,2,2}, 2));

            AddDataRooms(rooms);
            AddDataPeriods(periods);
            AddDataExaminations(examinations);
            AddDataPeriodHardConstraints(period_hard_constraints);

            NeighborSelection neighbor_selection = new NeighborSelection();
            EvaluationFunction evaluation = new EvaluationFunction();

            GraphColoring gc = new GraphColoring();
            SimulatedAnnealing sa = new SimulatedAnnealing();

            

            

            Solution solution = gc.Exec();

            Console.WriteLine("GC Fitness: " + evaluation.Fitness(solution));

            Solution final = sa.Exec(solution, 100, 0);



            while (true)
            {
                

                //Console.WriteLine("GC Valid: " + evaluation.IsValid(solution));
                //Console.WriteLine("GC Distance To Feasibility: " + evaluation.DistanceToFeasibility(solution));
                //Console.WriteLine("GC Fitness: " + evaluation.Fitness(solution));

                //PrintToFile("..//..//output.txt", solution);

                final = sa.Exec(final, 200, 0);

                //Console.WriteLine("SA Valid: " + evaluation.IsValid(final));
                //Console.WriteLine("SA Distance To Feasibility: " + evaluation.DistanceToFeasibility(final));
                Console.WriteLine("SA Fitness: " + evaluation.Fitness(final));

                //PrintToFile("..//..//outout_final.txt", final);
            }

            

            Console.ReadKey();
        }

        private static void PrintToFile(string output_txt, Solution solution)
        {
            Tools.OutputFormatting.Format(output_txt, solution);
        }

        private static void AddDataRooms(Rooms rooms)
        {
            Room room0 = new Room(0, 5, 0);
            Room room1 = new Room(1, 7, 10);
            Room room2 = new Room(2, 7, 50);
            Room room3 = new Room(3, 7, 500);
            Room room4 = new Room(4, 7, 1000);
            Room room5 = new Room(5, 7, 2000);
            Room room6 = new Room(6, 7, 70);
            Room room7 = new Room(7, 7, 100);

            rooms.Insert(room0);
            rooms.Insert(room1);
            rooms.Insert(room2);
            rooms.Insert(room3);
            rooms.Insert(room4);
            rooms.Insert(room5);
            rooms.Insert(room6);
            rooms.Insert(room7);
        }

        private static void AddDataPeriods(Periods periods)
        {
            Period period0 = new Period(0, new DateTime(2005, 04, 15, 9, 30, 0), 210, 0);
            Period period1 = new Period(1, new DateTime(2005, 04, 15, 14, 0, 0), 210, 0);
            Period period2 = new Period(2, new DateTime(2005, 04, 18, 9, 30, 0), 210, 0);
            Period period3 = new Period(3, new DateTime(2005, 04, 18, 14, 0, 0), 210, 0);
            Period period4 = new Period(4, new DateTime(2005, 04, 19, 9, 30, 0), 210, 0);

            periods.Insert(period0);
            periods.Insert(period1);
            periods.Insert(period2);
            periods.Insert(period3);
            periods.Insert(period4);
        }

        private static void AddDataPeriodHardConstraints(PeriodHardConstraints period_hard_constraints)
        {
            PeriodHardConstraint phc = new PeriodHardConstraint(0, 2, PeriodHardConstraint.types.EXCLUSION, 3);
            PeriodHardConstraint phc1 = new PeriodHardConstraint(1, 3, PeriodHardConstraint.types.EXCLUSION, 4);
            period_hard_constraints.Insert(phc);
            period_hard_constraints.Insert(phc1);

            phc = new PeriodHardConstraint(2, 4, PeriodHardConstraint.types.EXAM_COINCIDENCE, 5);
            phc1 = new PeriodHardConstraint(3, 4, PeriodHardConstraint.types.EXAM_COINCIDENCE, 2); //shall be deleted
            period_hard_constraints.Insert(phc);
            period_hard_constraints.Insert(phc1);
        }

        public static void AddDataExaminations(Examinations examinations)
        {
            for (int i = 0; i < 10; i++)
            {
                var ex1 = new Examination(i, 200, 5);
                examinations.Insert(ex1);
            }

            var a1 = examinations.GetById(0);
            var list = new List<int> {1, 2, 3, 4};
            a1.students = list;

            a1 = examinations.GetById(1);
            list = new List<int> {3, 2, 8};
            a1.students = list;

            a1 = examinations.GetById(2);
            list = new List<int> {1, 8, 10};
            a1.students = list;

            a1 = examinations.GetById(3);
            list = new List<int> {1, 3, 11};
            a1.students = list;

            a1 = examinations.GetById(4);
            list = new List<int> {20, 21, 8};
            a1.students = list;

            a1 = examinations.GetById(5);
            list = new List<int> {22};
            a1.students = list;

            a1 = examinations.GetById(6);
            list = new List<int> {23, 1};
            a1.students = list;

            a1 = examinations.GetById(7);
            list = new List<int> {2};
            a1.students = list;

            a1 = examinations.GetById(8);
            list = new List<int> {20, 23, 22, 1, 10};
            a1.students = list;

            a1 = examinations.GetById(9);
            list = new List<int> { 20, 23, 22, 90};
            a1.students = list;
            //a1.duration = 100;
        }

        public static void PrintConflictMatrix(Solution solution, Examinations examinations)
        {
            for (int x = 0; x < solution.conflict_matrix.GetLength(0); x += 1)
            {
                for (int y = 0; y < solution.conflict_matrix.GetLength(1); y += 1)
                {
                    Console.Write(solution.conflict_matrix[x, y] + " ");
                }
                Console.WriteLine();
            }
            foreach (var exam in examinations.GetAll().OrderBy(ex => ex.id))
            {
                Console.Write(exam.conflict+ " ");
            }
            Console.WriteLine();
        }

        private static void PrintExaminationCoincidences(PeriodHardConstraints period_hard_constraints)
        {
            foreach (PeriodHardConstraint coincidence in period_hard_constraints.GetByType(PeriodHardConstraint.types.EXAM_COINCIDENCE))
            {
                Console.WriteLine(coincidence.ex1 + " " + coincidence.type.ToString() + " " + coincidence.ex2);
            }
        }
    }
}
