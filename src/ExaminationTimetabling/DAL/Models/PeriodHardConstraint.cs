using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class PeriodHardConstraint : IEntity
    {
        public int id { get; set; }
        public int ex1 { get; set; }
        public enum types { AFTER, EXAM_COINCIDENCE, EXCLUSION };
        public types type { get; set; }
        public int ex2 { get; set; }

        public PeriodHardConstraint(int id, int ex1, types type, int ex2)
        {
            this.id = id;
            this.ex1 = ex1;
            this.type = type;
            this.ex2 = ex2;
        }
    }
}
