﻿using Business;
using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Solution.Timetabling;
using Tools;

namespace Heuristics
{
    public class GraphColoring
    {
        private readonly Examinations examinations;
        private readonly PeriodHardConstraints period_hard_constraints;
        private readonly Periods periods;
        private readonly RoomHardConstraints room_hard_constraints;
        private readonly Rooms rooms;

        private readonly bool[,] conflict_matrix;
        private FeasibilityTester feasibility_tester;
        private Solution solution;
        private List<Examination> unassigned_examinations;
        private List<Examination> unassigned_examinations_with_after;
        private List<Examination> unassigned_examinations_with_coincidence;
        private List<Examination> unassigned_examinations_with_exclusive;
        private List<Examination> assigned_examinations;


        public GraphColoring()
        {
            examinations = Examinations.Instance();
            period_hard_constraints = PeriodHardConstraints.Instance();
            periods = Periods.Instance();
            room_hard_constraints = RoomHardConstraints.Instance();
            rooms = Rooms.Instance();
            conflict_matrix = ConflictMatrix.Instance().Get();
        }

        public Solution Exec()
        {
            solution = new Solution(0, periods.EntryCount(), rooms.EntryCount(), examinations.EntryCount());

            feasibility_tester = new FeasibilityTester();

            AddExclusionToConflictMatrix();

            EraseCoincidenceHCWithConflict();

            PopulateAndSortAssignmentLists();

            int count = 0;
            //TODO Only for testing
            Stopwatch watch = Stopwatch.StartNew();

            int count_average = 0;

            while (assigned_examinations.Count != examinations.EntryCount())
            {
                if(++count % 5000 == 0)
                    Console.WriteLine(count);
                List<Examination> list_to_use;

                if (unassigned_examinations_with_exclusive.Any())
                    list_to_use = unassigned_examinations_with_exclusive;
                else if (unassigned_examinations_with_after.Any())
                    list_to_use = unassigned_examinations_with_after;
                else if (unassigned_examinations_with_coincidence.Any())
                    list_to_use = unassigned_examinations_with_coincidence;
                else
                    list_to_use = unassigned_examinations;

                var exam_to_assign = list_to_use.Last();
                list_to_use.RemoveAt(list_to_use.Count - 1);
                if (IsExaminationAssignable(exam_to_assign))
                {
                    //Console.WriteLine("Normal Assignment");
                    watch.Restart();
                    ExaminationNormalAssignment(exam_to_assign);
                    count_average++;
                    Console.WriteLine("Normal Assignment: " + watch.ElapsedMilliseconds);
                }
                else
                {
                    //Console.WriteLine("Forcing Assignment");
                    watch.Restart();
                    ExaminationForcingAssignment(exam_to_assign);
                    Console.WriteLine("Forcing Assignment: " + watch.ElapsedMilliseconds);
                }
                
                if(unassigned_examinations.Count() 
                    + unassigned_examinations_with_after.Count() 
                    + unassigned_examinations_with_coincidence.Count() 
                    + unassigned_examinations_with_exclusive.Count() 
                    + assigned_examinations.Count() 
                    != examinations.EntryCount())
                    throw new Exception("Examinations lists size mismatch");
            }
            Console.WriteLine("Average: "+(watch.ElapsedMilliseconds/count_average));
            return solution;
        }

        private bool IsExaminationAssignable(Examination exam_to_assign)
        {
            return periods.GetAll().Any(period => feasibility_tester.IsFeasiblePeriod(solution, exam_to_assign, period));
        }

