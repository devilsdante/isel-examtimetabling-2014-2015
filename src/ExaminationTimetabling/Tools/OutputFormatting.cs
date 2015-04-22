using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace Tools
{
    public static class OutputFormatting
    {
        public static void Format(string path, Solution solution)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(path);

            for (int i = 0; i < solution.epr_associasion.GetLength(0); i++)
            {
                file.Write(solution.epr_associasion[i, 0] + ", " + solution.epr_associasion[i, 1] + "\r\n");
            }

            file.Close();
        }
    }
}
