using System.Collections.Generic;
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
            for (int period_id = 0; period_id < solution.timetable_container.GetLength(0) - 1; ++period_id)
            {
                if (periods.GetById(period_id).date.Day != periods.GetById(period_id + 1).date.Day)
                    continue;
                for (int room_id = 0; room_id < solution.timetable_container.GetLength(1); ++room_id)
                {
                    for (int exam_id = 0; exam_id < solution.timetable_container.GetLength(2); ++exam_id)
                    {
                        if (solution.timetable_container[period_id, room_id, exam_id])
                        {
                            int period2_id = period_id + 1;

                            for (int room2_id = 0; room2_id < solution.timetable_container.GetLength(1); ++room2_id)
                            {
                                for (int exam2_id = 0;
                                    exam2_id < solution.timetable_container.GetLength(2);
                                    ++exam2_id)
                                {
                                    if (solution.timetable_container[period2_id, room2_id, exam2_id])
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
            for (int period_id = 0; period_id < solution.timetable_container.GetLength(0) - 2; ++period_id)
            {
                if (periods.GetById(period_id).date.Day != periods.GetById(period_id + 2).date.Day)
                    continue;

                for (int room_id = 0; room_id < solution.timetable_container.GetLength(1); ++room_id)
                {
                    for (int exam_id = 0; exam_id < solution.timetable_container.GetLength(2); ++exam_id)
                    {
                        if (solution.timetable_container[period_id, room_id, exam_id])
                        {
                            for (int period2_id = period_id + 2;
                                period2_id < solution.timetable_container.GetLength(0) && periods.GetById(period_id).date.Day == periods.GetById(period2_id).date.Day;
                                ++period2_id)
                            {
                                for (int room2_id = 0; room2_id < solution.timetable_container.GetLength(1); ++room2_id)
                                {
                                    for (int exam2_id = 0;
                                        exam2_id < solution.timetable_container.GetLength(2);
                                        ++exam2_id)
                                    {
                                        if (solution.timetable_container[period2_id, room2_id, exam2_id])
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
            for (int period_id = 0; period_id < solution.timetable_container.GetLength(0) - 1; ++period_id)
            {
                for (int room_id = 0; room_id < solution.timetable_container.GetLength(1); ++room_id)
                {
                    for (int exam_id = 0; exam_id < solution.timetable_container.GetLength(2); ++exam_id)
                    {
                        if (solution.timetable_container[period_id, room_id, exam_id])
                        {
                            for (int period2_id = period_id + 1;
                                period2_id < solution.timetable_container.GetLength(0) && period2_id <= period_id + model_weightings.Get().period_spread; 
                                ++period2_id)
                            {
                                for (int room2_id = 0; room2_id < solution.timetable_container.GetLength(1); ++room2_id)
                                {
                                    for (int exam2_id = 0;
                                        exam2_id < solution.timetable_container.GetLength(2);
                                        ++exam2_id)
                                    {
                                        if (solution.timetable_container[period2_id, room2_id, exam2_id])
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
            for (int period_id = 0; period_id < solution.timetable_container.GetLength(0) - 1; ++period_id)
            {
                for (int room_id = 0; room_id < solution.timetable_container.GetLength(1); ++room_id)
                {
                    List<int> sizes = new List<int>();

                    for (int exam_id = 0; exam_id < solution.timetable_container.GetLength(2); ++exam_id)
                    {
                        if (solution.timetable_container[period_id, room_id, exam_id])
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
                .GetRange(0, model_weightings.Get().front_load[1]);

            foreach (Examination exam in exams_front_load)
            {
                if (periods_from_load.Contains(periods.GetById(solution.epr_associasion[exam.id, 0])))
                    front_load += model_weightings.Get().front_load[2];
            }

            //Room Penalty & Period Penalty
            for (int exam_id = 0; exam_id < solution.epr_associasion.GetLength(0); ++exam_id)
            {
                room_penalty += rooms.GetById(solution.epr_associasion[exam_id, 1]).penalty;
                period_penalty += periods.GetById(solution.epr_associasion[exam_id, 0]).penalty;
            }

            return two_exams_in_a_row + two_exams_in_a_day + period_spread + mixed_durations + front_load + room_penalty +
                   period_penalty;
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
