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
            Examinations examinations = new Examinations(10);
            PeriodHardConstraints period_hard_constraints = new PeriodHardConstraints(4);
            Periods periods = new Periods(5);
            RoomHardConstraints room_hard_constraints = new RoomHardConstraints(0);
            Rooms rooms = new Rooms(3);
            Solutions solutions = new Solutions(1);
            ModelWeightings mws = new ModelWeightings();
            InstitutionalModelWeightings imw = new InstitutionalModelWeightings(5, 3, 2, new []{2,2,2}, 2);
            mws.Set(imw);

            GraphColoring gc = new GraphColoring(examinations, period_hard_constraints, periods, room_hard_constraints, rooms, solutions);

            EvaluationFunction evaluation = new EvaluationFunction(examinations, period_hard_constraints, room_hard_constraints, rooms, periods, mws);

            AddDataRooms(rooms);
            AddDataPeriods(periods);
            AddDataExaminations(examinations);
            AddDataPeriodHardConstraints(period_hard_constraints);

            PrintExaminationCoincidences(period_hard_constraints);
            gc.Work();
            PrintExaminationCoincidences(period_hard_constraints);
            PrintConflictMatrix(gc, examinations);
            PrintToFile("..//..//output.txt", gc.solution);
            Console.WriteLine("Valid: "+evaluation.IsValid(gc.solution));
            Console.WriteLine("Distance To Feasibility: "+evaluation.DistanceToFeasibility(gc.solution));
            Console.WriteLine("Fitness: "+evaluation.Fitness(gc.solution));
            Console.ReadKey();
        }

        private static void PrintToFile(string output_txt, Solution solution)
        {
            Tools.OutputFormatting.Format(output_txt, solution);
        }

        private static void AddDataRooms(Rooms rooms)
        {
            Room room0 = new Room(0, 5, 0);
            Room room1 = new Room(1, 7, 0);
            Room room2 = new Room(2, 7, 0);

            rooms.Insert(room0);
            rooms.Insert(room1);
            //rooms.Insert(room2);
        }

        private static void AddDataPeriods(Periods periods)
        {
            Period period0 = new Period(0, new DateTime(2005, 04, 15, 9, 30, 0), 210, 0);
            Period period1 = new Period(1, new DateTime(2005, 04, 15, 14, 0, 0), 210, 0);
            Period period2 = new Period(2, new DateTime(2005, 04, 18, 9, 30, 0), 210, 0);
            Period period3 = new Period(3, new DateTime(2005, 04, 18, 14, 0, 0), 210, 70);
            Period period4 = new Period(4, new DateTime(2005, 04, 19, 9, 30, 0), 210, 50);

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

        public static void PrintConflictMatrix(GraphColoring gc, Examinations examinations)
        {
            for (int x = 0; x < gc.conflict_matrix.GetLength(0); x += 1)
            {
                for (int y = 0; y < gc.conflict_matrix.GetLength(1); y += 1)
                {
                    Console.Write(gc.conflict_matrix[x, y] + " ");
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
