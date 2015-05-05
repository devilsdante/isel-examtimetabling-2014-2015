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
        private static RoomHardConstraints instance;

        public static RoomHardConstraints Instance(int size)
        {
            if (instance == null)
                instance = new RoomHardConstraints(size);
            return instance;
        }

        public static RoomHardConstraints Instance()
        {
            return instance;
        }

        /*******************/

        IRepository<RoomHardConstraint> room_hard_constraints_repo;

        private RoomHardConstraints(int size)
        {
            room_hard_constraints_repo = new Repository<RoomHardConstraint>(size);
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
        public IEnumerable<RoomHardConstraint> GetByTypeWithExamId(RoomHardConstraint.types type, int exam_id)
        {
            return GetAll().Where(rhc => rhc.type == type && rhc.examination == exam_id);
        }

        public IEnumerable<RoomHardConstraint> GetByType(RoomHardConstraint.types type)
        {
            return GetAll().Where(rhc => rhc.type == type);
        }

    }
}
