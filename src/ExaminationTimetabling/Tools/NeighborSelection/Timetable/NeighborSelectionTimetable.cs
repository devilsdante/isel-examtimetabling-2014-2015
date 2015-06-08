using System;
using System.Diagnostics;
using Business;
using DAL;
using DAL.Models;
using DAL.Models.Solution.Timetabling;
using Tools.EvaluationFunction.Timetable;
using Tools.Neighborhood;
using Tools.Neighborhood.Timetable;

namespace Tools.NeighborSelection.Timetable
{
    public class NeighborSelectionTimetable
    {
        private readonly Examinations examinations;
        private readonly Rooms rooms;
        private readonly Periods periods;
        private readonly FeasibilityTester feasibility_tester;
        private readonly EvaluationFunctionTimetable _evaluationFunctionTimetable;

        public NeighborSelectionTimetable()
        {
            examinations = Examinations.Instance();
            rooms = Rooms.Instance();
            periods = Periods.Instance();
            feasibility_tester = new FeasibilityTester();
            _evaluationFunctionTimetable = new EvaluationFunctionTimetable();

        }

        public INeighbor RoomSwap(Solution solution)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Random random = new Random((int) DateTime.Now.Ticks);
            Examination random_examination = examinations.GetById(random.Next(examinations.EntryCount()));
            Period period = periods.GetById(solution.GetPeriodFrom(random_examination.id));
            int random_room_id = random.Next(rooms.EntryCount());

            for (int room_id = 0; room_id < rooms.EntryCount(); ++room_id)
            {
                Room random_room = rooms.GetById((room_id + random_room_id) % rooms.EntryCount());
                if (solution.GetRoomFrom(random_examination.id) == random_room.id)
                    continue;
                if (feasibility_tester.IsFeasibleRoom(solution, random_examination, period, random_room))
                    return new RoomChangeNeighbor(solution, random_examination.id, random_room.id);

                for (int exam_to_swap_id = 0; exam_to_swap_id < examinations.EntryCount(); ++exam_to_swap_id)
                {
                    if (!solution.IsExamSetTo(period.id, random_room.id, exam_to_swap_id))
                        continue;

                    INeighbor neighbor = new RoomSwapNeighbor(solution, random_examination.id, exam_to_swap_id);
                    neighbor.Accept();

                    Period random_examination_period = period;
                    Room random_examination_room = rooms.GetById(solution.GetRoomFrom(random_examination.id));

                    Period exam_to_swap_period = period;
                    Room exam_to_swap_room = rooms.GetById(solution.GetRoomFrom(exam_to_swap_id));

                    solution.UnsetExam(random_examination.id);
                    bool is_feasible = feasibility_tester.IsFeasibleRoom(solution, random_examination,
                        random_examination_period, random_examination_room);
                    solution.SetExam(random_examination_period.id, random_examination_room.id, random_examination.id);

                    if (!is_feasible)
                    {
                        neighbor.Reverse();
                        continue;
                    }

                    solution.UnsetExam(exam_to_swap_id);
                    is_feasible = feasibility_tester.IsFeasibleRoom(solution, examinations.GetById(exam_to_swap_id),
                            exam_to_swap_period, exam_to_swap_room);
                    solution.SetExam(exam_to_swap_period.id, exam_to_swap_room.id, exam_to_swap_id);

                    if (!is_feasible)
                    {
                        neighbor.Reverse();
                        continue;
                    }
                    neighbor.Reverse();

                    return neighbor;
                }
            }
            return null;
        }

        public INeighbor PeriodSwap(Solution solution)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Random random = new Random((int) DateTime.Now.Ticks);
            Examination random_examination = examinations.GetById(random.Next(examinations.EntryCount()));
            int random_period_id = random.Next(periods.EntryCount());
            Room room = rooms.GetById(solution.GetRoomFrom(random_examination.id));

            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                Period random_period = periods.GetById((period_id + random_period_id) % periods.EntryCount());
                if (solution.GetPeriodFrom(random_examination.id) == random_period.id)
                    continue;
                if (feasibility_tester.IsFeasiblePeriodRoom(solution, random_examination, random_period, room))
                    return new PeriodChangeNeighbor(solution, random_examination.id, random_period.id);
                    

