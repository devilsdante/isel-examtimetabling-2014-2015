using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business;
using Tools.Loader.Timetable;

namespace Tests.LoaderTest
{
    public class Main1
    {
        public static void Main()
        {
            for (int SET = 1; SET <= 8; SET++)
            {
                LoaderTimetable loader = new LoaderTimetable("..//..//exam_comp_set"+SET+".exam");
                loader.Load();
                Console.WriteLine("Examinations: "+Examinations.Instance().EntryCount());
                Console.WriteLine("Periods: " + Periods.Instance().EntryCount());
                Console.WriteLine("Rooms: " + Rooms.Instance().EntryCount());
                loader.Unload();
            }
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }
    }
}
