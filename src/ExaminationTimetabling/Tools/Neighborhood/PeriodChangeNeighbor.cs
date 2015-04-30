using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace Tools.Neighborhood
{
    public class PeriodChangeNeighbor : INeighbor
    {
        private readonly Solution solution;
        private readonly int examination_id;
        private readonly int room_id;
        private readonly int old_period_id;
        private readonly int new_period_id;

        public PeriodChangeNeighbor(Solution solution, int examination_id, int new_period_id, int room_id)
        {
            this.solution = solution;
            this.new_period_id = new_period_id;
            this.examination_id = examination_id;
            this.room_id = room_id;
            this.old_period_id = solution.epr_associasion[examination_id, 0];
        }

        public Solution Accept()
        {
            solution.timetable_container[old_period_id, room_id, examination_id] = false;

            solution.timetable_container[new_period_id, room_id, examination_id] = true;
            solution.epr_associasion[examination_id, 0] = new_period_id;
            return solution;
        }

        public Solution Reverse()
        {
            solution.timetable_container[old_period_id, room_id, examination_id] = true;

            solution.timetable_container[new_period_id, room_id, examination_id] = false;
            solution.epr_associasion[examination_id, 0] = old_period_id;
            return solution;
        }
    }
}
