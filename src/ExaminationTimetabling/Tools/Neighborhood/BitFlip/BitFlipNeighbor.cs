using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Solution;
using DAL.Models.Solution.BitFlip;

namespace Tools.Neighborhood.BitFlip
{
    public class BitFlipNeighbor : INeighbor
    {
        public int fitness{ get; set; }

        public int type
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public SolutionBitFlip solution;
        public int bit;
        public char value;

        public BitFlipNeighbor(SolutionBitFlip solution, int bit)
        {
            this.solution = solution;
            this.value = solution.bits_string[bit];
            this.fitness = -1;
            this.bit = bit;
        }

        public SolutionBitFlip Accept()
        {
            StringBuilder sb = new StringBuilder(solution.bits_string);
            sb[bit] = value == '0' ? '1' : '0';
            solution.bits_string = sb.ToString();
            return solution;
        }

        public SolutionBitFlip Reverse()
        {
            StringBuilder sb = new StringBuilder(solution.bits_string);
            sb[bit] = value;
            solution.bits_string = sb.ToString();
            return solution;
        }

        ISolution INeighbor.Accept()
        {
            return Accept();
        }

        ISolution INeighbor.Reverse()
        {
            return Reverse();
        }
    }
}
