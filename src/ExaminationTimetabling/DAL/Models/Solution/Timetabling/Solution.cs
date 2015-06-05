using System.Collections.Generic;

namespace DAL.Models.Solution.Timetabling
{
    public class Solution : ISolution
    {
        public int id { get; set; }
        private bool[,,] timetable_container { get; set; }
        private int[,] epr_associasion; //idx = exam; 0 = period; 1 = room
        public int fitness { get; set; }

        private readonly int period_count;
        private readonly int room_count;
        private readonly int examination_count;

        public List<int> assigned_examinations;

        public Solution(int id, int period_count, int room_count, int examination_count)
        {
            this.id = id;
            this.fitness = -1;

            timetable_container = new bool[period_count, room_count, examination_count];
            epr_associasion = new int[examination_count,2];

            for(int i = 0; i < examination_count; i++)
            {
                epr_associasion[i, 0] = -1;
                epr_associasion[i, 1] = -1;
            }

            this.period_count = period_count;
            this.room_count = room_count;
            this.examination_count = examination_count;
            this.assigned_examinations = new List<int>(examination_count);
        }

        public int AssignedExaminations()
        {
            return assigned_examinations.Count;
        }

        public int ExaminationCount()
        {
            return examination_count;
        }

        public int RoomCount()
        {
            return room_count;
        }

        public int PeriodCount()
        {
            return period_count;
        }

        public void SetExam(int period_id, int room_id, int exam_id)
        {
            timetable_container[period_id, room_id, exam_id] = true;
            epr_associasion[exam_id, 0] = period_id;
            epr_associasion[exam_id, 1] = room_id;

            assigned_examinations.Add(exam_id);
        }

        public void UnsetExam(int period_id, int room_id, int exam_id)
        {
            timetable_container[period_id, room_id, exam_id] = false;
            epr_associasion[exam_id, 0] = -1;
            epr_associasion[exam_id, 1] = -1;

            assigned_examinations.Remove(exam_id);
        }

        public int GetPeriodFrom(int exam_id)
        {
            return epr_associasion[exam_id, 0];
        }

        public int GetRoomFrom(int exam_id)
        {
            return epr_associasion[exam_id, 1];
        }

        public bool IsExamSetTo(int period_id, int room_id, int exam_id)
        {
            return timetable_container[period_id, room_id, exam_id];
        }

        public ISolution Copy()
        {
            Solution solution = new Solution(id, period_count, room_count, examination_count);
            solution.timetable_container = (bool[,,]) timetable_container.Clone();
            solution.epr_associasion = (int[,]) epr_associasion.Clone();
            solution.assigned_examinations = assigned_examinations;
            solution.fitness = fitness;

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
