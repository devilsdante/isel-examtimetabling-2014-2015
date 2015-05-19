using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using DAL.Models.Solution;

namespace Tools.Neighborhood
{
    public interface INeighbor
    {
        int fitness { get; set; }
        ISolution Accept();
        ISolution Reverse();
    }
}
