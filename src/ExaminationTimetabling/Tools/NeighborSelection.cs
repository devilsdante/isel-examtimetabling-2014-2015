﻿using System;
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
        private readonly EvaluationFunction evaluation_function;

        public NeighborSelection()
        {
            examinations = Examinations.Instance();
            rooms = Rooms.Instance();
            periods = Periods.Instance();
            feasibility_tester = new FeasibilityTester();
            evaluation_function = new EvaluationFunction();

        }

        public INeighbor PeriodRoomChange(Solution solution)
        {
            //examination and a new period are randomly selected. If no conflict results from 
            //assigning the selected exam to the new period,
            //(into a randomly selected available room) the new assignment is returned
            Examination random_examination = examinations.GetById(new Random((int)DateTime.Now.Ticks).Next(examinations.EntryCount()));
            Random random = new Random((int) DateTime.Now.Ticks);
            int random_period_id = random.Next(periods.EntryCount());
            int random_room_id = random.Next(rooms.EntryCount());

            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                Period random_period = periods.GetById((period_id + random_period_id)%periods.EntryCount());
                for (int room_id = 0; room_id < rooms.EntryCount(); ++room_id)
                {
                    Room random_room = rooms.GetById((room_id + random_room_id)%rooms.EntryCount());
                    if (solution.epr_associasion[random_examination.id, 0] == random_period.id &&
                        solution.epr_associasion[random_examination.id, 1] == random_room.id)
                        continue;
                    PeriodRoomChangeNeighbor prc_neighbor = new PeriodRoomChangeNeighbor(solution, random_examination.id, random_period.id, random_room.id);
                    prc_neighbor.Accept();
                    if (evaluation_function.DistanceToFeasibility(solution) == 0)
                    {
                        prc_neighbor.Reverse();
                        return prc_neighbor;
                    }
                    prc_neighbor.Reverse();
                }
            }
            return null;
        }

        public INeighbor RoomSwap(Solution solution)
        {
            //An examination and a new room are randomly selected. If just one conflict results 
            //from assigning the selected exam into the new room (keeping its period assignment), 
            //and it is possible to swap these two exams (each keeping its period assignment and taking
            //the room assignment of the other exam), such a swap is returned
            return null;
        }

        public INeighbor PeriodSwap(Solution solution)
        {
            //An examination and a new period are randomly selected. If just one conflict
            //results from assigning the selected exam to the new period (keeping its room assignment),
            //and it is possible to swap these two exams (each keeping its room assignment and taking the
            //period assignment of the other exam), such a swap is returned. The following periods are
            //tried otherwise.
            return null;
        }

        public INeighbor PeriodRoomSwap(Solution solution)
        {
            //An examination is randomly selected. A new period and room are randomly selected.
            //If there is only one conflicting examination, and it is possible to swap the 
            //selected examination with it, such a swap is returned.
            return null;
        }

        public INeighbor PeriodChange(Solution solution)
        {
            Examination random_examination = examinations.GetById(new Random((int)DateTime.Now.Ticks).Next(examinations.EntryCount()));
            int random_period_id = new Random((int)DateTime.Now.Ticks).Next(periods.EntryCount());
            Room room_id = rooms.GetById(solution.epr_associasion[random_examination.id, 1]);

            for (int period_id = 0; period_id < periods.EntryCount(); ++period_id)
            {
                Period random_period = periods.GetById((period_id + random_period_id) % periods.EntryCount());
                if (solution.epr_associasion[random_examination.id, 0] == random_period.id)
                    continue;
                if (feasibility_tester.IsFeasiblePeriod(solution, random_examination, random_period) &&
                    feasibility_tester.IsFeasibleRoom(solution, random_examination, random_period, room_id))
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
                if (random_examination.students.Count() > random_room.capacity)
                    continue;
                if (feasibility_tester.IsFeasibleRoom(solution, random_examination, period, random_room))
                    return new RoomChangeNeighbor(solution, random_examination.id, random_room.id);
            }
            return null;
        }
    }
}
