﻿using Business;
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
        private int error = -1;
        private readonly Examinations examinations;
        private readonly PeriodHardConstraints period_hard_constraints;
        private readonly RoomHardConstraints room_hard_constraints;
        private readonly Rooms rooms;
        private readonly bool[,] conflict_matrix;

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
            List<int> exam_ids = (List<int>)period_hard_constraints.GetAllExaminationsWithChainingCoincidence(exam_to_assign.id);
            if (
                !exam_ids.All(
                    exam_id => period.duration >= examinations.GetById(exam_id).duration))
            {
                error = 1;
                return false; //exam_to_assign or his coincidences' length cannot surpass the period's length
            }
                

            foreach (
                PeriodHardConstraint phc in
                    period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE,
                        exam_to_assign.id))
            {
                if (phc.ex1 == exam_to_assign.id && solution.epr_associasion[phc.ex2, 0] != period.id && solution.epr_associasion[phc.ex2, 0] != -1
                    || phc.ex2 == exam_to_assign.id && solution.epr_associasion[phc.ex1, 0] != period.id && solution.epr_associasion[phc.ex1, 0] != -1)
                {
                    error = 2;
                    return false; //exam_to_assign has COINCIDENCE conflicts with another examination
                }
            }

            for (int exam_id = 0; exam_id < conflict_matrix.GetLength(0); exam_id += 1)
            {
                if (conflict_matrix[exam_id, exam_to_assign.id] && solution.epr_associasion[exam_id, 0] == period.id)
                {
                    error = 3;
                    return false; //exam_to_assign has STUDENT or EXCLUSION conflicts with another examination
                }
            }

            foreach (PeriodHardConstraint phc in period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam_to_assign.id))
            {
                if (phc.ex2 == exam_to_assign.id && solution.epr_associasion[phc.ex1, 0] != -1 && solution.epr_associasion[phc.ex1, 0] <= period.id)
                {
                    error = 4;
                    return false; //exam_to_assign must occur AFTER another
                }

                if (phc.ex1 == exam_to_assign.id && solution.epr_associasion[phc.ex2, 0] != -1 && solution.epr_associasion[phc.ex2, 0] >= period.id)
                {
                    error = 5;
                    return false; //another examination must occur AFTER exam_to_assign
                }

            }

            for (int room_id = 0; room_id < solution.timetable_container.GetLength(1); room_id++)
            {
                if (IsFeasibleRoom(solution, exam_to_assign, period, rooms.GetById(room_id)))
                {
                    error = -1;
                    return true;
                }
                    
            }
            error = 6;
            return false;
        }

        public bool IsFeasibleRoom(Solution solution, Examination exam_to_assign, Period period, Room room)
        {
            int room_capacity = RoomCurrentCapacityOnPeriod(solution, period, room);

            if (exam_to_assign.students.Count() > room_capacity)
                return false; //exam_to_assign's number of students must not surpass the CLASSROOM's CAPACITY

            if (room_hard_constraints.HasRoomExclusivesWithExam(exam_to_assign.id) &&
                room_capacity != room.capacity)
                return false; //exam_to_assign needs room EXCLUSIVITY

            foreach (RoomHardConstraint rhc in room_hard_constraints.GetByType(RoomHardConstraint.types.ROOM_EXCLUSIVE))
            {
                if (solution.epr_associasion[rhc.examination, 0] == period.id &&
                    solution.epr_associasion[rhc.examination, 1] == room.id)
                    return false; //there's an exam that needs EXCLUSIVITY in this period and room
            }

            return true; //exam_to_assign can be assign
        }

        public int RoomCurrentCapacityOnPeriod(Solution solution, Period period, Room room)
        {
            int room_capacity = room.capacity;

            for (int exam_id = 0; exam_id < solution.timetable_container.GetLength(2); exam_id++)
            {
                if (solution.timetable_container[period.id, room.id, exam_id])
                    room_capacity -= examinations.GetById(exam_id).students.Count();
            }

            return room_capacity;
        }
    }
}
