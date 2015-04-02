using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business;
using DAL;
using Heuristics;

namespace Tests
{
    class Program
    {
        static void Main()
        {
            //testing
            Examinations examinations = new Examinations();
            for (int i = 0; i < 8; i++)
            {
                var ex1 = new Examination(i, 200);
                examinations.Insert(ex1);
            }

            var a1 = examinations.GetById(0);
            var list = new LinkedList<int>();
            list.AddLast(1);
            list.AddLast(2);
            list.AddLast(3);
            list.AddLast(4);
            a1.students = list;

            a1 = examinations.GetById(1);
            list = new LinkedList<int>();
            list.AddLast(3);
            list.AddLast(2);
            list.AddLast(8);
            a1.students = list;

            a1 = examinations.GetById(2);
            list = new LinkedList<int>();
            list.AddLast(1);
            list.AddLast(8);
            list.AddLast(10);
            a1.students = list;

            a1 = examinations.GetById(3);
            list = new LinkedList<int>();
            list.AddLast(1);
            list.AddLast(3);
            list.AddLast(11);
            a1.students = list;

            a1 = examinations.GetById(4);
            list = new LinkedList<int>();
            list.AddLast(20);
            list.AddLast(21);
            list.AddLast(8);
            a1.students = list;

            a1 = examinations.GetById(5);
            list = new LinkedList<int>();
            list.AddLast(22);
            a1.students = list;

            a1 = examinations.GetById(6);
            list = new LinkedList<int>();
            list.AddLast(23);
            list.AddLast(1);
            a1.students = list;

            a1 = examinations.GetById(7);
            list = new LinkedList<int>();
            list.AddLast(2);
            a1.students = list;

            GraphColoring gc = new GraphColoring(examinations, null, null, null, null, null);

            gc.Work();

            for (int x = 0; x < gc.conflict_matrix.GetLength(0); x += 1)
            {
                for (int y = 0; y < gc.conflict_matrix.GetLength(1); y += 1)
                {
                    Console.Write(gc.conflict_matrix[x, y] + " ");
                }
                Console.WriteLine();
            }

            Console.ReadKey();
        }
    }
}
