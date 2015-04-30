using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace Tools.Neighborhood
{
    public interface INeighbor
    {
        Solution Accept();
        Solution Reverse();
    }
}
