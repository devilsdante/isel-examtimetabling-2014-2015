using Business;
using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristics
{
    public class GraphColoring
    {
        Examinations examinations;
        PeriodHardConstraints period_hard_constraints;
        Periods periods;
        RoomHardConstraints room_hard_constraints;
        Rooms rooms;
        Solutions solutions;


        public bool[,] conflict_matrix;
        public int[] conflicts;
        public Solution solution;
        public List<Examination> unassigned_examinations;
        public List<Examination> assigned_examinations;

        public GraphColoring(Examinations examinations, PeriodHardConstraints period_hard_constraints, Periods periods, RoomHardConstraints room_hard_constraints, 
            Rooms rooms, Solutions solutions)
        {
            this.examinations = examinations;
            this.period_hard_constraints = period_hard_constraints;
            this.periods = periods;
            this.room_hard_constraints = room_hard_constraints;
            this.rooms = rooms;
            this.solutions = solutions;
        }

        public void Work()
        {
            conflict_matrix = new bool[examinations.EntryCount(), examinations.EntryCount()];

            conflicts = new int[examinations.EntryCount()];

            PopulateConflictMatrix();

            EraseCoincidenceHCWithConflict();

            //SolutionInitialization();

            //PeriodSelection();
        }

        private void PopulateConflictMatrix()
        {
            // student conflicts //
            for (int x = 0; x < conflict_matrix.GetLength(0); x += 1)
            {
                for (int y = 0; y < conflict_matrix.GetLength(1); y += 1)
                {
                    if (x == y)
                        conflict_matrix[x, y] = false;
                    else if (x > y)
                        conflict_matrix[x, y] = conflict_matrix[y, x];
                    else if (examinations.Conflict(x, y))
                    {
                        conflict_matrix[x, y] = true;
                        conflicts[x] += 1;
                        conflicts[y] += 1;
                    }
                    else
                        conflict_matrix[x, y] = false;
                }
            }

            // exclusion hard constraints //
            IEnumerable<PeriodHardConstraint> exclusions = period_hard_constraints.GetByType(PeriodHardConstraint.types.EXCLUSION);
            foreach (PeriodHardConstraint phc in exclusions)
            {
                if (conflict_matrix[phc.ex1, phc.ex2] == false)
                {
                    conflict_matrix[phc.ex1, phc.ex2] = true;
                    conflict_matrix[phc.ex2, phc.ex1] = true;

                    conflicts[phc.ex1] += 1;
                    conflicts[phc.ex2] += 1;
                }
            }
        }
        private void EraseCoincidenceHCWithConflict()
        {
            foreach (PeriodHardConstraint coincidence in period_hard_constraints.GetByType(PeriodHardConstraint.types.EXAM_COINCIDENCE))
            {
                if (conflict_matrix[coincidence.ex1, coincidence.ex2])
                {
                    period_hard_constraints.Delete(coincidence);
                }
            }
        }

        private int IsFeasiblePeriod(int exam_to_assign, int period)
        {
            if (periods.GetById(period).duration > examinations.GetById(exam_to_assign).duration)
            {
                return -1; //exam_to_assign doesn't fit the period
            }

            // TODO COINCIDENCE

            for (int x = 0; x < conflict_matrix.GetLength(0); x += 1)
            {
                if (conflict_matrix[x, exam_to_assign] && solution.epr_associasion[x,0] == period)
                {
                    return -1; //exam_to_assign has conflict(s) with another examination(s) on this period
                }
            }

            foreach (PeriodHardConstraint phc in period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam_to_assign))
            {
                if (phc.ex2 == exam_to_assign && solution.epr_associasion[phc.ex1, 0] <= period)
                {
                    return -1; //exam_to_assign cannot occur AFTER another
                }

                if (phc.ex1 == exam_to_assign && solution.epr_associasion[phc.ex2, 0] >= period)
                {
                    return -1; //another examination cannot occur AFTER exam_to_assign
                }

            }

            for (int room = 0; room < solution.timetable_container.GetLength(1); room++)
            {
                int room_capacity = rooms.GetById(room).capacity;
                
                for (int exam = 0; exam < solution.timetable_container.GetLength(2); exam++)
                {
                    if (solution.timetable_container[period, room, exam])
                        room_capacity -= examinations.GetById(exam).students.Count();
                }

                if (examinations.GetById(exam_to_assign).students.Count() > room_capacity)
                    continue; //exam_to_assign's students don't fit the classroom's capacity

                return room; //exam_to_assign can be assign


            }
            return -1;
        }
        //private void SolutionInitialization()
        //{
        //    solution = new Solution(1,1,1,1); //1 to change TODO
        //    foreach (Period period in periods.GetAll())
        //    {
        //        solution.periods_map.Add(period, new List<ExaminationRoomRel>());
        //    }
        //}
        //private void PeriodSelection()
        //{
        //    unassigned_examinations = examinations.GetAll().ToList();
        //    assigned_examinations = new List<Examination>();

        //    while (unassigned_examinations.Any())
        //    {
        //        List<Period> possible_periods;
        //        Examination exam_to_assign = unassigned_examinations.First();

        //        foreach (Examination exam in unassigned_examinations)
        //        {
        //            if (conflicts[exam.id] > conflicts[exam_to_assign.id])
        //            {
        //                exam_to_assign = exam;
        //            }
        //        }

        //        // Most common option
        //        if (!period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam_to_assign.id).Any()
        //            &&
        //            !period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE, exam_to_assign.id).Any())
        //        {
        //            possible_periods = periods.GetAllThatFitsDuration(exam_to_assign.id).ToList();

        //            if (GetPeriodsWithoutStudentOrExclusiveConflicts(exam_to_assign, possible_periods).Any())
        //            {
        //                //Examination can be assign TODO
        //            }
        //            else
        //            {
        //                //Examination can not be assign TODO
        //            }
        //        }
        //        // With AFTER and/or EXAM_COINCIDENCE Constraints TODO
        //        else
        //        {
                    
        //        }


        //    }

        //}


    }
}
