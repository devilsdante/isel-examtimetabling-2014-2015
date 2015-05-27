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
        private static Examinations instance;

        public static Examinations Instance(int size)
        {
            if(instance == null)
                instance = new Examinations(size);
            return instance;
        }

        public static Examinations Instance()
        {
            return instance;
        }

        public static void Kill()
        {
            instance = null;
        }

        /*******************/

        readonly IRepository<Examination> examinations_repo;

        private Examinations(int size)
        {
            examinations_repo = new Repository<Examination>(size);
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

        public bool Conflict(Examination ex1, Examination ex2)
        {
            int i = 0, j = 0;
            List<int> students1 = (List<int>) ex1.students;
            List<int> students2 = (List<int>) ex2.students;

            while (i < students1.Count() && j < students2.Count())
            {
                if (students1[i] == students2[j])
                    return true;
                if (students1[i] < students2[j])
                    ++i;
                else
                    ++j;
            }
            return false;
        }

        public int NoOfConflicts(Examination ex1, Examination ex2)
        {
            int count = 0;

            int i = 0, j = 0;
            List<int> students1 = (List<int>)ex1.students;
            List<int> students2 = (List<int>)ex2.students;

            while (i < students1.Count() && j < students2.Count())
            {
                if (students1[i] == students2[j])
                    count++;
                if (students1[i] < students2[j])
                    ++i;
                else
                    ++j;
            }

            return count;
        }
    }
}
