using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Loader.Timetable;

namespace Tests.LoaderTest
{
    public class Main1
    {
        public static void Main_()
        {
            LoaderTimetable loader = new LoaderTimetable("..//..//exam_comp_set8.exam");
            loader.Exec();
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }
    }
}
