using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class Periods
    {
        private static Periods instance;

        public static Periods Instance(int size)
        {
            if (instance == null)
                instance = new Periods(size);
            return instance;
        }

        public static Periods Instance()
        {
            return instance;
        }

        /*******************/

        IRepository<Period> periods_repo;

        private Periods(int size)
        {
            periods_repo = new Repository<Period>(size);
        }


        public void Insert(Period exam)
        {
            periods_repo.Insert(exam);
        }
        public void Delete(Period exam)
        {
            periods_repo.Delete(exam);
        }
        public IEnumerable<Period> GetAll()
        {
            return periods_repo.GetAll();
        }
        public Period GetById(int id)
        {
            return periods_repo.GetById(id);
        }

        public IEnumerable<Period> GetAllThatFitsDuration(int duration)
        {
            return GetAll().Where(p => p.duration >= duration).OrderBy(p => p.duration);
        }

        public int EntryCount()
        {
            return periods_repo.EntryCount();
        }
    }
}
