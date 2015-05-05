using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace Tools.Neighborhood
{
    class PeriodRoomChangeNeighbor:INeighbor
    {
        private readonly Solution solution;
        private readonly int examination_id;
        private readonly int old_period_id;
        private readonly int new_period_id;
        private readonly int old_room_id;
        private readonly int new_room_id;

        public PeriodRoomChangeNeighbor(Solution solution, int examination_id, int new_period_id, int new_room_id)
        {
            this.solution = solution;
            this.new_period_id = new_period_id;
            this.examination_id = examination_id;
            this.old_room_id = solution.epr_associasion[examination_id, 1];
            this.old_period_id = solution.epr_associasion[examination_id, 0];
            this.new_room_id = new_room_id;
        }

        public Solution Accept()
        {
            solution.timetable_container[old_period_id, old_room_id, examination_id] = false;

            solution.timetable_container[new_period_id, new_room_id, examination_id] = true;
            solution.epr_associasion[examination_id, 0] = new_period_id;
            solution.epr_associasion[examination_id, 1] = new_room_id;
            return solution;
        }

        public Solution Reverse()
        {
            solution.timetable_container[old_period_id, old_room_id, examination_id] = true;

            solution.timetable_container[new_period_id, new_room_id, examination_id] = false;
            solution.epr_associasion[examination_id, 0] = old_period_id;
            solution.epr_associasion[examination_id, 1] = old_room_id;
            return solution;
        }
    }
}
