using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class ConflictMatrix
    {

        private static ConflictMatrix instance;

        public static ConflictMatrix Instance()
        {
            if (instance == null)
                instance = new ConflictMatrix();
            return instance;
        }

        public static ConflictMatrix Instance(int[,] conflict_matrix)
        {
            if (instance == null)
                instance = new ConflictMatrix(conflict_matrix);
            return instance;
        }

        public static void Kill()
        {
            instance = null;
        }

        /*******************/

        private int[,] conflict_matrix;

        public ConflictMatrix(int[,] conflict_matrix)
        {
            this.conflict_matrix = conflict_matrix;
        }

        public ConflictMatrix()
        {
            
        }

        public int[,] Get()
        {
            return conflict_matrix;
        }

        public void Set(int[,] conflict_matrix)
        {
            this.conflict_matrix = conflict_matrix;
        }


    }
}
