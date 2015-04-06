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

        public GraphColoring(Examinations examinations, PeriodHardConstraints period_hard_constraints,
            Periods periods, RoomHardConstraints room_hard_constraints, Rooms rooms, Solutions solutions)
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

            SolutionInitialization();

            PeriodSelection();

            RoomSelection();
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
        private void SolutionInitialization()
        {
            solution = new Solution(1); //1 to change TODO
            foreach (Period period in periods.GetAll())
            {
                solution.periods_map.Add(period, new List<ExaminationRoomRel>());
            }
        }
        private void PeriodSelection()
        {
            unassigned_examinations = examinations.GetAll().ToList();
            assigned_examinations = new List<Examination>();

            while (unassigned_examinations.Any())
            {
                List<Period> possible_periods;
                Examination exam_to_assign = unassigned_examinations.First();

                foreach (Examination exam in unassigned_examinations)
                {
                    if (conflicts[exam.id] > conflicts[exam_to_assign.id])
                    {
                        exam_to_assign = exam;
                    }
                }

                // Most common option
                if (!period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam_to_assign.id).Any()
                    &&
                    !period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE, exam_to_assign.id).Any())
                {
                    possible_periods = periods.GetAllThatFitsDuration(exam_to_assign.id).ToList();

                    if (GetPeriodsWithoutStudentOrExclusiveConflicts(exam_to_assign, possible_periods).Any())
                    {
                        //Examination can be assign TODO
                    }
                    else
                    {
                        //Examination can not be assign TODO
                    }
                }
                // With AFTER and/or EXAM_COINCIDENCE Constraints TODO
                else
                {
                    
                }

                
            }
            
        }

        private void RoomSelection()
        {

        }

        private bool HaveConflicts(Examination exam1, Examination exam2)
        {

            return true;
        }

        private List<Period> GetPeriodsWithoutStudentOrExclusiveConflicts(Examination exam_to_assign, IEnumerable<Period> periods)
        {
            List<Period> to_return = periods.ToList();

            foreach (Period period in periods.Where(period => solution.periods_map[period].Any(exam_room => HaveConflicts(exam_to_assign, exam_room.exam))))
            {
                to_return.Remove(period);
            }
            
            return to_return;
        }
    }
}
