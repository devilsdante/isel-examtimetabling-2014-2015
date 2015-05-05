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

        /*******************/

        IRepository<Examination> examinations_repo;

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
            //foreach (int student in ex1.students)
            //    if (ex2.students.Contains(student))
            //        return true;
            //return false;

            foreach (int student1 in ex1.students)
            {
                foreach (int student2 in ex2.students)
                {
                    if (student1 > student2)
                        break;
                    if (student1 == student2)
                    {
                        return true;
                    }
                }
                
            }
            return false;
        }

        public int NoOfConflicts(Examination ex1, Examination ex2)
        {
            int count = 0;

            foreach (int student1 in ex1.students)
            {
                foreach (int student2 in ex2.students)
                {
                    if (student1 > student2)
                        break;
                    if (student1 == student2) {
                        ++count; break;
                    }
                }
            }

            return count;
        }
    }
}
