using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class PeriodHardConstraints
    {
        IRepository<PeriodHardConstraint> period_hard_constraints_repo;

        public PeriodHardConstraints(int size)
        {
            period_hard_constraints_repo = Repository<PeriodHardConstraint>.Instance(size);
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
        public IEnumerable<PeriodHardConstraint> GetByType(PeriodHardConstraint.types type)
        {
            return GetAll().Where(p => p.type == type);
        }
        public IEnumerable<PeriodHardConstraint> GetByTypeWithExamId(PeriodHardConstraint.types type, int exam_id)
        {
            return GetAll().Where(p => p.type == type && (p.ex1 == exam_id || p.ex2 == exam_id));
        }
    }
}
