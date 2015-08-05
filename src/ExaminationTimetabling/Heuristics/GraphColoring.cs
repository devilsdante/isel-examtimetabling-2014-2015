using Business;
using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

        private readonly int[,] conflict_matrix;
        private FeasibilityTester feasibility_tester;
        private Solution solution;
        private List<Examination> unassigned_examinations;
        private List<Examination> unassigned_examinations_with_after;
        private List<Examination> unassigned_examinations_with_coincidence;
        private List<Examination> unassigned_examinations_with_exclusive;

        private readonly Random random;
        private List<int> my_list;


        public GraphColoring()
        {
            examinations = Examinations.Instance();
            period_hard_constraints = PeriodHardConstraints.Instance();
            periods = Periods.Instance();
            room_hard_constraints = RoomHardConstraints.Instance();
            rooms = Rooms.Instance();
            conflict_matrix = ConflictMatrix.Instance().Get();
            random = new Random(Guid.NewGuid().GetHashCode());
            my_list = new List<int>(new int[examinations.EntryCount()]);
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
            //Stopwatch watch = Stopwatch.StartNew();
            //Stopwatch watch2 = Stopwatch.StartNew();

            int [] normal_assignments = new int[examinations.EntryCount()];

            while (solution.AssignedExaminations() != examinations.EntryCount())
            {
                if (++count%500 == 0)
                {
                    Console.WriteLine(count);
                    //Console.WriteLine(unassigned_examinations.Count);
                }
                    
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
                my_list[exam_to_assign.id]++;
                list_to_use.RemoveAt(list_to_use.Count - 1);

                /**/
                //Console.WriteLine("Normal Assignment");
                //watch.Restart();

                //if (watch2.ElapsedMilliseconds%1000 == 0)
                //{
                //    if (unassigned_examinations_with_exclusive.Count > 0)
                //        Console.WriteLine("Exclusive: " + unassigned_examinations_with_exclusive.Count);
                //    if (unassigned_examinations_with_after.Count > 0)
                //        Console.WriteLine("After: " + unassigned_examinations_with_after.Count);
                //    if (unassigned_examinations_with_coincidence.Count > 0)
                //        //Console.WriteLine("Coincidence: " + unassigned_examinations_with_coincidence.Count);
                //        Console.WriteLine("Coincidence: " + unassigned_examinations_with_coincidence[0].id);
                //    if (unassigned_examinations.Count > 0)
                        //Console.WriteLine("Others: " + unassigned_examinations.Count);
                //    Console.WriteLine();
                //}

                //if(exam_to_assign.conflict > 0)
                //    exam_to_assign.conflict -= 10;
                //else
                //    exam_to_assign.conflict -= 1;

                if (!ExaminationNormalAssignment(exam_to_assign))
                {
                    ExaminationForcingAssignment(exam_to_assign);
                }
                else
                {
                    normal_assignments[exam_to_assign.id] += 1;
                }
                    
                /**/
                if(unassigned_examinations.Count() 
                    + unassigned_examinations_with_after.Count() 
                    + unassigned_examinations_with_coincidence.Count() 
                    + unassigned_examinations_with_exclusive.Count() 
                    + solution.AssignedExaminations()
                    != examinations.EntryCount())
                    throw new Exception("Examinations lists size mismatch");
            }
            return solution;
        }

        private void PopulateAndSortAssignmentLists()
        {
            unassigned_examinations = new List<Examination>();
            unassigned_examinations_with_after = new List<Examination>();
            unassigned_examinations_with_coincidence = new List<Examination>();
            unassigned_examinations_with_exclusive = new List<Examination>();

            foreach (Examination exam in examinations.GetAll())
            {
                if (room_hard_constraints.HasRoomExclusivity(exam.id))
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
                if (conflict_matrix[phc.ex1, phc.ex2] == 0)
                {
                    conflict_matrix[phc.ex1, phc.ex2] += 1;
                    conflict_matrix[phc.ex2, phc.ex1] += 1;

                    examinations.GetById(phc.ex1).conflict += 1;
                    examinations.GetById(phc.ex2).conflict += 1;
                }
            }
        }

        private void EraseCoincidenceHCWithConflict()
        {
            foreach (PeriodHardConstraint coincidence in period_hard_constraints.GetByType(PeriodHardConstraint.types.EXAM_COINCIDENCE))
            {
                if (conflict_matrix[coincidence.ex1, coincidence.ex2] > 0)
                {
                    period_hard_constraints.Delete(coincidence);
                }
            }
        }

        private bool ExaminationNormalAssignment(Examination exam_to_assign)
        {
            List<Period> possible_periods = periods.GetAll().Where(period => feasibility_tester.IsFeasiblePeriod(solution, exam_to_assign, period)).ToList();

            if (!possible_periods.Any())
                return false;

            int random_assignable = random.Next(possible_periods.Count);
            Period period_to_assign = possible_periods[random_assignable];

            if(period_to_assign == null)
                throw new NullReferenceException("Period was not successfully assigned");

            List<Room> possible_rooms = rooms.GetAll().Where(room => feasibility_tester.IsFeasibleRoom(solution, exam_to_assign, period_to_assign, room)).ToList();

            random_assignable = random.Next(possible_rooms.Count);
            Room room_to_assign = possible_rooms[random_assignable];

            if (room_to_assign == null)
                throw new NullReferenceException("Room was not successfully assigned");

            AssignExamination(period_to_assign, room_to_assign, exam_to_assign);

            return true;
        }

        private void ExaminationForcingAssignment(Examination exam_to_assign)
        {
            //Console.WriteLine(exam_to_assign.id);
            int random_room = -1;
            int random_period = -1;
            Room room_to_assign = null;
            Period period_to_assign = null;

            //Default room's capacity must be higher than the examination's number of students
            while (true)
            {
                random_room = random.Next(rooms.EntryCount());
                if (rooms.GetById(random_room).capacity >=
                    examinations.GetById(exam_to_assign.id).students_count)
                {
                    room_to_assign = rooms.GetById(random_room);
                    break;
                }
            }

            if (room_to_assign == null)
                throw new NullReferenceException("Room was not successfully assigned");

            List<int> exam_ids = period_hard_constraints.GetExamsWithChainingCoincidence(exam_to_assign.id).ToList();

            //If there's a coincident examination assigned, the exam_to_assign will be forced to be put on the same period as its coincident
            foreach (int exam_id in exam_ids)
            {
                if (exam_id != exam_to_assign.id && solution.GetPeriodFrom(exam_id) != -1 && solution.GetRoomFrom(exam_id) != -1)
                {
                    //To avoid loops in SET 6
                    if (random.Next(4) != 0)
                    {
                        random_period = solution.GetPeriodFrom(exam_id);
                        period_to_assign = periods.GetById(random_period);
                        break;
                    }
                    else
                    {
                        //Console.WriteLine("HAPPENED!!!!!!!!!!!!!!!!!!");
                        UnassignExaminations(exam_ids);
                        break;
                    }
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
            foreach (int exam_id in solution.GetExaminationsFrom(period_to_assign.id))
            {
                if (conflict_matrix[exam_id, exam_to_assign.id] > 0)
                {
                    UnassignExaminationAndCoincidences(examinations.GetById(exam_id));
                }
            }

            //If exam_to_assign needs room exclusivity, unassign all examinations taking place in the period at the room
            if (room_hard_constraints.HasRoomExclusivity(exam_to_assign.id))
            {
                foreach (int exam_id in solution.GetExaminationsFrom(period_to_assign.id, room_to_assign.id))
                {
                    if (solution.IsExamSetTo(period_to_assign.id, room_to_assign.id, exam_id))
                     {
                         UnassignExaminationAndCoincidences(examinations.GetById(exam_id));
                     }
                }
            }

            //All examinations taking place in the period at the room that needs room exclusivity, must be unassigned
            else
            {
                foreach (int exam_id in solution.GetExaminationsFrom(period_to_assign.id, room_to_assign.id))
                {
                    if (room_hard_constraints.HasRoomExclusivity(exam_id))
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
                    && solution.GetPeriodFrom(phc.ex2) != -1
                    && solution.GetRoomFrom(phc.ex2) != -1
                    && solution.GetPeriodFrom(phc.ex2) >= period_to_assign.id)
                {
                    UnassignExaminationAndCoincidences(examinations.GetById(phc.ex2));
                }
                else if (phc.ex2 == exam_to_assign.id
                    && solution.GetPeriodFrom(phc.ex1) != -1
                    && solution.GetRoomFrom(phc.ex1) != -1
                    && solution.GetPeriodFrom(phc.ex1) <= period_to_assign.id)
                {
                    UnassignExaminationAndCoincidences(examinations.GetById(phc.ex1));
                }
            }

            //Removes examinations from period and room, until the room has enough capacity for the new examination to take place
            int room_to_assign_curr_capacity =
                feasibility_tester.RoomCurrentCapacityOnPeriod(solution, period_to_assign, room_to_assign);
            int n_of_examinations_in_period_and_room = solution.GetExaminationsFrom(period_to_assign.id, room_to_assign.id).Count;

            while (room_to_assign_curr_capacity < exam_to_assign.students_count)
            {
                int random_examination_unassignment = random.Next(n_of_examinations_in_period_and_room);

                int random_examination =
                    solution.GetExaminationsFrom(period_to_assign.id, room_to_assign.id)[random_examination_unassignment];

                List<int> exams_to_unassign =
                            period_hard_constraints.GetExamsWithChainingCoincidence(random_examination)
                                .Where(
                                    ex_id =>
                                        solution.GetPeriodFrom(ex_id) == period_to_assign.id &&
                                        solution.GetRoomFrom(ex_id) == room_to_assign.id).ToList();
                n_of_examinations_in_period_and_room -= exams_to_unassign.Count;
                room_to_assign_curr_capacity +=
                    exams_to_unassign.Sum(ex_id => examinations.GetById(ex_id).students_count);
                UnassignExaminations(exams_to_unassign);
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

        private void UnassignExaminationsAndCoincidences(List<int> exams_to_unassign)
        {
            foreach(int exam_id in exams_to_unassign)
                UnassignExaminationAndCoincidences(examinations.GetById(exam_id));
        }

        private void UnassignExamination(Examination exam)
        {
            if (solution.GetPeriodFrom(exam.id) == -1 || solution.GetRoomFrom(exam.id) == -1)
                return;

            solution.UnsetExam(solution.GetPeriodFrom(exam.id), solution.GetRoomFrom(exam.id), exam.id);

            if (room_hard_constraints.HasRoomExclusivity(exam.id))
            {
                unassigned_examinations_with_exclusive.Add(exam);
                unassigned_examinations_with_exclusive = unassigned_examinations_with_exclusive.OrderBy(examination => examination.conflict).ToList();
            }

            else if (period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam.id).Any())
            {
                unassigned_examinations_with_after.Add(exam);
                unassigned_examinations_with_after = unassigned_examinations_with_after.OrderBy(examination => examination.conflict).ToList();
            }

            else if (
                period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE, exam.id)
                    .Any())
            {
                unassigned_examinations_with_coincidence.Add(exam);
                unassigned_examinations_with_coincidence = unassigned_examinations_with_coincidence.OrderBy(examination => examination.conflict).ToList();
            }

            else
            {
                unassigned_examinations.Add(exam);
                unassigned_examinations = unassigned_examinations.OrderBy(examination => examination.conflict).ToList();
            }

            //if (room_hard_constraints.HasRoomExclusivity(exam.id))
            //{
            //    unassigned_examinations_with_exclusive.Insert(
            //        random.Next(unassigned_examinations_with_exclusive.Count), exam);
            //}

            //else if (period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.AFTER, exam.id).Any())
            //{
            //    unassigned_examinations_with_after.Insert(
            //        random.Next(unassigned_examinations_with_after.Count), exam);
            //}

            //else if (
            //    period_hard_constraints.GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE, exam.id)
            //        .Any())
            //{
            //    unassigned_examinations_with_coincidence.Insert(
            //    random.Next(unassigned_examinations_with_coincidence.Count), exam);
            //}

            //else
            //{
            //    unassigned_examinations.Insert(
            //        random.Next(unassigned_examinations.Count), exam);
            //}
        }

        private void UnassignExaminationAndCoincidences(Examination exam)
        {
            foreach (int exam_id in period_hard_constraints.GetExamsWithChainingCoincidence(exam.id))
            {
                UnassignExamination(examinations.GetById(exam_id));
            }
        }

        private void AssignExamination(Period period, Room room, Examination exam)
        {
            solution.SetExam(period.id, room.id, exam.id);
            if (unassigned_examinations.Remove(exam)) {}
            else if (unassigned_examinations_with_after.Remove(exam)) {}
            else if (unassigned_examinations_with_coincidence.Remove(exam)) {}
            else unassigned_examinations_with_exclusive.Remove(exam);
        }

        private void UnassignExaminations(List<int> exams_to_unassign)
        {
            foreach (int exam in exams_to_unassign)
            {
                UnassignExamination(examinations.GetById(exam));
            }
        }
    }
}
