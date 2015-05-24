namespace DAL.Models.Solution.Timetabling
{
    public class Solution : ISolution
    {
        public int id { get; set; }
        public bool[,,] timetable_container { get; set; }
        public int[,] epr_associasion; //idx = exam; 0 = period; 1 = room
        public int fitness { get; set; }


        public Solution(int id, int period_count, int room_count, int examination_count)
        {
            this.id = id;
            fitness = -1;

            timetable_container = new bool[period_count, room_count, examination_count];
            epr_associasion = new int[examination_count,2];

            for(int i = 0; i < examination_count; i++)
            {
                epr_associasion[i, 0] = -1;
                epr_associasion[i, 1] = -1;
            }
        }

        public ISolution Copy()
        {
            Solution solution = new Solution(id, timetable_container.GetLength(0), timetable_container.GetLength(1), timetable_container.GetLength(2));
            solution.timetable_container = (bool[,,]) timetable_container.Clone();
            solution.epr_associasion = (int[,]) epr_associasion.Clone();
            return solution;
        }

        public bool Equals(ISolution solution)
        {
            return Equals((Solution)solution);
        }

        public bool Equals(Solution obj)
        {
            if (id != obj.id)
                return false;

            for (int i = 0; i < timetable_container.GetLength(0); i++)
            {
                for (int j = 0; j < timetable_container.GetLength(1); j++)
                {
                    for (int h = 0; h < timetable_container.GetLength(2); h++)
                    {
                        if (timetable_container[i, j, h] != obj.timetable_container[i, j, h])
                            return false;
                    }
                }
            }

            for (int i = 0; i < epr_associasion.GetLength(0); i++)
            {
                if (epr_associasion[i, 0] != obj.epr_associasion[i, 0] ||
                    epr_associasion[i, 1] != obj.epr_associasion[i, 1])
                {
                    return false;
                }
            }

            return true;
        }
    }

}
