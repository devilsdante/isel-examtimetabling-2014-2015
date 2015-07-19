using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Business;
using DAL;
using DAL.Models;
using DAL.Models.Solution;
using DAL.Models.Solution.Timetabling;
using Tools.Neighborhood;
using Tools.Neighborhood.Timetable;

namespace Tools.EvaluationFunction.Timetable
{
    public class EvaluationFunctionTimetable : IEvaluationFunction
    {
        private readonly int[,] conflict_matrix;
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
                {
                    if (conflict_matrix[exam1_id, exam2_id] > 0 && solution.GetPeriodFrom(exam1_id) == solution.GetPeriodFrom(exam2_id))
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
                rooms_usage[solution.GetRoomFrom(exam_id), solution.GetPeriodFrom(exam_id)] += examinations.GetById(exam_id).students_count;
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

            //Stopwatch watch = new Stopwatch();
            //watch.Start();

            List<int>[] period_examinations = new List<int>[periods.EntryCount()];

            for (int period_id = 0; period_id < periods.EntryCount(); period_id++)
            {
                period_examinations[period_id] = solution.GetExaminationsFrom(period_id);
            }

            // Two examinations in a row/day

            for (int period1_id = 0; period1_id < periods.EntryCount() - 1; ++period1_id)
            {
                int period2_id = period1_id + 1;

                if (periods.GetById(period1_id).date.Day != periods.GetById(period2_id).date.Day)
                    continue;

                for (int exam1_index = 0; exam1_index < period_examinations[period1_id].Count; ++exam1_index)
                {
                    // Two examinations in a row
                    int exam1_id = period_examinations[period1_id][exam1_index];

                    for (int exam2_index = 0; exam2_index < period_examinations[period2_id].Count; ++exam2_index)
                    {
                        int exam2_id = period_examinations[period2_id][exam2_index];
                        int no_conflicts = conflict_matrix[exam1_id, exam2_id];

                        if (no_conflicts == 1)
                        {
                            if (period_hard_constraints.GetByType(PeriodHardConstraint.types.EXCLUSION).Any(phc => phc.ex1 == exam1_id && phc.ex2 == exam2_id ||
                                                                                                                   phc.ex1 == exam2_id && phc.ex2 == exam1_id))
                            {
                                --no_conflicts;
                            }
                        }

                        two_exams_in_a_row += no_conflicts * model_weightings.Get().two_in_a_row;

                        // Period Spread (in a row)
                        if (period2_id - period1_id <= model_weightings.Get().period_spread)
                            period_spread += no_conflicts;
                    }
                }

                for (period2_id = period2_id + 1; period2_id < periods.EntryCount(); ++period2_id)
                {
                    if (periods.GetById(period1_id).date.Day != periods.GetById(period2_id).date.Day)
                        break;

                    for (int exam1_index = 0; exam1_index < period_examinations[period1_id].Count; ++exam1_index)
                    {
                        // Two examinations in a day
                        int exam1_id = period_examinations[period1_id][exam1_index];

                        for (int exam2_index = 0; exam2_index < period_examinations[period2_id].Count; ++exam2_index)
                        {
                            int exam2_id = period_examinations[period2_id][exam2_index];
                            int no_conflicts = conflict_matrix[exam1_id, exam2_id];

                            if (no_conflicts == 1)
                            {
                                if (period_hard_constraints.GetByType(PeriodHardConstraint.types.EXCLUSION).Any(phc => phc.ex1 == exam1_id && phc.ex2 == exam2_id ||
                                                                                                                       phc.ex1 == exam2_id && phc.ex2 == exam1_id))
                                {
                                    --no_conflicts;
                                }
                            }

                            two_exams_in_a_day += no_conflicts * model_weightings.Get().two_in_a_day;

                            // Period Spread (in the rest of the day)
                            if (period2_id - period1_id <= model_weightings.Get().period_spread)
                                period_spread += no_conflicts;
                        }
                    }
                }
            }
            //Console.WriteLine("Time1&2: " + watch.ElapsedMilliseconds);
            //watch.Restart();

            // Period Spread (in other days)

            for (int period1_id = 0; period1_id < periods.EntryCount() - 1; ++period1_id)
            {
                for (int period2_id = period1_id + 1; period2_id < periods.EntryCount(); ++period2_id)
                {
                    if (periods.GetById(period1_id).date.Day == periods.GetById(period2_id).date.Day)
                        continue;

                    if (period2_id - period1_id > model_weightings.Get().period_spread)
                        break;

                    for (int exam1_index = 0; exam1_index < period_examinations[period1_id].Count; ++exam1_index)
                    {
                        int exam1_id = period_examinations[period1_id][exam1_index];

                        for (int exam2_index = 0; exam2_index < period_examinations[period2_id].Count; ++exam2_index)
                        {
                            int exam2_id = period_examinations[period2_id][exam2_index];
                            int no_conflicts = conflict_matrix[exam1_id, exam2_id];

                            if (no_conflicts == 1)
                            {
                                if (period_hard_constraints.GetByType(PeriodHardConstraint.types.EXCLUSION).Any(phc => phc.ex1 == exam1_id && phc.ex2 == exam2_id ||
                                                                                                                       phc.ex1 == exam2_id && phc.ex2 == exam1_id))
                                {
                                    --no_conflicts;
                                }
                            }

                            period_spread += no_conflicts;
                        }
                    }
                }
            }
            //Console.WriteLine("Time3: " + watch.ElapsedMilliseconds);
            //watch.Restart();

            // Mixed Durations
            for (int period_id = 0; period_id < solution.PeriodCount(); ++period_id)
            {
                for (int room_id = 0; room_id < solution.RoomCount(); ++room_id)
                {
                    List<int> sizes = new List<int>();
                    
                    foreach (int exam_id in solution.GetExaminationsFrom(period_id, room_id))
                    {
                        int curr_duration = examinations.GetById(exam_id).duration;
                        if (!sizes.Contains(curr_duration))
                            sizes.Add(curr_duration);
                    }
                    if(sizes.Count != 0)
                        mixed_durations += (sizes.Count() - 1)*model_weightings.Get().non_mixed_durations;
                }
            }
            //Console.WriteLine("Time4: " + watch.ElapsedMilliseconds);
            //watch.Restart();

            // Front Load
            List<Examination> exams_front_load =
                examinations.GetAll()
                    .OrderByDescending(ex => ex.students_count)
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

            //Console.WriteLine("Time5: " + watch.ElapsedMilliseconds);
            //watch.Restart();

            //Room Penalty & Period Penalty
            for (int exam_id = 0; exam_id < solution.ExaminationCount(); ++exam_id)
            {
                room_penalty += rooms.GetById(solution.GetRoomFrom(exam_id)).penalty;
                period_penalty += periods.GetById(solution.GetPeriodFrom(exam_id)).penalty;
            }

            //Console.WriteLine("Time6: " + watch.ElapsedMilliseconds);
            //watch.Restart();

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



        //public int Fitness(INeighbor neighbor)
        //{
        //    ISolution new_solution = neighbor.Accept();
        //    int fitness = Fitness(new_solution);
        //    neighbor.Reverse();
        //    return fitness;
        //}

        private int Fitness(RoomChangeNeighbor n)
        {
            int fitness = n.solution.fitness + (rooms.GetById(n.new_room_id).penalty - rooms.GetById(n.old_room_id).penalty);

            List<int> sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.period_id, n.old_room_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
                fitness -= (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;

            sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.period_id, n.new_room_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
                fitness -= (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;

            //--
            n.Accept();
            //--

            sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.period_id, n.old_room_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
                fitness += (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;

            sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.period_id, n.new_room_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
                fitness += (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;

            n.Reverse();

            //ISolution new_solution = n.Accept();
            //int dtf = Fitness(new_solution);
            //n.Reverse();
            //if (fitness != dtf)
            //    throw new Exception();

            return fitness;
        }

        private int Fitness(RoomSwapNeighbor n)
        {
            int fitness = n.solution.fitness;

            List<int> sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.period1_id, n.room1_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
                fitness -= (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;

            sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.period2_id, n.room2_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
                fitness -= (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;

            //--
            n.Accept();
            //--

            sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.period1_id, n.room1_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
                fitness += (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;

            sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.period2_id, n.room2_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
                fitness += (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;

            n.Reverse();

            //ISolution new_solution = n.Accept();
            //int dtf = Fitness(new_solution);
            //n.Reverse();
            //if (fitness != dtf)
            //    throw new Exception();

            return fitness;
        }

        private int Fitness(PeriodChangeNeighbor n)
        {
            int fitness = n.solution.fitness;

            //Period penalty
            fitness += (periods.GetById(n.new_period_id).penalty - periods.GetById(n.old_period_id).penalty);

            //Remove old Mixed durations
            List<int> sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.old_period_id, n.room_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
            {
                fitness -= (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;
            }
                

            sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.new_period_id, n.room_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
            {
                fitness -= (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;
            }
                

            //Add new Mixed durations
            n.Accept();

            sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.old_period_id, n.room_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
            {
                fitness += (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;
            }
                

            sizes = new List<int>();

            foreach (int exam_id in n.solution.GetExaminationsFrom(n.new_period_id, n.room_id))
            {
                int curr_duration = examinations.GetById(exam_id).duration;
                if (!sizes.Contains(curr_duration))
                    sizes.Add(curr_duration);
            }
            if (sizes.Count != 0)
            {
                fitness += (sizes.Count() - 1) * model_weightings.Get().non_mixed_durations;
            }
                

            n.Reverse();


            //Remove old Period spread
            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                if (period_id >= n.old_period_id - model_weightings.Get().period_spread &&
                    period_id <= n.old_period_id + model_weightings.Get().period_spread &&
                    period_id != n.old_period_id)
                {
                    List<int> exams = n.solution.GetExaminationsFrom(period_id);

                    foreach (int exam in exams)
                    {
                        int no_conflicts = conflict_matrix[exam, n.examination_id];

                        if (no_conflicts == 1)
                        {
                            if (
                                period_hard_constraints.GetByType(PeriodHardConstraint.types.EXCLUSION)
                                    .Any(phc => phc.ex1 == exam && phc.ex2 == n.examination_id ||
                                                phc.ex1 == n.examination_id && phc.ex2 == exam))
                            {
                                --no_conflicts;
                            }
                        }
                        fitness -= no_conflicts;
                    }
                }
            }

            //Add new Period spread
            n.Accept();
            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                
                if (period_id >= n.new_period_id - model_weightings.Get().period_spread &&
                    period_id <= n.new_period_id + model_weightings.Get().period_spread &&
                    period_id != n.new_period_id)
                {
                    List<int> exams = n.solution.GetExaminationsFrom(period_id);

                    foreach (int exam in exams)
                    {
                        int no_conflicts = conflict_matrix[exam, n.examination_id];

                        if (no_conflicts == 1)
                        {
                            if (
                                period_hard_constraints.GetByType(PeriodHardConstraint.types.EXCLUSION)
                                    .Any(phc => phc.ex1 == exam && phc.ex2 == n.examination_id ||
                                                phc.ex1 == n.examination_id && phc.ex2 == exam))
                            {
                                --no_conflicts;
                            }
                        }
                        fitness += no_conflicts;
                    }
                }
            }
            n.Reverse();

            //Remove all Front load
            List<Examination> exams_front_load =
                examinations.GetAll()
                    .OrderByDescending(ex => ex.students_count)
                    .ToList()
                    .GetRange(0, model_weightings.Get().front_load[0]);
            List<Period> periods_from_load = periods.GetAll()
                .OrderByDescending(pe => pe.id)
                .ToList()
                .GetRange(0, model_weightings.Get().front_load[1] < periods.EntryCount() ? model_weightings.Get().front_load[1] : periods.EntryCount());

            foreach (Examination exam in exams_front_load)
            {
                if (periods_from_load.Contains(periods.GetById(n.solution.GetPeriodFrom(exam.id))))
                    fitness -= model_weightings.Get().front_load[2];
            }

            //Recalculate all Fron load
            n.Accept();
            exams_front_load =
                examinations.GetAll()
                    .OrderByDescending(ex => ex.students_count)
                    .ToList()
                    .GetRange(0, model_weightings.Get().front_load[0]);
            periods_from_load = periods.GetAll()
                .OrderByDescending(pe => pe.id)
                .ToList()
                .GetRange(0, model_weightings.Get().front_load[1] < periods.EntryCount() ? model_weightings.Get().front_load[1] : periods.EntryCount());

            foreach (Examination exam in exams_front_load)
            {
                if (periods_from_load.Contains(periods.GetById(n.solution.GetPeriodFrom(exam.id))))
                    fitness += model_weightings.Get().front_load[2];
            }
            n.Reverse();

            fitness -= TotalConflictInADay(n.examination_id, n.old_period_id, n.solution);
            n.Accept();
            fitness += TotalConflictInADay(n.examination_id, n.new_period_id, n.solution);
            n.Reverse();

            //ISolution new_solution = n.Accept();
            //int dtf = Fitness(new_solution);
            //n.Reverse();
            //if (fitness != dtf)
            //{
            //    //OutputFormatting.Write("..//..//ahetal.txt", n.examination_id + " " + n.old_period_id + " " + n.new_period_id);
            //    throw new Exception();
            //    return dtf;
            //}

            return fitness;
        }

        public int Fitness(INeighbor neighbor)
        {
            if (neighbor.type == 4)
            {
                RoomChangeNeighbor n = (RoomChangeNeighbor)neighbor;

                return Fitness(n);
            }
            if (neighbor.type == 5)
            {
                RoomSwapNeighbor n = (RoomSwapNeighbor)neighbor;

                return Fitness(n);
            }
            if (neighbor.type == 0)
            {
                PeriodChangeNeighbor n = (PeriodChangeNeighbor)neighbor;

                return Fitness(n);
            }
            else
            {
                ISolution new_solution = neighbor.Accept();
                int fitness = Fitness(new_solution);
                neighbor.Reverse();
                return fitness;
            }
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

        private int TotalConflictInADay(int examination, int period, Solution solution)
        {
            int conflict = 0;

            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                if (periods.GetById(period_id).date.Month != periods.GetById(period).date.Month)
                    continue;
                if (periods.GetById(period_id).date.Day < periods.GetById(period).date.Day)
                    continue;
                if (periods.GetById(period_id).date.Day > periods.GetById(period).date.Day)
                    break;
                if (period_id == period)
                    continue;

                List<int> exams = solution.GetExaminationsFrom(period_id);

                foreach (int exam in exams)
                {
                    int no_conflicts = conflict_matrix[exam, examination];

                    if (no_conflicts == 1)
                    {
                        if (period_hard_constraints.GetByType(PeriodHardConstraint.types.EXCLUSION).Any(phc => phc.ex1 == exam && phc.ex2 == examination ||
                                                                                                               phc.ex1 == examination && phc.ex2 == exam))
                        {
                            --no_conflicts;
                        }
                    }
                    if (period_id == period - 1 || period_id == period + 1)
                    {
                        conflict += no_conflicts * model_weightings.Get().two_in_a_row;
                    }
                    else
                    {
                        conflict += no_conflicts * model_weightings.Get().two_in_a_day;
                    }
                        
                }
            }

            return conflict;
        }
    }
}
