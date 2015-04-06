using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business;
using DAL;
using DAL.Models;
using Heuristics;

namespace Tests
{
    class Program
    {
        static void Main()
        {
            //testing
            Examinations examinations = new Examinations();
            PeriodHardConstraints period_hard_constraints = new PeriodHardConstraints();
            GraphColoring gc = new GraphColoring(examinations, period_hard_constraints, null, null, null, null);

            AddDataExaminations(examinations);
            AddDataPeriodHardConstraints(period_hard_constraints);

            PrintExaminationCoincidences(period_hard_constraints);
            gc.Work();
            PrintExaminationCoincidences(period_hard_constraints);
            PrintConflictMatrix(gc);
            Console.ReadKey();
        }

        private static void AddDataPeriodHardConstraints(PeriodHardConstraints period_hard_constraints)
        {
            PeriodHardConstraint phc = new PeriodHardConstraint(1, 2, PeriodHardConstraint.types.EXCLUSION, 3);
            PeriodHardConstraint phc1 = new PeriodHardConstraint(2, 3, PeriodHardConstraint.types.EXCLUSION, 4);
            period_hard_constraints.Insert(phc);
            period_hard_constraints.Insert(phc1);

            phc = new PeriodHardConstraint(3, 4, PeriodHardConstraint.types.EXAM_COINCIDENCE, 5);
            phc1 = new PeriodHardConstraint(4, 4, PeriodHardConstraint.types.EXAM_COINCIDENCE, 2); //shall be deleted
            period_hard_constraints.Insert(phc);
            period_hard_constraints.Insert(phc1);
        }

        public static void AddDataExaminations(Examinations examinations)
        {
            for (int i = 0; i < 8; i++)
            {
                var ex1 = new Examination(i, 200);
                examinations.Insert(ex1);
            }

            var a1 = examinations.GetById(0);
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            a1.students = list;

            a1 = examinations.GetById(1);
            list = new List<int>();
            list.Add(3);
            list.Add(2);
            list.Add(8);
            a1.students = list;

            a1 = examinations.GetById(2);
            list = new List<int>();
            list.Add(1);
            list.Add(8);
            list.Add(10);
            a1.students = list;

            a1 = examinations.GetById(3);
            list = new List<int>();
            list.Add(1);
            list.Add(3);
            list.Add(11);
            a1.students = list;

            a1 = examinations.GetById(4);
            list = new List<int>();
            list.Add(20);
            list.Add(21);
            list.Add(8);
            a1.students = list;

            a1 = examinations.GetById(5);
            list = new List<int>();
            list.Add(22);
            a1.students = list;

            a1 = examinations.GetById(6);
            list = new List<int>();
            list.Add(23);
            list.Add(1);
            a1.students = list;

            a1 = examinations.GetById(7);
            list = new List<int>();
            list.Add(2);
            a1.students = list;
        }

        public static void PrintConflictMatrix(GraphColoring gc)
        {
            for (int x = 0; x < gc.conflict_matrix.GetLength(0); x += 1)
            {
                for (int y = 0; y < gc.conflict_matrix.GetLength(1); y += 1)
                {
                    Console.Write(gc.conflict_matrix[x, y] + " ");
                }
                Console.WriteLine();
            }
            foreach (var conf in gc.conflicts)
            {
                Console.Write(conf+" ");
            }
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
