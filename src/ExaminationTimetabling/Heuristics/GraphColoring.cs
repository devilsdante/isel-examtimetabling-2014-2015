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
                
                if(unassigned_examinations.Count() 
                    + unassigned_examinations_with_after.Count() 
                    + unassigned_examinations_with_coincidence.Count() 
                    + unassigned_examinations_with_exclusive.Count() 
                    + assigned_examinations.Count() 
                    != examinations.EntryCount())
                    throw new Exception("Examinations lists size mismatch");
            }

            solutions.Insert(solution);
        }

        private bool CheckIfExaminationCanBeAssignToAnyPeriod(Examination exam_to_assign)
        {
            return periods.GetAll().Any(period => IsFeasiblePeriod(exam_to_assign, period));
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

        private bool IsFeasiblePeriod(Examination exam_to_assign, Period period)
        {
            List<int> exam_ids = (List<int>)period_hard_constraints.GetAllExaminationsWithChainingCoincidence(exam_to_assign.id);
            if (
                !exam_ids.All(
                    exam_id => period.duration >= examinations.GetById(exam_id).duration))
                return false; //exam_to_assign or his coincidences' length cannot surpass the period's length

            foreach (
                PeriodHardConstraint phc in
                    period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE,
                        exam_to_assign.id))
            {
                if (phc.ex1 == exam_to_assign.id && solution.epr_associasion[phc.ex2, 0] != period.id
                    || phc.ex2 == exam_to_assign.id && solution.epr_associasion[phc.ex1, 0] != period.id)
                {
                    return false; //exam_to_assign has COINCIDENCE conflicts with another examination
                }
            }

            for (int exam_id = 0; exam_id < conflict_matrix.GetLength(0); exam_id += 1)
            {
                if (conflict_matrix[exam_id, exam_to_assign.id] && solution.epr_associasion[exam_id, 0] == period.id)
                {
                    return false; //exam_to_assign has STUDENT or EXCLUSION conflicts with another examination
                }
            }

            foreach (PeriodHardConstraint phc in period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam_to_assign.id))
            {
                if (phc.ex2 == exam_to_assign.id && solution.epr_associasion[phc.ex1, 0] <= period.id)
                {
                    return false; //exam_to_assign must occur AFTER another
                }

                if (phc.ex1 == exam_to_assign.id && solution.epr_associasion[phc.ex2, 0] >= period.id)
                {
                    return false; //another examination must occur AFTER exam_to_assign
                }

            }

            for (int room_id = 0; room_id < solution.timetable_container.GetLength(1); room_id++)
            {
                if (IsFeasibleRoom(exam_to_assign, period, rooms.GetById(room_id)))
                    return true;
            }
            return false;
        }

        private bool IsFeasibleRoom(Examination exam_to_assign, Period period, Room room)
        {
            int room_capacity = RoomCurrentCapacityOnPeriod(period, room);

            if (room_hard_constraints.HasRoomExclusivesWithExam(exam_to_assign.id) &&
                room_capacity != room.capacity)
                return false; //exam_to_assign needs room EXCLUSIVITY

            if (exam_to_assign.students.Count() > room_capacity)
                return false; //exam_to_assign's number of students must surpass the CLASSROOM's CAPACITY

            return true; //exam_to_assign can be assign
        }

        private void ExaminationNormalAssignment(Examination exam_to_assign)
        {
            int n_of_feasibles = periods.GetAll().Count(period => IsFeasiblePeriod(exam_to_assign, period));
            Random random = new Random();
            int random_assignable = random.Next(n_of_feasibles);
            Period period_to_assign = null;

            foreach (Period period in periods.GetAll())
            {
                if (IsFeasiblePeriod(exam_to_assign, period))
                {
                    if (--random_assignable < 0)
                    {
                        period_to_assign = period;
                        break;
                    }
                }
            }

            if(period_to_assign == null)
                throw new NullReferenceException("Period was not successfully assigned");

            n_of_feasibles = rooms.GetAll().Count(room => IsFeasibleRoom(exam_to_assign, period_to_assign, room));
            random_assignable = random.Next(n_of_feasibles);
            Room room_to_assign = null;

            foreach (Room room in rooms.GetAll())
            {
                if (IsFeasibleRoom(exam_to_assign, period_to_assign, room))
                {
                    if (--random_assignable < 0)
                    {
                        room_to_assign = room;
                        break;
                    }
                }
            }

            if (room_to_assign == null)
                throw new NullReferenceException("Room was not successfully assigned");

            AssignExamination(period_to_assign, room_to_assign, exam_to_assign);
        }

        private void ExaminationForcingAssignment(Examination exam_to_assign)
        {
            Random random = new Random();
            int random_room = -1;
            int random_period = -1;
            Room room_to_assign = null;
            Period period_to_assign = null;

            //Default room's capacity must be higher than the examination's number of students
            while (true)
            {
                random_room = random.Next(rooms.EntryCount());
                if (rooms.GetById(random_room).capacity >=
                    examinations.GetById(exam_to_assign.id).students.Count())
                {
                    room_to_assign = rooms.GetById(random_room);
                    break;
                }
                    
            }

            if (room_to_assign == null)
                throw new NullReferenceException("Room was not successfully assigned");

            List<int> exam_ids = (List<int>)period_hard_constraints.GetAllExaminationsWithChainingCoincidence(exam_to_assign.id);

            //If there's a coincident examination assigned, the exam_to_assign will be forced to be put on the same period as its coincident
            foreach (int exam_id in exam_ids)
            {
                if (solution.epr_associasion[exam_id, 0] != -1)
                {
                    random_period = solution.epr_associasion[exam_id, 0];
                    period_to_assign = periods.GetById(random_period);
                    break;
                }
                    
            }

            //If not - Period's length must fit examination's duration and all its coincidences
            if(random_period != -1)
            {
                while (true)
                {
                    random_period = random.Next(periods.EntryCount());

                    if (
                        exam_ids.All(
                            exam_id => periods.GetById(random_period).duration >= examinations.GetById(exam_id).duration))
                    {
                        period_to_assign = periods.GetById(random_period);
                        break;
                    }
                        
                }
            }

            if (period_to_assign == null)
                throw new NullReferenceException("Period was not successfully assigned");
            

            //Unassigning all examinations conflicting with the new assignment (students and EXCLUSION) on the period
            for (int room = 0; room < rooms.EntryCount(); room++)
            {
                for (int exam = 0; exam < examinations.EntryCount(); exam++)
                {
                    if (solution.timetable_container[random_period, room, exam] &&
                        conflict_matrix[exam_to_assign.id, exam])
                            UnassignExaminationAndCoincidences(examinations.GetById(exam));
                }
            }

            //If exam_to_assign needs room exclusivity, unassign all examinations taking place in the period at the room
            if (room_hard_constraints.HasRoomExclusivesWithExam(exam_to_assign.id))
            {
                for (int exam_id = 0; exam_id < examinations.EntryCount(); exam_id++)
                {
                    if (solution.timetable_container[random_period, random_room, exam_id])
                    {
                        UnassignExaminationAndCoincidences(examinations.GetById(exam_id));
                    }
                }
            }
            //All examinations taking place in the period at the room that needs room exclusivity, must be unassigned
            else
            {
                for (int exam_id = 0; exam_id < examinations.EntryCount(); exam_id++)
                {
                    if (solution.timetable_container[random_period, random_room, exam_id] && room_hard_constraints.HasRoomExclusivesWithExam(exam_id))
                    {
                        UnassignExaminationAndCoincidences(examinations.GetById(exam_id));
                    }
                }
            }

            //All examinations with AFTER conflicts with exam_to_assign, must be unassigned
            foreach (
                PeriodHardConstraint phc in
                    period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam_to_assign.id))
            {
                if (phc.ex1 == exam_to_assign.id 
                    && solution.epr_associasion[phc.ex2, 0] != -1
                    && solution.epr_associasion[phc.ex2, 1] != -1
                    && solution.epr_associasion[phc.ex2, 0] >=  solution.epr_associasion[phc.ex1, 0])
                {
                    UnassignExaminationAndCoincidences(examinations.GetById(phc.ex2));
                }
                else if (phc.ex2 == exam_to_assign.id
                    && solution.epr_associasion[phc.ex1, 0] != -1
                    && solution.epr_associasion[phc.ex1, 1] != -1
                    && solution.epr_associasion[phc.ex1, 0] <= solution.epr_associasion[phc.ex2, 0])
                {
                    UnassignExaminationAndCoincidences(examinations.GetById(phc.ex2));
                }
            }

            //Removes examinations from period and room, until the room has enough capacity for the new examination to take place
            int room_to_assign_curr_capacity =
                RoomCurrentCapacityOnPeriod(period_to_assign, room_to_assign);
            int n_of_examinations_in_period_and_room = 0;

            while (room_to_assign_curr_capacity < exam_to_assign.students.Count())
            {
                if (n_of_examinations_in_period_and_room == 0)
                {
                    for (int exam_id = 0; exam_id < examinations.EntryCount(); exam_id++)
                    {
                        if (solution.timetable_container[random_period, random_room, exam_id])
                        {
                            ++n_of_examinations_in_period_and_room;
                        }
                    }
                }
                
                int random_examination_unassignment = random.Next(n_of_examinations_in_period_and_room);
                for (int exam_id = 0; exam_id < examinations.EntryCount(); exam_id++)
                {
                    if (--random_examination_unassignment < 0)
                    {
                        --n_of_examinations_in_period_and_room;
                        room_to_assign_curr_capacity += examinations.GetById(exam_id).students.Count();
                        UnassignExaminationAndCoincidences(examinations.GetById(exam_id));
                        break;
                    }
                }
            }

            if (!IsFeasiblePeriod(exam_to_assign, period_to_assign) ||
                !IsFeasibleRoom(exam_to_assign, period_to_assign, room_to_assign))
                throw new Exception("Period and room are not feasible but should've been");


            AssignExamination(period_to_assign, room_to_assign, exam_to_assign);
        }

        private void UnassignExamination(Examination exam)
        {
            solution.timetable_container[
                solution.epr_associasion[exam.id, 0], solution.epr_associasion[exam.id, 1], exam.id] = false;

            solution.epr_associasion[exam.id, 0] = -1;
            solution.epr_associasion[exam.id, 1] = -1;

            //Why not random?
            Random random = new Random();
            if (room_hard_constraints.HasRoomExclusivesWithExam(exam.id))
                unassigned_examinations_with_exclusive.Insert(
                    random.Next(unassigned_examinations_with_exclusive.Count), exam);
            else if (period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam.id).Any())
                unassigned_examinations_with_after.Insert(
                    random.Next(unassigned_examinations_with_after.Count), exam);
            else if (period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE, exam.id).Any())
                unassigned_examinations_with_coincidence.Insert(
                    random.Next(unassigned_examinations_with_coincidence.Count), exam);
            else
                unassigned_examinations.Insert(
                    random.Next(unassigned_examinations.Count), exam);

            assigned_examinations.Remove(exam);
        }

        private void UnassignExaminationAndCoincidences(Examination exam)
        {
            UnassignExamination(exam);

            foreach (
                PeriodHardConstraint phc in
                    period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE, exam.id))
            {
                int exam2 = phc.ex1 == exam.id ? phc.ex2 : phc.ex1;

                if (solution.epr_associasion[exam2, 0] != -1 || solution.epr_associasion[exam2, 1] != -1)
                    UnassignExaminationAndCoincidences(examinations.GetById(exam2));
            }
        }

        private void AssignExamination(Period period, Room room, Examination exam)
        {
            solution.timetable_container[period.id, room.id, exam.id] = true;
            solution.epr_associasion[exam.id, 0] = period.id;
            solution.epr_associasion[exam.id, 1] = room.id;
            if (unassigned_examinations.Remove(exam)) {}
            else if (unassigned_examinations_with_after.Remove(exam)) {}
            else if (unassigned_examinations_with_coincidence.Remove(exam)) {}
            else unassigned_examinations_with_exclusive.Remove(exam);
            assigned_examinations.Add(exam);
        }

        private int RoomCurrentCapacityOnPeriod(Period period, Room room)
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
