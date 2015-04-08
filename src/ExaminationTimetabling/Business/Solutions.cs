﻿using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class Solutions
    {
        IRepository<Solution> solutions_repo;

        public Solutions(int size)
        {
            solutions_repo = Repository<Solution>.Instance(size);
        }

        public void Insert(Solution solution)
        {
            solutions_repo.Insert(solution);
        }
        public void Delete(Solution solution)
        {
            solutions_repo.Delete(solution);
        }
        public IEnumerable<Solution> GetAll()
        {
            return solutions_repo.GetAll();
        }
        public Solution GetById(int id)
        {
            return solutions_repo.GetById(id);
        }
    }
}
