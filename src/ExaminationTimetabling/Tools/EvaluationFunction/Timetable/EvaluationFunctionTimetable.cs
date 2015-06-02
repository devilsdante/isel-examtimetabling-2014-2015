using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Business;
using DAL;
using DAL.Models;
using DAL.Models.Solution;
using DAL.Models.Solution.Timetabling;
using Tools.Neighborhood;

namespace Tools.EvaluationFunction.Timetable
{
    public class EvaluationFunctionTimetable : IEvaluationFunction
    {
        private readonly bool[,] conflict_matrix;
        private readonly Examinations examinations;
        private readonly PeriodHardConstraints period_hard_constraints;
        private readonly RoomHardConstraints room_hard_constraints;
        private readonly Rooms rooms;
        private readonly Periods periods;
        private readonly ModelWeightings model_weightings;

        public int student_conflicts_hc;
        public int period_lengths_hc;
        public int exam_coincidence_hc;
        public int exam_exclusion_hc;
        public int exam_after_hc;
        public int room_exclusivity_hc;
        public int room_capacity_hc;



        public EvaluationFunctionTimetable()
        {
            examinations = Examinations.Instance();
            period_hard_constraints = PeriodHardConstraints.Instance();
            periods = Periods.Instance();
            room_hard_constraints = RoomHardConstraints.Instance();
            rooms = Rooms.Instance();
            model_weightings = ModelWeightings.Instance();
            conflict_matrix = ConflictMatrix.Instance().Get();
        }

        public int DistanceToFeasibility(Solution solution)
        {
            student_conflicts_hc = 0;
            period_lengths_hc = 0;
            exam_coincidence_hc = 0;
            exam_exclusion_hc = 0;
            exam_after_hc = 0;
            room_exclusivity_hc = 0;
            room_capacity_hc = 0;

            if (!IsValid(solution))
                return -1;

            // Student Conflicts

            for (int exam1_id = 0; exam1_id < conflict_matrix.GetLength(0); ++exam1_id)
            {
                for (int exam2_id = exam1_id + 1; exam2_id < conflict_matrix.GetLength(1); ++exam2_id)
                //for (int exam2_id = 0; exam2_id < conflict_matrix.GetLength(1); ++exam2_id)
                {
                    if (conflict_matrix[exam1_id, exam2_id] && solution.GetPeriodFrom(exam1_id) == solution.GetPeriodFrom(exam2_id))
                    {
                        ++student_conflicts_hc;
                    }
                }

            }

            // Period Lengths Conflicts

            for(int exam_id = 0; exam_id < examinations.EntryCount(); exam_id++)
            {
                if (examinations.GetById(exam_id).duration >
                    periods.GetById(solution.GetPeriodFrom(exam_id)).duration)
                {
                    ++period_lengths_hc;
                }
                    
            }

            // Examination Coincidences

            foreach (
                PeriodHardConstraint phc in
                    period_hard_constraints.GetByType(PeriodHardConstraint.types.EXAM_COINCIDENCE))
            {
                if (solution.GetPeriodFrom(phc.ex1) != solution.GetPeriodFrom(phc.ex2))
                {
                    ++exam_coincidence_hc;
                }

            }

            // Examination Exclusions

            foreach (
                PeriodHardConstraint phc in
                    period_hard_constraints.GetByType(PeriodHardConstraint.types.EXCLUSION))
            {
                if (solution.GetPeriodFrom(phc.ex1) == solution.GetPeriodFrom(phc.ex2))
                {
                    ++exam_exclusion_hc;
                }
            }

            // Examination Afters

            foreach (
                PeriodHardConstraint phc in
                    period_hard_constraints.GetByType(PeriodHardConstraint.types.AFTER))
            {
                if (periods.GetById(solution.GetPeriodFrom(phc.ex1)).date <= periods.GetById(solution.GetPeriodFrom(phc.ex2)).date)
                {
                    ++exam_after_hc;
                }
            }

            // Examination Room Exclusives
            
            foreach (RoomHardConstraint rhc in room_hard_constraints.GetByType(RoomHardConstraint.types.ROOM_EXCLUSIVE))
            {
                int period_id = solution.GetPeriodFrom(rhc.examination);
                int room_id = solution.GetRoomFrom(rhc.examination);

                for (int exam_id = 0; exam_id < solution.ExaminationCount(); ++exam_id)
                {
                    if (solution.IsExamSetTo(period_id, room_id, exam_id) &&
                        rhc.examination != exam_id)
                    {
                        ++room_exclusivity_hc;
                        break;
                    }
                }
            }

            // Room Capacities

            int[,] rooms_usage = new int[rooms.EntryCount(), periods.EntryCount()];
            for (int exam_id = 0; exam_id < solution.ExaminationCount(); ++exam_id)
            {
                rooms_usage[solution.GetRoomFrom(exam_id), solution.GetPeriodFrom(exam_id)] += examinations.GetById(exam_id).students.Count;
            }

            for (int room_id = 0; room_id < rooms.EntryCount(); room_id++)
            {
                for (int period_id = 0; period_id < periods.EntryCount(); period_id++)
                {
                    if (rooms_usage[room_id, period_id] > rooms.GetById(room_id).capacity)
                    {
                        ++room_capacity_hc;
                    }
                        
                }
                    
            }

            return student_conflicts_hc + exam_after_hc + exam_coincidence_hc + exam_exclusion_hc + period_lengths_hc +
                   room_capacity_hc + room_exclusivity_hc;
        }

