using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public interface ISolution : IEntity
    {
        int fitness { get; set; }
        ISolution Copy();
        bool Equals(ISolution solution);
    }
}
