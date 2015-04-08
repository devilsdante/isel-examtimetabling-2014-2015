using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        /*
         * Singleton Pattern
         */

        private static Repository<T> instance;

        public static Repository<T> Instance(int size)
        {
                if (instance == null)
                {
                    instance = new Repository<T>(size);
                }
                return instance;
        }

        /*******************/

        protected List<T> list;

        private Repository(int size)
        {
            list = new List<T>(size);
        }

        public void Insert(T entity)
        {
            list.Insert(entity.id, entity);
        }

        public void Delete(T entity)
        {
            list.RemoveAt(entity.id);
        }

        public IEnumerable<T> GetAll()
        {
            return list.ToList();
        }

        public T GetById(int id)
        {
            return list[id];
        }

        public int EntryCount()
        {
            return list.Count;
        }
    }
}