                for (int exam_to_swap_id = 0; exam_to_swap_id < examinations.EntryCount(); ++exam_to_swap_id)
                {
                    if (!solution.IsExamSetTo(random_period.id, room.id, exam_to_swap_id))
                        continue;

                    INeighbor neighbor = new PeriodSwapNeighbor(solution, random_examination.id, exam_to_swap_id);
                    neighbor.Accept();

                    Period random_examination_period = periods.GetById(solution.GetPeriodFrom(random_examination.id));
                    Room random_examination_room = room;

                    Period exam_to_swap_period = periods.GetById(solution.GetPeriodFrom(exam_to_swap_id));
                    Room exam_to_swap_room = room;

                    solution.UnsetExam(random_examination.id);
                    bool is_feasible = feasibility_tester.IsFeasiblePeriodRoom(solution, random_examination,
                        random_examination_period, random_examination_room);
                    solution.SetExam(random_examination_period.id, random_examination_room.id, random_examination.id);

                    if (!is_feasible)
                    {
                        neighbor.Reverse();
                        continue;
                    }

                    solution.UnsetExam(exam_to_swap_id);
                    is_feasible = feasibility_tester.IsFeasiblePeriodRoom(solution, examinations.GetById(exam_to_swap_id),
                            exam_to_swap_period, exam_to_swap_room);
                    solution.SetExam(exam_to_swap_period.id, exam_to_swap_room.id, exam_to_swap_id);

                    if (!is_feasible)
                    {
                        neighbor.Reverse();
                        continue;
                    }
                    neighbor.Reverse();
                    
                    return neighbor;
                }
            }
            return null;
        }

        public INeighbor PeriodRoomSwap(Solution solution)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Random random = new Random((int)DateTime.Now.Ticks);
            Examination random_examination = examinations.GetById(random.Next(examinations.EntryCount()));
            int random_period_id = random.Next(periods.EntryCount());
            int random_room_id = random.Next(rooms.EntryCount());

            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                Period random_period = periods.GetById((period_id + random_period_id) % periods.EntryCount());
                if (!feasibility_tester.IsFeasiblePeriod(solution, random_examination, random_period))
                    continue;
                for (int room_id = 0; room_id < rooms.EntryCount(); ++room_id)
                {
                    Room random_room = rooms.GetById((room_id + random_room_id) % rooms.EntryCount());
                    if (solution.GetPeriodFrom(random_examination.id) == random_period.id ||
                        solution.GetRoomFrom(random_examination.id) == random_room.id)
                        continue;

                    if (feasibility_tester.IsFeasibleRoom(solution, random_examination, random_period, random_room))
                        return new PeriodRoomChangeNeighbor(solution, random_examination.id, random_period.id, random_room.id);
                        

                    for (int exam_to_swap_id = 0; exam_to_swap_id < examinations.EntryCount(); ++exam_to_swap_id)
                    {
                        if (!solution.IsExamSetTo(random_period.id, random_room.id, exam_to_swap_id))
                            continue;
                        INeighbor neighbor = new PeriodRoomSwapNeighbor(solution, random_examination.id, exam_to_swap_id);
                        neighbor.Accept();

                        Period random_examination_period = periods.GetById(solution.GetPeriodFrom(random_examination.id));
                        Room random_examination_room = rooms.GetById(solution.GetRoomFrom(random_examination.id));

                        Period exam_to_swap_period = periods.GetById(solution.GetPeriodFrom(exam_to_swap_id)); ;
                        Room exam_to_swap_room = rooms.GetById(solution.GetRoomFrom(exam_to_swap_id));

                        solution.UnsetExam(random_examination.id);
                        bool is_feasible = feasibility_tester.IsFeasiblePeriodRoom(solution, random_examination,
                            random_examination_period, random_examination_room);
                        solution.SetExam(random_examination_period.id, random_examination_room.id, random_examination.id);

                        if (!is_feasible)
                        {
                            neighbor.Reverse();
                            continue;
                        }

                        solution.UnsetExam(exam_to_swap_id);
                        is_feasible = feasibility_tester.IsFeasiblePeriodRoom(solution, examinations.GetById(exam_to_swap_id),
                                exam_to_swap_period, exam_to_swap_room);
                        solution.SetExam(exam_to_swap_period.id, exam_to_swap_room.id, exam_to_swap_id);

                        if (!is_feasible)
                        {
                            neighbor.Reverse();
                            continue;
                        }
                        neighbor.Reverse();
                        return neighbor;
                    }
                }
            }

            return null;
        }

        public INeighbor PeriodChange(Solution solution)
        {
            Examination random_examination = examinations.GetById(new Random((int)DateTime.Now.Ticks).Next(examinations.EntryCount()));
            Room room = rooms.GetById(solution.GetRoomFrom(random_examination.id));
            int random_period_id = new Random((int)DateTime.Now.Ticks).Next(periods.EntryCount());

            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                Period random_period = periods.GetById((period_id + random_period_id) % periods.EntryCount());
                if (solution.GetPeriodFrom(random_examination.id) == random_period.id)
                    continue;
                if (feasibility_tester.IsFeasiblePeriodRoom(solution, random_examination, random_period, room))
                    return new PeriodChangeNeighbor (solution, random_examination.id, random_period.id);
            }
            return null;
        }

        public INeighbor RoomChange(Solution solution)
        {
            Examination random_examination = examinations.GetById(new Random((int)DateTime.Now.Ticks).Next(examinations.EntryCount()));
            Period period = periods.GetById(solution.GetPeriodFrom(random_examination.id));
            int random_room_id = new Random((int)DateTime.Now.Ticks).Next(rooms.EntryCount());
            for (int room_id = 0; room_id < rooms.EntryCount(); ++room_id)
            {
                Room random_room = rooms.GetById((room_id + random_room_id) % rooms.EntryCount());
                if (solution.GetRoomFrom(random_examination.id) == random_room.id)
                    continue;
                if (feasibility_tester.IsFeasibleRoom(solution, random_examination, period, random_room))
                    return new RoomChangeNeighbor(solution, random_examination.id, random_room.id);
            }
            return null;
        }

        public INeighbor PeriodRoomChange(Solution solution)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            Examination random_examination = examinations.GetById(random.Next(examinations.EntryCount()));
            int random_period_id = random.Next(periods.EntryCount());
            int random_room_id = random.Next(rooms.EntryCount());

            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                Period random_period = periods.GetById((period_id + random_period_id) % periods.EntryCount());
                if (!feasibility_tester.IsFeasiblePeriod(solution, random_examination, random_period))
                    continue;
                for (int room_id = 0; room_id < rooms.EntryCount(); ++room_id)
                {
                    Room random_room = rooms.GetById((room_id + random_room_id) % rooms.EntryCount());
                    if (solution.GetPeriodFrom(random_examination.id) == random_period.id ||
                        solution.GetRoomFrom(random_examination.id) == random_room.id)
                        continue;
                    if (feasibility_tester.IsFeasibleRoom(solution, random_examination, random_period, random_room))
                        return new PeriodRoomChangeNeighbor(solution, random_examination.id, random_period.id, random_room.id);
                }
            }
            return null;
        }
    }
}
