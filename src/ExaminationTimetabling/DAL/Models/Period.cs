﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Period : IEntity
    {
        public int id { get; set; }
        public DateTime date { get; set; }
        public int duration { get; set; }
        public int penalty { get; set; }

        public Period(int id1, DateTime date, int duration, int penalty)
        {
            id = id1;
            this.date = date;
            this.duration = duration;
            this.penalty = penalty;
        }
    }
}
