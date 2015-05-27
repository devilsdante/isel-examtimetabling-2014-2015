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

        private static ModelWeightings instance;

        public static ModelWeightings Instance()
        {
            if(instance == null)
                instance = new ModelWeightings();
            return instance;
        }

        public static ModelWeightings Instance(InstitutionalModelWeightings imw)
        {
            if (instance == null)
                instance = new ModelWeightings(imw);
            return instance;
        }

        public static void Kill()
        {
            instance = null;
        }

        /*******************/

        private InstitutionalModelWeightings imw;

        private ModelWeightings()
        {

        }

        private ModelWeightings(InstitutionalModelWeightings imw)
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
