using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business;
using DAL;
using DAL.Models;

namespace Tools
{
    public class NeighborSelection
    {
        private readonly Examinations examinations;
        private readonly PeriodHardConstraints period_hard_constraints;
        private readonly RoomHardConstraints room_hard_constraints;
        private readonly Rooms rooms;
        private readonly Periods periods;
        private readonly FeasibilityTester feasibilityTester;

        public NeighborSelection(Examinations examinations, PeriodHardConstraints period_hard_constraints, RoomHardConstraints room_hard_constraints,
            Rooms rooms, Periods periods)
        {
            this.examinations = examinations;
            this.period_hard_constraints = period_hard_constraints;
            this.room_hard_constraints = room_hard_constraints;
            this.rooms = rooms;
            this.periods = periods;
            feasibilityTester = new FeasibilityTester(examinations, period_hard_constraints, room_hard_constraints, rooms);

        }

        public Solution RoomMove(Solution solution)
        {
            Examination random_examination = examinations.GetById(new Random((int) DateTime.Now.Ticks).Next(examinations.EntryCount()));
            Period period = periods.GetById(solution.epr_associasion[random_examination.id, 0]);
            int random_room_id = new Random((int) DateTime.Now.Ticks).Next(rooms.EntryCount());
            for (int room_id = 0; room_id < rooms.EntryCount(); ++room_id)
            {
                Room random_room = rooms.GetById((room_id+random_room_id)%rooms.EntryCount());
                if (solution.epr_associasion[random_examination.id, 1] == random_room.id)
                    continue;
                if (random_examination.students.Count() > random_room.capacity)
                    continue;
                if (feasibilityTester.IsFeasibleRoom(solution, random_examination, period, random_room))
                    return ReplaceRoom(solution, random_examination, period, random_room);
            }
            return null;
        }

        private Solution ReplaceRoom(Solution solution, Examination examination, Period period, Room new_room)
        {
            Solution neighbor = solution.Copy();
            neighbor.timetable_container[period.id, solution.epr_associasion[examination.id, 1], examination.id] = false;

            neighbor.timetable_container[period.id, new_room.id, examination.id] = true;
            neighbor.epr_associasion[examination.id, 1] = new_room.id;
            return neighbor;
        }

        public Solution ExaminationSwap(Solution solution)
        {
            return null;
        }

        public Solution PeriodMove(Solution solution)
        {
            return null;
        }
    }
}
