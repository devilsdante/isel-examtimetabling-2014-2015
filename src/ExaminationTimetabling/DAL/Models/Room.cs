using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Room : IEntity
    {
        public int id { get; set; }
        public int capacity { get; set; }
        public int penalty { get; set; }

        public Room(int id, int capacity, int penalty)
        {
            this.id = id;
            this.capacity = capacity;
            this.penalty = penalty;
        }
    }
}
