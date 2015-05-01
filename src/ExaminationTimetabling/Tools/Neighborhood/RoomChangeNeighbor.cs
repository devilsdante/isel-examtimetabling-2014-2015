﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace Tools.Neighborhood
{
    public class RoomChangeNeighbor : INeighbor
    {
        private readonly Solution solution;
        private readonly int new_room_id;
        private readonly int examination_id;
        private readonly int period_id;
        private readonly int old_room_id;

        public RoomChangeNeighbor(Solution solution, int examination_id, int new_room_id)
        {
            this.solution = solution;
            this.new_room_id = new_room_id;
            this.examination_id = examination_id;
            this.period_id = solution.epr_associasion[examination_id, 0];
            this.old_room_id = solution.epr_associasion[examination_id, 1];
        }

        public Solution Accept()
        {
            solution.timetable_container[period_id, old_room_id, examination_id] = false;

            solution.timetable_container[period_id, new_room_id, examination_id] = true;
            solution.epr_associasion[examination_id, 1] = new_room_id;
            return solution;
        }

        public Solution Reverse()
        {
            solution.timetable_container[period_id, old_room_id, examination_id] = true;

            solution.timetable_container[period_id, new_room_id, examination_id] = false;
            solution.epr_associasion[examination_id, 1] = old_room_id;
            return solution;
        }
    }
}
