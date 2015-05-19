using System;

namespace DAL.Models.Solution.BitFlip
{
    public class SolutionBitFlip : ISolution
    {
        public int fitness { get; set; }
        public String bits_string;
        public int id { get; set; }

        public SolutionBitFlip()
        {
            fitness = -1;
        }

        public ISolution Copy()
        {
            throw new NotImplementedException();
        }

        public bool Equals(ISolution solution)
        {
            throw new NotImplementedException();
        }
    }
}
