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
            this.period1_id = solution.epr_associasion[examination1_id, 0];
            this.period2_id = solution.epr_associasion[examination2_id, 0];
            this.room1_id = solution.epr_associasion[examination1_id, 1];
            this.room2_id = solution.epr_associasion[examination2_id, 1];
        }

        public Solution Accept()
        {
            solution.timetable_container[period1_id, room1_id, examination1_id] = false;
            solution.timetable_container[period2_id, room2_id, examination2_id] = false;

            solution.timetable_container[period2_id, room2_id, examination1_id] = true;
            solution.timetable_container[period1_id, room1_id, examination2_id] = true;

            solution.epr_associasion[examination1_id, 0] = period2_id;
            solution.epr_associasion[examination2_id, 0] = period1_id;
            solution.epr_associasion[examination1_id, 1] = room2_id;
            solution.epr_associasion[examination2_id, 1] = room1_id;
            return solution;
        }

        public Solution Reverse()
        {
            solution.timetable_container[period1_id, room1_id, examination1_id] = true;
            solution.timetable_container[period2_id, room2_id, examination2_id] = true;

            solution.timetable_container[period2_id, room2_id, examination1_id] = false;
            solution.timetable_container[period1_id, room1_id, examination2_id] = false;

            solution.epr_associasion[examination1_id, 0] = period1_id;
            solution.epr_associasion[examination2_id, 0] = period2_id;
            solution.epr_associasion[examination1_id, 1] = room1_id;
            solution.epr_associasion[examination2_id, 1] = room2_id;
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
