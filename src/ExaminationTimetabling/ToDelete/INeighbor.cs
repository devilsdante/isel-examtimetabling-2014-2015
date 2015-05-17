using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDelete
{
    public interface INeighbor
    {
        int fitness { get; set; }
        Solution Accept();
        Solution Reverse();
    }
}
