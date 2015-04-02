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
        IRepository<Room> rooms_repo;

        public Rooms()
        {
            rooms_repo = Repository<Room>.Instance;
        }

        public void Insert(Room exam)
        {
            rooms_repo.Insert(exam);
        }
        public void Delete(Room exam)
        {
            rooms_repo.Delete(exam);
        }
        public IEnumerable<Room> GetAll()
        {
            return rooms_repo.GetAll();
        }
        public Room GetById(int id)
        {
            return rooms_repo.GetById(id);
        }
    }
}
