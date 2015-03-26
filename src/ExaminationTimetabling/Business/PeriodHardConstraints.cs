using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class PeriodHardConstraints
    {
        IRepository<PeriodHardConstraint> _periodhardconstraints;

        public PeriodHardConstraints()
        {
            _periodhardconstraints = Repository<PeriodHardConstraint>.Instance;
        }

        public void Insert(PeriodHardConstraint exam)
        {
            _periodhardconstraints.Insert(exam);
        }
        public void Delete(PeriodHardConstraint exam)
        {
            _periodhardconstraints.Delete(exam);
        }
        public IEnumerable<PeriodHardConstraint> GetAll()
        {
            return _periodhardconstraints.GetAll();
        }
        public PeriodHardConstraint GetById(int id)
        {
            return _periodhardconstraints.GetById(id);
        }
    }
}
