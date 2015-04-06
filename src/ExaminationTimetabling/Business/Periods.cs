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
        IRepository<Period> periods_repo;

        public Periods()
        {
            periods_repo = Repository<Period>.Instance;
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
    }
}
