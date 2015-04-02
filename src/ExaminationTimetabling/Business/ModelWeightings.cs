using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    class ModelWeightings
    {
        public InstitutionalModelWeightings imw;

        public ModelWeightings(){
            imw = new InstitutionalModelWeightings();
        }

        public ModelWeightings(InstitutionalModelWeightings imw)
        {
            this.imw = imw;
        }
    }
}
