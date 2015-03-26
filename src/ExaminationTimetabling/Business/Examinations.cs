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
        IRepository<Examination> _examinations;

        public Examinations()
        {
            _examinations = Repository<Examination>.Instance;
        }

        public void Insert(Examination exam)
        {
            _examinations.Insert(exam);
        }
        public void Delete(Examination exam)
        {
            _examinations.Delete(exam);
        }
        public IEnumerable<Examination> GetAll()
        {
            return _examinations.GetAll();
        }
        public Examination GetById(int id)
        {
            return _examinations.GetById(id);
        }

    }
}
