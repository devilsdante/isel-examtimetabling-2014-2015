using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace Tools.Neighborhood
{
    class RoomSwapNeighbor:INeighbor
    {
        private readonly Solution solution;
        private readonly int examination1_id;
        private readonly int examination2_id;
        private readonly int period1_id;
        private readonly int period2_id;
        private readonly int room1_id;
        private readonly int room2_id;
        
        public RoomSwapNeighbor(Solution solution, int examination1_id, int examination2_id)
        {
            this.solution = solution;
            this.examination1_id = examination1_id;
            this.examination2_id = examination2_id;
            this.period1_id = solution.epr_associasion[examination1_id, 0];
            this.period2_id = solution.epr_associasion[examination2_id, 0];
            this.room1_id = solution.epr_associasion[examination1_id, 1];
            this.room2_id = solution.epr_associasion[examination2_id, 1];
        }
        public Solution Accept()
        {
            solution.timetable_container[period1_id, room1_id, examination1_id] = false;
            solution.timetable_container[period2_id, room2_id, examination2_id] = false;

            solution.timetable_container[period1_id, room2_id, examination1_id] = true;
            solution.timetable_container[period2_id, room1_id, examination2_id] = true;

            solution.epr_associasion[examination1_id, 1] = room2_id;
            solution.epr_associasion[examination2_id, 1] = room1_id;
            return solution;
        }

        public Solution Reverse()
        {
            solution.timetable_container[period1_id, room1_id, examination1_id] = true;
            solution.timetable_container[period2_id, room2_id, examination2_id] = true;

            solution.timetable_container[period1_id, room2_id, examination1_id] = false;
            solution.timetable_container[period2_id, room1_id, examination2_id] = false;

            solution.epr_associasion[examination1_id, 1] = room1_id;
            solution.epr_associasion[examination2_id, 1] = room2_id;
            return solution;
        }
    }
}
