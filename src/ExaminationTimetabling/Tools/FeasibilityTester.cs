using Business;
using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Solution.Timetabling;

namespace Tools
{
    public class FeasibilityTester
    {
        public int error_period = -1;
        public int error_room = -1;
        private readonly Examinations examinations;
        private readonly PeriodHardConstraints period_hard_constraints;
        private readonly RoomHardConstraints room_hard_constraints;
        private readonly Rooms rooms;
        private readonly int[,] conflict_matrix;

        public FeasibilityTester()
        {
            examinations = Examinations.Instance();
            rooms = Rooms.Instance();
            room_hard_constraints = RoomHardConstraints.Instance();
            period_hard_constraints = PeriodHardConstraints.Instance();
            conflict_matrix = ConflictMatrix.Instance().Get();
        }

        public bool IsFeasiblePeriod(Solution solution, Examination exam_to_assign, Period period)
        {
            List<int> exam_ids = period_hard_constraints.GetAllExaminationsWithChainingCoincidence(exam_to_assign.id).ToList();

            if (
                !exam_ids.All(
                    exam_id => period.duration >= examinations.GetById(exam_id).duration))
            {
                error_period = 1;
                return false; //exam_to_assign or his coincidences' length cannot surpass the period's length
            }

            foreach (int exam_id in exam_ids)
            {
                if (exam_id == exam_to_assign.id)
                    continue;
                if (solution.GetPeriodFrom(exam_id) != period.id && solution.GetRoomFrom(exam_id) != -1)
                {
                    error_period = 2;
                    return false; //exam_to_assign has COINCIDENCE conflicts with another examination, which is assigned to another period
                }
            }

            foreach (int exam_id in solution.assigned_examinations)
            {
                 if (conflict_matrix[exam_id, exam_to_assign.id] > 0 && solution.GetPeriodFrom(exam_id) == period.id)
                 {
                     error_period = 3;
                     return false; //exam_to_assign has STUDENT or EXCLUSION conflicts with another examination
                 }
            }

            foreach (PeriodHardConstraint phc in period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam_to_assign.id))
            {
                if (phc.ex2 == exam_to_assign.id && solution.GetPeriodFrom(phc.ex1) != -1 && solution.GetPeriodFrom(phc.ex1) <= period.id)
                {
                    error_period = 4;
                    return false; //exam_to_assign must occur AFTER another
                }

                if (phc.ex1 == exam_to_assign.id && solution.GetPeriodFrom(phc.ex2) != -1 && solution.GetPeriodFrom(phc.ex2) >= period.id)
                {
                    error_period = 5;
                    return false; //another examination must occur AFTER exam_to_assign
                }

            }

            for (int room_id = 0; room_id < solution.RoomCount(); room_id++)
            {
                if (IsFeasibleRoom(solution, exam_to_assign, period, rooms.GetById(room_id)))
                {
                    error_period = -1;
                    return true;
                }
            }

            error_period = 6;
            return false;
        }

        public bool IsFeasibleRoom(Solution solution, Examination exam_to_assign, Period period, Room room)
        {
            int room_capacity = RoomCurrentCapacityOnPeriod(solution, period, room);

            if (exam_to_assign.students.Count() > room_capacity)
            {
                error_room = 1;
                return false; //exam_to_assign's number of students must not surpass the CLASSROOM's CAPACITY
            }

            if (room_hard_constraints.HasRoomExclusivity(exam_to_assign.id) &&
                room_capacity != room.capacity)
            {
                error_room = 2;
                return false; //exam_to_assign needs room EXCLUSIVITY
            }

            foreach (RoomHardConstraint rhc in room_hard_constraints.GetByType(RoomHardConstraint.types.ROOM_EXCLUSIVE))
            {
                if (solution.GetPeriodFrom(rhc.examination) == period.id &&
                    solution.GetRoomFrom(rhc.examination) == room.id)
                {
                    error_room = 3;
                    return false; //there's an exam that needs EXCLUSIVITY in this period and room
                }
                    
            }
            error_room = -1;
            return true; //exam_to_assign can be assign
        }

        public int RoomCurrentCapacityOnPeriod(Solution solution, Period period, Room room)
        {
            int room_capacity = room.capacity;

            for (int exam_id = 0; exam_id < solution.ExaminationCount(); exam_id++)
            {
                if (solution.IsExamSetTo(period.id, room.id, exam_id))
                    room_capacity -= examinations.GetById(exam_id).students.Count();
            }

            return room_capacity;
        }
    }
}
