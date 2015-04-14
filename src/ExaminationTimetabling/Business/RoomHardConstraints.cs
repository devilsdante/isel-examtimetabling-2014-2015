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
        IRepository<RoomHardConstraint> room_hard_constraints_repo;

        public RoomHardConstraints(int size)
        {
            room_hard_constraints_repo = Repository<RoomHardConstraint>.Instance(size);
        }

        public void Insert(RoomHardConstraint exam)
        {
            room_hard_constraints_repo.Insert(exam);
        }
        public void Delete(RoomHardConstraint exam)
        {
            room_hard_constraints_repo.Delete(exam);
        }
        public IEnumerable<RoomHardConstraint> GetAll()
        {
            return room_hard_constraints_repo.GetAll();
        }
        public RoomHardConstraint GetById(int id)
        {
            return room_hard_constraints_repo.GetById(id);
        }
        public bool HasRoomExclusivesWithExam(int exam_id)
        {
            return
                GetAll().Any(rhc => rhc.type == RoomHardConstraint.types.ROOM_EXCLUSIVE && rhc.examination == exam_id);
        }

        public bool HasExclusivity(int room_id)
        {
            return GetAll().Any(rhc => rhc.)
        }
    }
}
