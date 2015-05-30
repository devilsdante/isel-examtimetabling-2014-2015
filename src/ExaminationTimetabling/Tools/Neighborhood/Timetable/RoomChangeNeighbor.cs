using DAL.Models.Solution;
using DAL.Models.Solution.Timetabling;

namespace Tools.Neighborhood.Timetable
{
    public class RoomChangeNeighbor : INeighbor
    {
        public int fitness { get; set; }
        private readonly Solution solution;
        private readonly int new_room_id;
        private readonly int examination_id;
        private readonly int period_id;
        private readonly int old_room_id;

        public RoomChangeNeighbor(Solution solution, int examination_id, int new_room_id)
        {
            this.fitness = -1;
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
