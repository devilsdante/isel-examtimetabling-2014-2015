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

        public static void Kill()
        {
            instance = null;
        }

        /*******************/

        readonly IRepository<RoomHardConstraint> rhc_repo;

        private RoomHardConstraints(int size)
        {
            rhc_repo = new Repository<RoomHardConstraint>(size);
        }

        public void Insert(RoomHardConstraint rhc)
        {
            rhc_repo.Insert(rhc);
        }

        public void Delete(RoomHardConstraint rhc)
        {
            rhc_repo.Delete(rhc);
        }

        public IEnumerable<RoomHardConstraint> GetAll()
        {
            return rhc_repo.GetAll();
        }

        public RoomHardConstraint GetById(int id)
        {
            return rhc_repo.GetById(id);
        }

        public bool HasRoomExclusivity(int exam_id)
        {
            return
                GetAll().Any(rhc => rhc.type == RoomHardConstraint.types.ROOM_EXCLUSIVE && rhc.examination == exam_id);
        }
        //public IEnumerable<RoomHardConstraint> GetByTypeWithExamId(RoomHardConstraint.types type, int exam_id)
        //{
        //    return GetAll().Where(rhc => rhc.type == type && rhc.examination == exam_id);
        //}

        public IEnumerable<RoomHardConstraint> GetByType(RoomHardConstraint.types type)
        {
            return GetAll().Where(rhc => rhc.type == type);
        }

    }
}
