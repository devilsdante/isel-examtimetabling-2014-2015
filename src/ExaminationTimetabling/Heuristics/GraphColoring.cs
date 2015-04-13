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
        public Solution solution;
        public List<Examination> unassigned_examinations;
        public List<Examination> unassigned_examinations_with_after;
        public List<Examination> unassigned_examinations_with_coincidence;
        public List<Examination> unassigned_examinations_with_exclusive;
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

            PopulateConflictMatrix();

            EraseCoincidenceHCWithConflict();

            PopulateAndSortAssignmentLists();

            while (assigned_examinations.Count != examinations.EntryCount())
            {
                List<Examination> list_to_use;
                Examination exam_to_assign;

                if (unassigned_examinations_with_exclusive.Any())
                    list_to_use = unassigned_examinations_with_exclusive;
                else if (unassigned_examinations_with_after.Any())
                    list_to_use = unassigned_examinations_with_after;
                else if (unassigned_examinations_with_coincidence.Any())
                    list_to_use = unassigned_examinations_with_coincidence;
                else
                    list_to_use = unassigned_examinations;

                exam_to_assign = list_to_use.Last();
                list_to_use.RemoveAt(unassigned_examinations_with_exclusive.Count - 1);

                
                if (CheckIfExaminationCanBeAssignToAnyPeriod(exam_to_assign))
                {
                    ExaminationNormalAssignment(exam_to_assign);
                }
                else
                {
                    ExaminationForcingAssignment(exam_to_assign);
                }
                
            }
        }

        private bool CheckIfExaminationCanBeAssignToAnyPeriod(Examination exam_to_assign)
        {
            return periods.GetAll().Any(period => IsFeasiblePeriod(exam_to_assign.id, period.id) >= 0);
        }

        private void PopulateAndSortAssignmentLists()
        {
            foreach (Examination exam in examinations.GetAll())
            {
                if (room_hard_constraints.HasRoomExclusivesWithExam(exam.id))
                    unassigned_examinations_with_exclusive.Add(exam);
                else if(period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam.id).Any())
                    unassigned_examinations_with_after.Add(exam);
                else if(period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE, exam.id).Any())
                    unassigned_examinations_with_coincidence.Add(exam);
                else
                    unassigned_examinations.Add(exam);
            }
            unassigned_examinations_with_exclusive = unassigned_examinations_with_exclusive.OrderBy(ex => ex.conflict).ToList();
            unassigned_examinations_with_after = unassigned_examinations_with_after.OrderBy(ex => ex.conflict).ToList();
            unassigned_examinations_with_coincidence = unassigned_examinations_with_coincidence.OrderBy(ex => ex.conflict).ToList();
            unassigned_examinations = unassigned_examinations.OrderBy(ex => ex.conflict).ToList();
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
                        examinations.GetById(x).conflict += 1;
                        examinations.GetById(y).conflict += 1;
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

                    examinations.GetById(phc.ex1).conflict += 1;
                    examinations.GetById(phc.ex2).conflict += 1;
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
                return -1; //exam_to_assign's length cannot surpass the period's length
            }

            foreach (
                PeriodHardConstraint phc in
                    period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE,
                        exam_to_assign))
            {
                if (phc.ex1 == exam_to_assign && solution.epr_associasion[phc.ex2, 0] != period
                    || phc.ex2 == exam_to_assign && solution.epr_associasion[phc.ex1, 0] != period)
                {
                    return -1; //exam_to_assign has COINCIDENCE conflicts with another examination
                }
            }

            for (int x = 0; x < conflict_matrix.GetLength(0); x += 1)
            {
                if (conflict_matrix[x, exam_to_assign] && solution.epr_associasion[x,0] == period)
                {
                    return -1; //exam_to_assign has STUDENT or EXCLUSION conflicts with another examination
                }
            }

            foreach (PeriodHardConstraint phc in period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam_to_assign))
            {
                if (phc.ex2 == exam_to_assign && solution.epr_associasion[phc.ex1, 0] <= period)
                {
                    return -1; //exam_to_assign must occur AFTER another
                }

                if (phc.ex1 == exam_to_assign && solution.epr_associasion[phc.ex2, 0] >= period)
                {
                    return -1; //another examination must occur AFTER exam_to_assign
                }

            }

            for (int room = 0; room < solution.timetable_container.GetLength(1); room++)
            {
                if (IsFeasibleRoom(exam_to_assign, period, room))
                    return room;
            }
            return -1;
        }

        private bool IsFeasibleRoom(int exam_to_assign, int period, int room)
        {
            int room_capacity = rooms.GetById(room).capacity;

            for (int exam = 0; exam < solution.timetable_container.GetLength(2); exam++)
            {
                if (solution.timetable_container[period, room, exam])
                    room_capacity -= examinations.GetById(exam).students.Count();
            }

            if (room_hard_constraints.HasRoomExclusivesWithExam(exam_to_assign) &&
                room_capacity == rooms.GetById(room).capacity)
                return false; //exam_to_assign needs room EXCLUSIVITY

            if (examinations.GetById(exam_to_assign).students.Count() > room_capacity)
                return false; //exam_to_assign's number of students must surpass the CLASSROOM's CAPACITY

            return true; //exam_to_assign can be assign
        }

        private void ExaminationNormalAssignment(Examination exam_to_assign)
        {
            int n_of_feasibles = periods.GetAll().Count(period => IsFeasiblePeriod(exam_to_assign.id, period.id) >= 0);
            Random random = new Random();
            int random_assignable = random.Next(n_of_feasibles);
            Period period_to_assign = null;

            foreach (Period period in periods.GetAll())
            {
                if (IsFeasiblePeriod(exam_to_assign.id, period.id) >= 0)
                {
                    if (--random_assignable < 0)
                    {
                        period_to_assign = period;
                        break;
                    }
                }
            }

            if(period_to_assign == null)
                throw new NullReferenceException();      //Never happens

            n_of_feasibles = rooms.GetAll().Count(room => IsFeasibleRoom(exam_to_assign.id, period_to_assign.id, room.id));
            random_assignable = random.Next(n_of_feasibles);
            Room room_to_assign = null;

            foreach (Room room in rooms.GetAll())
            {
                if (IsFeasibleRoom(exam_to_assign.id, period_to_assign.id, room.id))
                {
                    if (--random_assignable < 0)
                    {
                        room_to_assign = room;
                        break;
                    }
                }
            }

            if (room_to_assign == null)
                throw new NullReferenceException();      //Never happens

            solution.timetable_container[period_to_assign.id, room_to_assign.id, exam_to_assign.id] = true;
            solution.epr_associasion[exam_to_assign.id, 0] = period_to_assign.id;
            solution.epr_associasion[exam_to_assign.id, 1] = room_to_assign.id;
        }

        private void ExaminationForcingAssignment(Examination exam_to_assign)
        {
            
        }
    }
}
