using DAL.Models.Solution;
using DAL.Models.Solution.Timetabling;

namespace Tools.Neighborhood.Timetable
{
    class PeriodRoomSwapNeighbor:INeighbor
    {
        public int fitness { get; set; }
        private readonly Solution solution;
        private readonly int examination1_id;
        private readonly int examination2_id;
        private readonly int period1_id;
        private readonly int period2_id;
        private readonly int room1_id;
        private readonly int room2_id;

        public PeriodRoomSwapNeighbor(Solution solution, int examination1_id, int examination2_id)
        {
            this.fitness = -1;
            this.solution = solution;
            this.examination1_id = examination1_id;
            this.examination2_id = examination2_id;
            this.period1_id = solution.GetPeriodFrom(examination1_id);
            this.period2_id = solution.GetPeriodFrom(examination2_id);
            this.room1_id = solution.GetRoomFrom(examination1_id);
            this.room2_id = solution.GetRoomFrom(examination2_id);
        }

        public Solution Accept()
        {
            solution.UnsetExam(period1_id, room1_id, examination1_id);
            solution.UnsetExam(period2_id, room2_id, examination2_id);

            solution.SetExam(period2_id, room2_id, examination1_id);
            solution.SetExam(period1_id, room1_id, examination2_id);

            return solution;
        }

        public Solution Reverse()
        {
            solution.UnsetExam(period2_id, room2_id, examination1_id);
            solution.UnsetExam(period1_id, room1_id, examination2_id);

            solution.SetExam(period1_id, room1_id, examination1_id);
            solution.SetExam(period2_id, room2_id, examination2_id);

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
