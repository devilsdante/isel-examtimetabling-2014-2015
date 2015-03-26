using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class RoomHardConstraints
    {
        IRepository<RoomHardConstraint> _roomhardconstraints;

        public RoomHardConstraints()
        {
            _roomhardconstraints = Repository<RoomHardConstraint>.Instance;
        }

        public void Insert(RoomHardConstraint exam)
        {
            _roomhardconstraints.Insert(exam);
        }
        public void Delete(RoomHardConstraint exam)
        {
            _roomhardconstraints.Delete(exam);
        }
        public IEnumerable<RoomHardConstraint> GetAll()
        {
            return _roomhardconstraints.GetAll();
        }
        public RoomHardConstraint GetById(int id)
        {
            return _roomhardconstraints.GetById(id);
        }
    }
}
