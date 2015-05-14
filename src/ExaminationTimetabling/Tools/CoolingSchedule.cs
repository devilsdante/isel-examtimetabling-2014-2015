using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public interface ICoolingSchedule
    {
        double TMax { get; set; }
        double TMin { get; set; }
        double rate { get; set; }
        int span { get; set; }

        double G(double T);
    }
}
