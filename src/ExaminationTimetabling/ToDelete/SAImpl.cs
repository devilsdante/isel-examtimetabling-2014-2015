using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDelete
{
    class SAImpl : SA
    {
        protected IEFunction<ISolution> evaluation { get; set; }

        protected override INeighbor GenerateNeighbor(ISolution solution, types type)
        {
            throw new NotImplementedException();
        }

        protected override void InitVals(types type)
        {
            throw new NotImplementedException();
        }

        public SAImpl()
        {
            evaluation = new EvaluationFunctionExaminations();
        }
    }
}
