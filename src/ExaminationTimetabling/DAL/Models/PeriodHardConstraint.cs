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
        public int p1 { get; set; }
        public enum types { AFTER, EXAM_COINCIDENCE, EXCLUSION };
        public int type { get; set; }
        public int p2 { get; set; }

        public PeriodHardConstraint(int id, int p1, int type, int p2)
        {
            this.id = id;
            this.p1 = p1;
            this.type = type;
            this.p2 = p2;
        }
    }
}
