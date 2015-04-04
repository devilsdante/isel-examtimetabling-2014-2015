﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Solution : IEntity
    {
        public int id { get; set; }
        public IDictionary<Period, List<ExaminationRoomRel>> periods_map { get; set; }

        public Solution(int id)
        {
            this.id = id;
            periods_map = new Dictionary<Period, List<ExaminationRoomRel>>();
        }
    }

    public class ExaminationRoomRel
    {
        public Examination exam { get; set; }
        public Room room { get; set; }

        public ExaminationRoomRel(Examination exam, Room room){
            this.exam = exam;
            this.room = room;
        }
    }
}
