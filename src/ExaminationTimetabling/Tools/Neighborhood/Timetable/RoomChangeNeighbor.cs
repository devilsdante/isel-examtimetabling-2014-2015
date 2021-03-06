﻿using DAL.Models.Solution;
using DAL.Models.Solution.Timetabling;

namespace Tools.Neighborhood.Timetable
{
    public class RoomChangeNeighbor : INeighbor
    {
        public int fitness { get; set; }
        public int type { get; set; }
        public readonly Solution solution;
        public readonly int new_room_id;
        public readonly int examination_id;
        public readonly int period_id;
        public readonly int old_room_id;

        public RoomChangeNeighbor(Solution solution, int examination_id, int new_room_id)
        {
            this.fitness = -1;
            this.type = 4;
            this.solution = solution;
            this.new_room_id = new_room_id;
            this.examination_id = examination_id;
            this.period_id = solution.GetPeriodFrom(examination_id);
            this.old_room_id = solution.GetRoomFrom(examination_id);
        }

        public Solution Accept()
        {
            solution.UnsetExam(period_id, old_room_id, examination_id);

            solution.SetExam(period_id, new_room_id, examination_id);
            return solution;
        }

        public Solution Reverse()
        {
            solution.UnsetExam(period_id, new_room_id, examination_id);

            solution.SetExam(period_id, old_room_id, examination_id);
            return solution;
        }

        ISolution INeighbor.Accept()
        {
            return Accept();
        }

        ISolution INeighbor.Reverse()
        {
            return Reverse();
        }
    }
}
