﻿using System;
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

                for (int exam_id = 0; exam_id < examinations.EntryCount(); ++exam_id)
                {
                    if (!solution.IsExamSetTo(period.id, random_room.id, exam_id))
                        continue;
                    INeighbor neighbor = new RoomSwapNeighbor(solution, random_examination.id, exam_id);

                    if (_evaluationFunctionTimetable.DistanceToFeasibility(neighbor) == 0)
                    {
                        return neighbor;
                    }
                }
            }
            return null;
        }

        public INeighbor PeriodSwap(Solution solution)
        {
            Examination random_examination = examinations.GetById(new Random((int)DateTime.Now.Ticks).Next(examinations.EntryCount()));
            int random_period_id = new Random((int)DateTime.Now.Ticks).Next(periods.EntryCount());
            Room room = rooms.GetById(solution.GetRoomFrom(random_examination.id));

            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                Period random_period = periods.GetById((period_id + random_period_id) % periods.EntryCount());
                if (solution.GetPeriodFrom(random_examination.id) == random_period.id)
                    continue;
                if (feasibility_tester.IsFeasiblePeriod(solution, random_examination, random_period) &&
                    feasibility_tester.IsFeasibleRoom(solution, random_examination, random_period, room))
                    return new PeriodChangeNeighbor(solution, random_examination.id, random_period.id);
                    

                for (int exam_id = 0; exam_id < examinations.EntryCount(); ++exam_id)
                {
                    if (!solution.IsExamSetTo(random_period.id, room.id, exam_id))
                        continue;

                    INeighbor neighbor = new PeriodSwapNeighbor(solution, random_examination.id, exam_id);
                    if (_evaluationFunctionTimetable.DistanceToFeasibility(neighbor) == 0)
                        return neighbor;
                }

            }
            return null;
        }

        public INeighbor PeriodRoomSwap(Solution solution)
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
                        

                    for (int exam_id = 0; exam_id < examinations.EntryCount(); ++exam_id)
                    {
                        if (!solution.IsExamSetTo(random_period.id, random_room.id, exam_id))
                            continue;
                        INeighbor neighbor = new PeriodRoomSwapNeighbor(solution, random_examination.id, exam_id);

                        if (_evaluationFunctionTimetable.DistanceToFeasibility(neighbor) == 0)
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
                if (feasibility_tester.IsFeasiblePeriod(solution, random_examination, random_period) &&
                    feasibility_tester.IsFeasibleRoom(solution, random_examination, random_period, room))
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
                    //PeriodRoomChangeNeighbor prc_neighbor = new PeriodRoomChangeNeighbor(solution, random_examination.id, random_period.id, random_room.id);
                    //prc_neighbor.Accept();
                    //if (_evaluationFunctionTimetable.DistanceToFeasibility(solution) == 0)
                    //{
                    //    prc_neighbor.Reverse();
                    //    return prc_neighbor;
                    //}
                    //prc_neighbor.Reverse();
                }
            }
            return null;
        }
    }
}
