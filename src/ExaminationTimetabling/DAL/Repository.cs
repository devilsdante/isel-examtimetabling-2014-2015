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

        public static Repository<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Repository<T>();
                }
                return instance;
            }
        }

        /*******************/

        protected IDictionary<int, T> map;

        private Repository()
        {
            map = new Dictionary<int, T>();
        }

        public void Insert(T entity)
        {
            map.Add(entity.id, entity);
        }

        public void Delete(T entity)
        {
            map.Remove(entity.id);
        }

        public IEnumerable<T> GetAll()
        {
            return map.Values.ToList();
        }

        public T GetById(int id)
        {
            if (map.ContainsKey(id))
                return map[id];
            else
                return null;
        }

        public int EntryCount()
        {
            return map.Count;
        }
    }
}
