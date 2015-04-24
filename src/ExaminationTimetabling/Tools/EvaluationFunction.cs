using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business;
using DAL;
using DAL.Models;

namespace Tools
{
    public class EvaluationFunction
    {
        private bool[,] conflict_matrix;
        private readonly Examinations examinations;
        private readonly PeriodHardConstraints period_hard_constraints;
        private readonly RoomHardConstraints room_hard_constraints;
        private readonly Rooms rooms;
        private readonly Periods periods;


        public EvaluationFunction(Examinations examinations, PeriodHardConstraints period_hard_constraints, RoomHardConstraints room_hard_constraints,
            Rooms rooms, Periods periods)
        {
            this.examinations = examinations;
            this.period_hard_constraints = period_hard_constraints;
            this.room_hard_constraints = room_hard_constraints;
            this.rooms = rooms;
            this.periods = periods;
            PopulateConflictMatrix();
        }

        public int DistanceToFeasibility(Solution solution)
        {
            int student_conflicts_hc = 0;
            int period_lengths_hc = 0;
            int exam_coincidence_hc = 0;
            int exam_exclusion_hc = 0;
            int exam_after_hc = 0;
            int room_exclusivity_hc = 0;
            int room_capacity_hc = 0;

            if (!IsValid(solution))
                return -1;

            // Student Conflicts

            for (int exam1_id = 0; exam1_id < conflict_matrix.GetLength(0); ++exam1_id)
            {
                for (int exam2_id = exam1_id + 1; exam2_id < conflict_matrix.GetLength(1); ++exam2_id)
                {
                    if (conflict_matrix[exam1_id, exam2_id] && solution.epr_associasion[exam1_id, 0] == solution.epr_associasion[exam2_id, 0])
                    {
                        ++student_conflicts_hc;
                    }
                }

            }

            // Period Lengths Conflicts

            for(int exam_id = 0; exam_id < examinations.EntryCount(); exam_id++)
            {
                if (examinations.GetById(exam_id).duration >
                    periods.GetById(solution.epr_associasion[exam_id, 0]).duration)
                {
                    ++period_lengths_hc;
                }
                    
            }

            // Examination Coincidences

            foreach (
                PeriodHardConstraint phc in
                    period_hard_constraints.GetByType(PeriodHardConstraint.types.EXAM_COINCIDENCE))
            {
                if (solution.epr_associasion[phc.ex1, 0] != solution.epr_associasion[phc.ex2, 0])
                {
                    ++exam_coincidence_hc;
                }

            }

            // Examination Exclusions

            foreach (
                PeriodHardConstraint phc in
                    period_hard_constraints.GetByType(PeriodHardConstraint.types.EXCLUSION))
            {
                if (solution.epr_associasion[phc.ex1, 0] == solution.epr_associasion[phc.ex2, 0])
                {
                    ++exam_exclusion_hc;
                }
            }

            // Examination Afters

            foreach (
                PeriodHardConstraint phc in
                    period_hard_constraints.GetByType(PeriodHardConstraint.types.AFTER))
            {
                if (periods.GetById(solution.epr_associasion[phc.ex1, 0]).date <= periods.GetById(solution.epr_associasion[phc.ex2, 0]).date)
                {
                    ++exam_after_hc;
                }
            }

            // Examination Room Exclusives

            foreach (RoomHardConstraint rhc in room_hard_constraints.GetByType(RoomHardConstraint.types.ROOM_EXCLUSIVE))
            {
                int period_id = solution.epr_associasion[rhc.examination, 0];
                int room_id = solution.epr_associasion[rhc.examination, 1];

                for (int exam_id = 0; exam_id < solution.timetable_container.GetLength(2); ++exam_id)
                {
                    if (solution.timetable_container[period_id, room_id, exam_id] &&
                        rhc.examination != exam_id)
                    {
                        ++room_exclusivity_hc;
                        break;
                    }
                }
            }

            // Room Capacities

            for (int period_id = 0; period_id < solution.timetable_container.GetLength(0); ++period_id)
            {
                for (int room_id = 0; room_id < solution.timetable_container.GetLength(1); ++room_id)
                {
                    int room_capacity_sum = 0;
                    int room_capacity = rooms.GetById(room_id).capacity;

                    for (int exam_id = 0; exam_id < solution.timetable_container.GetLength(2); ++exam_id)
                    {
                        if (solution.timetable_container[period_id, room_id, exam_id])
                        {
                            room_capacity_sum += examinations.GetById(exam_id).students.Count();

                            if (room_capacity_sum > room_capacity)
                            {
                                ++room_capacity_hc;
                                break;
                            }
                        }
                    }
                }
            }

            return exam_after_hc + exam_coincidence_hc + exam_exclusion_hc + period_lengths_hc + room_capacity_hc + room_exclusivity_hc + student_conflicts_hc;
        }

        public int Fitness(Solution solution)
        {
            return -1;
        }

        public bool IsValid(Solution solution)
        {
            if (solution.epr_associasion.GetLength(0) != examinations.EntryCount())
                return false;

            for (int exam_id = 0; exam_id < solution.epr_associasion.GetLength(0); ++exam_id)
            {
                if (solution.epr_associasion[exam_id, 0] == -1 || solution.epr_associasion[exam_id, 0] == -1)
                    return false;
            }

            return true;
        }
        private void PopulateConflictMatrix()
        {
            conflict_matrix = new bool[examinations.EntryCount(), examinations.EntryCount()];

            // student conflicts //
            for (int exam1_id = 0; exam1_id < conflict_matrix.GetLength(0); exam1_id += 1)
            {
                for (int exam2_id = 0; exam2_id < conflict_matrix.GetLength(1); exam2_id += 1)
                {
                    Examination exam1 = examinations.GetById(exam1_id);
                    Examination exam2 = examinations.GetById(exam2_id);
                    if (exam1_id == exam2_id)
                        conflict_matrix[exam1_id, exam2_id] = false;
                    else if (exam1_id > exam2_id)
                        conflict_matrix[exam1_id, exam2_id] = conflict_matrix[exam2_id, exam1_id];
                    else if (examinations.Conflict(exam1, exam2))
                    {
                        conflict_matrix[exam1_id, exam2_id] = true;
                        exam1.conflict += 1;
                        exam2.conflict += 1;
                    }
                    else
                        conflict_matrix[exam1_id, exam2_id] = false;
                }
            }
        }
    }
}
