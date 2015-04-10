using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Solution : IEntity
    {
        public int id { get; set; }
        public bool[,,] timetable_container { get; set; }
        public int[,] epr_associasion; //idx = exam; 0 = period; 1 = room


        public Solution(int id, int period_count, int room_count, int examination_count)
        {
            this.id = id;
            //timetable_container = new List<List<List<bool>>>();

            timetable_container = new bool[period_count, room_count, examination_count];
            epr_associasion = new int[examination_count,2];
        }


    }

}
