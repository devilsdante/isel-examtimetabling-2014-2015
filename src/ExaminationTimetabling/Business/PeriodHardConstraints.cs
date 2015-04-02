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
        IRepository<PeriodHardConstraint> period_hard_constraints_repo;

        public PeriodHardConstraints()
        {
            period_hard_constraints_repo = Repository<PeriodHardConstraint>.Instance;
        }

        public void Insert(PeriodHardConstraint exam)
        {
            period_hard_constraints_repo.Insert(exam);
        }
        public void Delete(PeriodHardConstraint exam)
        {
            period_hard_constraints_repo.Delete(exam);
        }
        public IEnumerable<PeriodHardConstraint> GetAll()
        {
            return period_hard_constraints_repo.GetAll();
        }
        public PeriodHardConstraint GetById(int id)
        {
            return period_hard_constraints_repo.GetById(id);
        }
    }
}
