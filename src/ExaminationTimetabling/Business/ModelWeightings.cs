using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class ModelWeightings
    {
        private InstitutionalModelWeightings imw;

        public ModelWeightings(){
        }

        public ModelWeightings(InstitutionalModelWeightings imw)
        {
            this.imw = imw;
        }

        public InstitutionalModelWeightings Get()
        {
            return imw;
        }

        public void Set(InstitutionalModelWeightings imw)
        {
            this.imw = imw;
        }
    }
}
