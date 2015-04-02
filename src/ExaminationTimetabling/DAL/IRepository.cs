using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public interface IRepository<T>
    {
        void Insert(T entity);
        void Delete(T entity);
        IEnumerable<T> GetAll();
        T GetById(int id);
        int EntryCount();
    }
}
