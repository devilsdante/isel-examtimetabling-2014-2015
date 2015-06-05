using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using DAL.Models.Solution.Timetabling;

namespace Tools
{
    public static class OutputFormatting
    {
        public static void Format(string path, Solution solution)
        {
            if (solution == null) return;

            System.IO.StreamWriter file = new System.IO.StreamWriter(path);

            for (int exam_id = 0; exam_id < solution.ExaminationCount(); exam_id++)
            {
                file.Write(solution.GetPeriodFrom(exam_id) + ", " + solution.GetRoomFrom(exam_id) + "\r\n");
            }

            file.Close();
        }
    }
}
