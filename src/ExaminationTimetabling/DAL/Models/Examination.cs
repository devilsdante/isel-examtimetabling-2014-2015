using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Examination : IEntity
    {
        public int id { get; set; }
        public int duration { get; set; }
        public int students_count { get; set; }
        public int conflict;

        public Examination(int id)
        {
            this.id = id;
            this.duration = -1;
            this.students_count = 0;
            this.conflict = 0;
        }

        public Examination(int id, int duration, int students_count)
        {
            this.id = id;
            this.students_count = students_count;
            this.duration = duration;
            this.conflict = 0;
        }
    }
}
