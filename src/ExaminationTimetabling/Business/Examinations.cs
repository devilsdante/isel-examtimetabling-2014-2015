using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class Examinations
    {
        IRepository<Examination> examinations_repo;

        public Examinations()
        {
            examinations_repo = Repository<Examination>.Instance;
        }

        public void Insert(Examination exam)
        {
            examinations_repo.Insert(exam);
        }
        public void Delete(Examination exam)
        {
            examinations_repo.Delete(exam);
        }
        public IEnumerable<Examination> GetAll()
        {
            return examinations_repo.GetAll();
        }
        public Examination GetById(int id)
        {
            return examinations_repo.GetById(id);
        }

        public int EntryCount()
        {
            return examinations_repo.EntryCount();
        }

        public bool Conflict(int x, int y)
        {
            Examination ex1 = GetById(x);
            Examination ex2 = GetById(y);

            foreach (int student in ex1.students)
                if (ex2.students.Contains(student))
                    return true;
            return false;
        }
    }
}
