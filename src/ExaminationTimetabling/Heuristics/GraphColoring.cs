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
        public int[] conflicts;

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

            conflicts = new int[examinations.EntryCount()];

            PopulateConflictMatrix();

            EraseCoincidenceHCWithConflict();

            PeriodSelection();

            RoomSelection();
        }

        private void PopulateConflictMatrix()
        {
            // student conflicts //
            for (int x = 0; x < conflict_matrix.GetLength(0); x += 1)
            {
                for (int y = 0; y < conflict_matrix.GetLength(1); y += 1)
                {
                    if (x == y)
                        conflict_matrix[x, y] = false;
                    else if (x > y)
                        conflict_matrix[x, y] = conflict_matrix[y, x];
                    else if (examinations.Conflict(x, y))
                    {
                        conflict_matrix[x, y] = true;
                        conflicts[x] += 1;
                        conflicts[y] += 1;
                    }
                    else
                        conflict_matrix[x, y] = false;
                }
            }

            // exclusion hard constraints //
            IEnumerable<PeriodHardConstraint> exclusions = period_hard_constraints.GetByType(PeriodHardConstraint.types.EXCLUSION);
            foreach (PeriodHardConstraint phc in exclusions)
            {
                if (conflict_matrix[phc.ex1, phc.ex2] == false)
                {
                    conflict_matrix[phc.ex1, phc.ex2] = true;
                    conflict_matrix[phc.ex2, phc.ex1] = true;

                    conflicts[phc.ex1] += 1;
                    conflicts[phc.ex2] += 1;
                }
            }
        }
        private void EraseCoincidenceHCWithConflict()
        {
            foreach (PeriodHardConstraint coincidence in period_hard_constraints.GetByType(PeriodHardConstraint.types.EXAM_COINCIDENCE))
            {
                if (conflict_matrix[coincidence.ex1, coincidence.ex2])
                {
                    period_hard_constraints.Delete(coincidence);
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
