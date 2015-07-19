using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Solution.BitFlip;
using Tools.Neighborhood;
using Tools.Neighborhood.BitFlip;

namespace Tools.NeighborSelection.BitFlip
{
    public class NeighborSelectionBitFlip
    {
        private readonly Random random = new Random(Guid.NewGuid().GetHashCode());

        public INeighbor BitSwap(SolutionBitFlip solution)
        {
            return new BitFlipNeighbor(solution, random.Next(5));
        }
    }
}
