using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business;
using DAL;
using DAL.Models;
using Tools.Neighborhood;

namespace Tools
{
    public class NeighborSelection
    {
        private readonly Examinations examinations;
        private readonly Rooms rooms;
        private readonly Periods periods;
        private readonly FeasibilityTester feasibility_tester;
        private readonly EvaluationFunctionTimetabling _evaluationFunctionTimetabling;

        public NeighborSelection()
        {
            examinations = Examinations.Instance();
            rooms = Rooms.Instance();
            periods = Periods.Instance();
            feasibility_tester = new FeasibilityTester();
            _evaluationFunctionTimetabling = new EvaluationFunctionTimetabling();

        }

        public INeighbor RoomSwap(Solution solution)
        {
            Random random = new Random((int) DateTime.Now.Ticks);
            Examination random_examination = examinations.GetById(random.Next(examinations.EntryCount()));
            Period period = periods.GetById(solution.epr_associasion[random_examination.id, 0]);
            int random_room_id = random.Next(rooms.EntryCount());

            for (int room_id = 0; room_id < rooms.EntryCount(); ++room_id)
            {
                Room random_room = rooms.GetById((room_id + random_room_id) % rooms.EntryCount());
                if (solution.epr_associasion[random_examination.id, 1] == random_room.id)
                    continue;
                if (feasibility_tester.IsFeasibleRoom(solution, random_examination, period, random_room))
                    return new RoomChangeNeighbor(solution, random_examination.id, random_room.id);

                for (int exam_id = 0; exam_id < examinations.EntryCount(); ++exam_id)
                {
                    if (!solution.timetable_container[period.id, random_room.id, exam_id])
                        continue;
                    INeighbor neighbor = new RoomSwapNeighbor(solution, random_examination.id, exam_id);

                    if (_evaluationFunctionTimetabling.DistanceToFeasibility(neighbor) == 0)
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
            Room room = rooms.GetById(solution.epr_associasion[random_examination.id, 1]);

            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                Period random_period = periods.GetById((period_id + random_period_id) % periods.EntryCount());
                if (solution.epr_associasion[random_examination.id, 0] == random_period.id)
                    continue;
                if (feasibility_tester.IsFeasiblePeriod(solution, random_examination, random_period) &&
                    feasibility_tester.IsFeasibleRoom(solution, random_examination, random_period, room))
                    return new PeriodChangeNeighbor(solution, random_examination.id, random_period.id);
                    

                for (int exam_id = 0; exam_id < examinations.EntryCount(); ++exam_id)
                {
                    if (!solution.timetable_container[random_period.id, room.id, exam_id])
                        continue;

                    INeighbor neighbor = new PeriodSwapNeighbor(solution, random_examination.id, exam_id);
                    if (_evaluationFunctionTimetabling.DistanceToFeasibility(neighbor) == 0)
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
                    if (solution.epr_associasion[random_examination.id, 0] == random_period.id ||
                        solution.epr_associasion[random_examination.id, 1] == random_room.id)
                        continue;

                    if (feasibility_tester.IsFeasibleRoom(solution, random_examination, random_period, random_room))
                        return new PeriodRoomChangeNeighbor(solution, random_examination.id, random_period.id, random_room.id);
                        

                    for (int exam_id = 0; exam_id < examinations.EntryCount(); ++exam_id)
                    {
                        if (!solution.timetable_container[random_period.id, random_room.id, exam_id])
                            continue;
                        INeighbor neighbor = new PeriodRoomSwapNeighbor(solution, random_examination.id, exam_id);

                        if (_evaluationFunctionTimetabling.DistanceToFeasibility(neighbor) == 0)
                            return neighbor;
                    }
                }
            }
            return null;
        }

        public INeighbor PeriodChange(Solution solution)
        {
            Examination random_examination = examinations.GetById(new Random((int)DateTime.Now.Ticks).Next(examinations.EntryCount()));
            Room room = rooms.GetById(solution.epr_associasion[random_examination.id, 1]);
            int random_period_id = new Random((int)DateTime.Now.Ticks).Next(periods.EntryCount());

            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                Period random_period = periods.GetById((period_id + random_period_id) % periods.EntryCount());
                if (solution.epr_associasion[random_examination.id, 0] == random_period.id)
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
            Period period = periods.GetById(solution.epr_associasion[random_examination.id, 0]);
            int random_room_id = new Random((int)DateTime.Now.Ticks).Next(rooms.EntryCount());
            for (int room_id = 0; room_id < rooms.EntryCount(); ++room_id)
            {
                Room random_room = rooms.GetById((room_id + random_room_id) % rooms.EntryCount());
                if (solution.epr_associasion[random_examination.id, 1] == random_room.id)
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
                    if (solution.epr_associasion[random_examination.id, 0] == random_period.id ||
                        solution.epr_associasion[random_examination.id, 1] == random_room.id)
                        continue;
                    if (feasibility_tester.IsFeasibleRoom(solution, random_examination, random_period, random_room))
                        return new PeriodRoomChangeNeighbor(solution, random_examination.id, random_period.id, random_room.id);
                    //PeriodRoomChangeNeighbor prc_neighbor = new PeriodRoomChangeNeighbor(solution, random_examination.id, random_period.id, random_room.id);
                    //prc_neighbor.Accept();
                    //if (_evaluationFunctionTimetabling.DistanceToFeasibility(solution) == 0)
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
