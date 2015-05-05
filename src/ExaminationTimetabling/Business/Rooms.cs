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
        private static Rooms instance;

        public static Rooms Instance(int size)
        {
            if (instance == null)
                instance = new Rooms(size);
            return instance;
        }

        public static Rooms Instance()
        {
            return instance;
        }

        /*******************/

        IRepository<Room> rooms_repo;

        private Rooms(int size)
        {
            rooms_repo = new Repository<Room>(size);
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
        public int EntryCount()
        {
            return rooms_repo.EntryCount();
        }
    }
}