        public int Fitness(Solution solution)
        {
            int two_exams_in_a_row = 0;
            int two_exams_in_a_day = 0;
            int period_spread = 0;
            int mixed_durations = 0;
            int front_load = 0;
            int room_penalty = 0;
            int period_penalty = 0;


            // Two examinations in a row
            for (int period_id = 0; period_id < solution.PeriodCount() - 1; ++period_id)
            {
                if (periods.GetById(period_id).date.Day != periods.GetById(period_id + 1).date.Day)
                    continue;
                for (int room_id = 0; room_id < solution.RoomCount(); ++room_id)
                {
                    for (int exam_id = 0; exam_id < solution.ExaminationCount(); ++exam_id)
                    {
                        if (solution.IsExamSetTo(period_id, room_id, exam_id))
                        {
                            int period2_id = period_id + 1;

                            for (int room2_id = 0; room2_id < solution.RoomCount(); ++room2_id)
                            {
                                for (int exam2_id = 0;
                                    exam2_id < solution.ExaminationCount();
                                    ++exam2_id)
                                {
                                    if (solution.IsExamSetTo(period2_id, room2_id, exam2_id))
                                    {
                                        two_exams_in_a_row += examinations.NoOfConflicts(examinations.GetById(exam_id), examinations.GetById(exam2_id)) * model_weightings.Get().two_in_a_row;
                                    }
                                }
                            }
                        }
                    }
                }
            }


            // Two examinations in a day
            for (int period_id = 0; period_id < solution.PeriodCount() - 2; ++period_id)
            {
                if (periods.GetById(period_id).date.Day != periods.GetById(period_id + 2).date.Day)
                    continue;

                for (int room_id = 0; room_id < solution.RoomCount(); ++room_id)
                {
                    for (int exam_id = 0; exam_id < solution.ExaminationCount(); ++exam_id)
                    {
                        if (solution.IsExamSetTo(period_id, room_id, exam_id))
                        {
                            for (int period2_id = period_id + 2;
                                period2_id < solution.PeriodCount() && periods.GetById(period_id).date.Day == periods.GetById(period2_id).date.Day;
                                ++period2_id)
                            {
                                for (int room2_id = 0; room2_id < solution.RoomCount(); ++room2_id)
                                {
                                    for (int exam2_id = 0;
                                        exam2_id < solution.ExaminationCount();
                                        ++exam2_id)
                                    {
                                        if (solution.IsExamSetTo(period2_id, room2_id, exam2_id))
                                        {
                                            two_exams_in_a_day += examinations.NoOfConflicts(examinations.GetById(exam_id), examinations.GetById(exam2_id)) * model_weightings.Get().two_in_a_day;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Period Spread
            for (int period_id = 0; period_id < solution.PeriodCount() - 1; ++period_id)
            {
                for (int room_id = 0; room_id < solution.RoomCount(); ++room_id)
                {
                    for (int exam_id = 0; exam_id < solution.ExaminationCount(); ++exam_id)
                    {
                        if (solution.IsExamSetTo(period_id, room_id, exam_id))
                        {
                            for (int period2_id = period_id + 1;
                                period2_id < solution.PeriodCount() && period2_id <= period_id + model_weightings.Get().period_spread; 
                                ++period2_id)
                            {
                                for (int room2_id = 0; room2_id < solution.RoomCount(); ++room2_id)
                                {
                                    for (int exam2_id = 0;
                                        exam2_id < solution.ExaminationCount();
                                        ++exam2_id)
                                    {
                                        if (solution.IsExamSetTo(period2_id, room2_id, exam2_id))
                                        {
                                            period_spread += examinations.NoOfConflicts(examinations.GetById(exam_id),
                                                examinations.GetById(exam2_id));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Mixed Durations
            for (int period_id = 0; period_id < solution.PeriodCount(); ++period_id)
            {
                for (int room_id = 0; room_id < solution.RoomCount(); ++room_id)
                {
                    List<int> sizes = new List<int>();

                    for (int exam_id = 0; exam_id < solution.ExaminationCount(); ++exam_id)
                    {
                        if (solution.IsExamSetTo(period_id, room_id, exam_id))
                        {
                            int curr_duration = examinations.GetById(exam_id).duration;
                            if (!sizes.Contains(curr_duration))
                                sizes.Add(curr_duration);
                        }
                    }
                    if(sizes.Count != 0)
                        mixed_durations += (sizes.Count() - 1)*model_weightings.Get().non_mixed_durations;
                }
            }

            // Front Load
            List<Examination> exams_front_load =
                examinations.GetAll()
                    .OrderByDescending(ex => ex.students.Count())
                    .ToList()
                    .GetRange(0, model_weightings.Get().front_load[0]);
            List<Period> periods_from_load = periods.GetAll()
                .OrderByDescending(pe => pe.id)
                .ToList()
                .GetRange(0, model_weightings.Get().front_load[1] < periods.EntryCount() ? model_weightings.Get().front_load[1] : periods.EntryCount());

            foreach (Examination exam in exams_front_load)
            {
                if (periods_from_load.Contains(periods.GetById(solution.GetPeriodFrom(exam.id))))
                    front_load += model_weightings.Get().front_load[2];
            }

            //Room Penalty & Period Penalty
            for (int exam_id = 0; exam_id < solution.ExaminationCount(); ++exam_id)
            {
                room_penalty += rooms.GetById(solution.GetRoomFrom(exam_id)).penalty;
                period_penalty += periods.GetById(solution.GetPeriodFrom(exam_id)).penalty;
            }

            //TODO só para testes
            //Console.WriteLine("Two in a row: " + two_exams_in_a_row);
            //Console.WriteLine("Two in a day: " + two_exams_in_a_day);
            //Console.WriteLine("Period spread: " + period_spread);
            //Console.WriteLine("Mixed durations: " + mixed_durations);
            //Console.WriteLine("Front load: " + front_load);
            //Console.WriteLine("Room penalty: " + room_penalty);
            //Console.WriteLine("Period penalty: " + period_penalty);

            return two_exams_in_a_row + two_exams_in_a_day + period_spread + mixed_durations + front_load + room_penalty +
                   period_penalty;
        }

        public bool IsValid(Solution solution)
        {
            if (solution.ExaminationCount() != examinations.EntryCount())
                return false;

            for (int exam_id = 0; exam_id < solution.PeriodCount(); ++exam_id)
            {
                if (solution.GetPeriodFrom(exam_id) == -1 || solution.GetRoomFrom(exam_id) == -1)
                    return false;
            }

            return true;
        }

        

        public int Fitness(INeighbor neighbor)
        {
            ISolution new_solution = neighbor.Accept();
            int fitness = Fitness(new_solution);
            neighbor.Reverse();
            return fitness;
        }

        public int DistanceToFeasibility(INeighbor neighbor)
        {
            ISolution new_solution = neighbor.Accept();
            int dtf = DistanceToFeasibility(new_solution);
            neighbor.Reverse();
            return dtf;
        }

        public int DistanceToFeasibility(ISolution solution)
        {
            return DistanceToFeasibility((Solution) solution);
        }

        public int Fitness(ISolution solution)
        {
            return Fitness((Solution)solution);
        }

        public bool IsValid(ISolution solution)
        {
            return IsValid((Solution)solution);
        }
    }
}
