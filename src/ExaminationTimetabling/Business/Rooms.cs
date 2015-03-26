using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class Rooms
    {
        IRepository<Room> _rooms;

        public Rooms()
        {
            _rooms = Repository<Room>.Instance;
        }

        public void Insert(Room exam)
        {
            _rooms.Insert(exam);
        }
        public void Delete(Room exam)
        {
            _rooms.Delete(exam);
        }
        public IEnumerable<Room> GetAll()
        {
            return _rooms.GetAll();
        }
        public Room GetById(int id)
        {
            return _rooms.GetById(id);
        }
    }
}
