using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class InstitutionalModelWeightings
    {
        public int two_in_a_row;
        public int two_in_a_day;
        public int period_spread;
        public int[] front_load;
        public int non_mixed_durations;

        public InstitutionalModelWeightings(int two_in_a_row, int two_in_a_day, int period_spread, int[] front_load,
            int non_mixed_durations)
        {
            this.two_in_a_row = two_in_a_row;
            this.two_in_a_day = two_in_a_day;
            this.period_spread = period_spread;
            this.front_load = front_load;
            this.non_mixed_durations = non_mixed_durations;
        }
    }
}
