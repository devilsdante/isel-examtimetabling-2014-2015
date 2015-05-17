using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace ToDelete
{
    public interface ISolution : IEntity
    {
        int fitness { get; set; }
    }
}
