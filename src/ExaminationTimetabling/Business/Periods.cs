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
        IRepository<Period> _periods;

        public Periods()
        {
            _periods = Repository<Period>.Instance;
        }


        public void Insert(Period exam)
        {
            _periods.Insert(exam);
        }
        public void Delete(Period exam)
        {
            _periods.Delete(exam);
        }
        public IEnumerable<Period> GetAll()
        {
            return _periods.GetAll();
        }
        public Period GetById(int id)
        {
            return _periods.GetById(id);
        }
    }
}
