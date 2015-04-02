using Business;
using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristics
{
    public class GraphColoring
    {
        Examinations examinations;
        PeriodHardConstraints period_hard_constraints;
        Periods periods;
        RoomHardConstraints room_hard_constraints;
        Rooms rooms;
        Solutions solutions;


        public bool[,] conflict_matrix;

        public GraphColoring(Examinations examinations, PeriodHardConstraints period_hard_constraints,
            Periods periods, RoomHardConstraints room_hard_constraints, Rooms rooms, Solutions solutions)
        {
            this.examinations = examinations;
            this.period_hard_constraints = period_hard_constraints;
            this.periods = periods;
            this.room_hard_constraints = room_hard_constraints;
            this.rooms = rooms;
            this.solutions = solutions;
        }

        public void Work()
        {
            conflict_matrix = new bool[examinations.EntryCount(), examinations.EntryCount()];

            PopulateConflictMatrix();

            PeriodSelection();

            RoomSelection();
        }

        private void PopulateConflictMatrix()
        {
            for (int x = 0; x < conflict_matrix.GetLength(0); x += 1)
            {
                for (int y = 0; y < conflict_matrix.GetLength(1); y += 1)
                {
                    if (x == y)
                        conflict_matrix[x, y] = false;
                    else if (x > y)
                        conflict_matrix[x, y] = conflict_matrix[y, x];
                    else
                        if (examinations.Conflict(x, y))
                            conflict_matrix[x, y] = true;
                        else
                            conflict_matrix[x, y] = false;
                }
            }
        }

        private void PeriodSelection()
        {

        }

        private void RoomSelection()
        {

        }
    }
}
