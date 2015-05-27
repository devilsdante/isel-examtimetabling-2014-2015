using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Solution.Timetabling;

namespace Business
{
    public class Solutions
    {
        private static Solutions instance;

        public static Solutions Instance(int size)
        {
            if (instance == null)
                instance = new Solutions(size);
            return instance;
        }

        public static Solutions Instance()
        {
            return instance;
        }

        public static void Kill()
        {
            instance = null;
        }

        /*******************/

        readonly IRepository<Solution> solutions_repo;

        private Solutions(int size)
        {
            solutions_repo = new Repository<Solution>(size);
        }

        public void Insert(Solution solution)
        {
            solutions_repo.Insert(solution);
        }
        public void Delete(Solution solution)
        {
            solutions_repo.Delete(solution);
        }
        public IEnumerable<Solution> GetAll()
        {
            return solutions_repo.GetAll();
        }
        public Solution GetById(int id)
        {
            return solutions_repo.GetById(id);
        }
    }
}
