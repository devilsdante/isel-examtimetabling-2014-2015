using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class PeriodHardConstraints
    {
        private static PeriodHardConstraints instance;

        public static PeriodHardConstraints Instance(int size)
        {
            if (instance == null)
                instance = new PeriodHardConstraints(size);
            return instance;
        }

        public static PeriodHardConstraints Instance()
        {
            return instance;
        }

        public static void Kill()
        {
            instance = null;
        }

        /*******************/

        readonly IRepository<PeriodHardConstraint> phc_repo;

        private PeriodHardConstraints(int size)
        {
            phc_repo = new Repository<PeriodHardConstraint>(size);
        }

        public void Insert(PeriodHardConstraint phc)
        {
            phc_repo.Insert(phc);
        }
        public void Delete(PeriodHardConstraint phc)
        {
            phc_repo.Delete(phc);
        }
        public IEnumerable<PeriodHardConstraint> GetAll()
        {
            return phc_repo.GetAll();
        }
        public PeriodHardConstraint GetById(int id)
        {
            return phc_repo.GetById(id);
        }
        public IEnumerable<PeriodHardConstraint> GetByType(PeriodHardConstraint.types type)
        {
            return GetAll().Where(phc => phc.type == type);
        }
        public IEnumerable<PeriodHardConstraint> GetByTypeWithExamId(PeriodHardConstraint.types type, int exam_id)
        {
            return GetAll().Where(phc => phc.type == type && (phc.ex1 == exam_id || phc.ex2 == exam_id));
        }

        public IEnumerable<int> GetExamsWithChainingCoincidence(int exam_id)
        {
            List<int> list = new List<int> { exam_id };
            if (!GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE, exam_id).Any())
                return list;
            GetExamsWithChainingCoincidenceAux(list);
            return list;
        }

        private void GetExamsWithChainingCoincidenceAux(List<int> exams)
        {
            List<int> exams_aux = exams.ToList();

            foreach (int exam_id in exams_aux)
            {
                foreach (
                    PeriodHardConstraint phc in
                        GetByTypeWithExamId(PeriodHardConstraint.types.EXAM_COINCIDENCE, exam_id))
                {
                    int exam_id2 = phc.ex1 == exam_id ? phc.ex2 : phc.ex1;

                    if (!exams.Contains(exam_id2))
                        exams.Add(exam_id2);
                }
            }

            if (exams.Count != exams_aux.Count)
                GetExamsWithChainingCoincidenceAux(exams);
        }
    }
}