        private void PopulateAndSortAssignmentLists()
        {
            unassigned_examinations = new List<Examination>();
            unassigned_examinations_with_after = new List<Examination>();
            unassigned_examinations_with_coincidence = new List<Examination>();
            unassigned_examinations_with_exclusive = new List<Examination>();
            assigned_examinations = new List<Examination>();

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

        private void AddExclusionToConflictMatrix()
        {
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

        private void ExaminationNormalAssignment(Examination exam_to_assign)
        {
            List<Period> possible_examinations = periods.GetAll().Where(period => feasibility_tester.IsFeasiblePeriod(solution, exam_to_assign, period)).ToList();
            Random random = new Random();
            int random_assignable = random.Next(possible_examinations.Count);
            Period period_to_assign = null;

            foreach (Period period in possible_examinations)
            {
                if (--random_assignable < 0)
                {
                    period_to_assign = period;
                    break;
                }
            }

            if(period_to_assign == null)
                throw new NullReferenceException("Period was not successfully assigned");

            List<Room> possible_rooms = rooms.GetAll().Where(room => feasibility_tester.IsFeasibleRoom(solution, exam_to_assign, period_to_assign, room)).ToList();
            random_assignable = random.Next(possible_rooms.Count);
            Room room_to_assign = null;

            foreach (Room room in possible_rooms)
            {
                if (--random_assignable < 0)
                {
                    room_to_assign = room;
                    break;
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
                if (exam_id != exam_to_assign.id && solution.epr_associasion[exam_id, 0] != -1 && solution.epr_associasion[exam_id, 1] != -1)
                {
                    random_period = solution.epr_associasion[exam_id, 0];
                    period_to_assign = periods.GetById(random_period);
                    break;
                }
            }

            //If not - Period's length must fit examination's duration and all its coincidences
            if(random_period == -1)
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
            for (int exam_id = 0; exam_id < conflict_matrix.GetLength(0); exam_id += 1)
            {
                if (conflict_matrix[exam_id, exam_to_assign.id] && solution.epr_associasion[exam_id, 0] == period_to_assign.id)
                {
                    UnassignExaminationAndCoincidences(examinations.GetById(exam_id));
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
                    && solution.epr_associasion[phc.ex2, 0] >= period_to_assign.id)
                {
                    UnassignExaminationAndCoincidences(examinations.GetById(phc.ex2));
                }
                else if (phc.ex2 == exam_to_assign.id
                    && solution.epr_associasion[phc.ex1, 0] != -1
                    && solution.epr_associasion[phc.ex1, 1] != -1
                    && solution.epr_associasion[phc.ex1, 0] <= period_to_assign.id)
                {
                    UnassignExaminationAndCoincidences(examinations.GetById(phc.ex1));
                }
            }

            //Removes examinations from period and room, until the room has enough capacity for the new examination to take place
            int room_to_assign_curr_capacity =
                feasibility_tester.RoomCurrentCapacityOnPeriod(solution, period_to_assign, room_to_assign);
            int n_of_examinations_in_period_and_room = 0;

            if (room_to_assign_curr_capacity < exam_to_assign.students.Count())
            {
                for (int exam_id = 0; exam_id < examinations.EntryCount(); exam_id++)
                    {
                        if (solution.timetable_container[random_period, random_room, exam_id])
                        {
                            ++n_of_examinations_in_period_and_room;
                        }
                    }
            }

            while (room_to_assign_curr_capacity < exam_to_assign.students.Count())
            {
                int random_examination_unassignment = random.Next(n_of_examinations_in_period_and_room);
                for (int exam_id = 0; exam_id < examinations.EntryCount(); exam_id++)
                {
                    if (solution.timetable_container[random_period, random_room, exam_id] 
                        && --random_examination_unassignment < 0)
                    {
                        List<int> exams_to_unassign =
                            period_hard_constraints.GetAllExaminationsWithChainingCoincidence(exam_id)
                                .Where(
                                    ex_id =>
                                        solution.epr_associasion[ex_id, 0] == random_period &&
                                        solution.epr_associasion[ex_id, 1] == random_room).ToList();
                        n_of_examinations_in_period_and_room -= exams_to_unassign.Count;
                        room_to_assign_curr_capacity +=
                            exams_to_unassign.Sum(ex_id => examinations.GetById(ex_id).students.Count);
                        UnassignExaminations(exams_to_unassign);

                        //n_of_examinations_in_period_and_room -= period_hard_constraints.GetAllExaminationsWithChainingCoincidence(exam_id).Count(ex_id => solution.epr_associasion[ex_id, 0] == random_period && solution.epr_associasion[ex_id, 1] == random_room);
                        //room_to_assign_curr_capacity +=
                        //    period_hard_constraints.GetAllExaminationsWithChainingCoincidence(exam_id).Where(ex_id => solution.epr_associasion[ex_id, 0] == random_period && solution.epr_associasion[ex_id, 1] == random_room).Sum(exam_id => examinations.GetById(exam_id).students.Count()); ;
                        //UnassignExaminationAndCoincidences(examinations.GetById(exam_id));
                        break;
                    }
                }
            }
            ////TODO remover, só para testes
            //if (feasibility_tester.RoomCurrentCapacityOnPeriod(solution, period_to_assign, room_to_assign) != room_to_assign_curr_capacity)
            //    throw new Exception("Period and room are not feasible but should've been");

            ////TODO remover, só para testes
            //if (!feasibility_tester.IsFeasiblePeriod(solution, exam_to_assign, period_to_assign) ||
            //    !feasibility_tester.IsFeasibleRoom(solution, exam_to_assign, period_to_assign, room_to_assign))
            //    throw new Exception("Period and room are not feasible but should've been");


            AssignExamination(period_to_assign, room_to_assign, exam_to_assign);
        }

        private void UnassignExaminations(List<int> exams_to_unassign)
        {
            foreach(int exam_id in exams_to_unassign)
                UnassignExamination(examinations.GetById(exam_id));
        }

        private void UnassignExamination(Examination exam)
        {
            if (solution.epr_associasion[exam.id, 0] == -1 || solution.epr_associasion[exam.id, 1] == -1)
                return;

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
            foreach (int exam_id in period_hard_constraints.GetAllExaminationsWithChainingCoincidence(exam.id))
            {
                UnassignExamination(examinations.GetById(exam_id));
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
    }
}
