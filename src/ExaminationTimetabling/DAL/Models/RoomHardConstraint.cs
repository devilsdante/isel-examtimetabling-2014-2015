using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class RoomHardConstraint : IEntity
    {
        public int id { get; set; }
        public int examination { get; set; }
        public enum types { ROOM_EXCLUSIVE };
        public types type { get; set; }

        public RoomHardConstraint(int id, int examination, types type)
        {
            this.id = id;
            this.examination = examination;
            this.type = type;
        }
    }
}
