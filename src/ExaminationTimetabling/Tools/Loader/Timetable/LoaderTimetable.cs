using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Business;
using DAL;

namespace Tools.Loader.Timetable
{
    public class LoaderTimetable
    {
        private Examinations examinations;
        private PeriodHardConstraints period_hard_constraints;
        private Periods periods;
        private RoomHardConstraints room_hard_constraints;
        private Rooms rooms;
        private ModelWeightings model_weightings;
        private Solutions solutions;

        private readonly Loader loader;

        public LoaderTimetable(string path)
        {
            loader = new Loader(path);
        }

        public void Exec()
        {
            InitExaminations();
            InitPeriods();
            InitRooms();
            InitPeriodHardConstraints();
            InitRoomHardConstraints();
            InitInstitutionalWeightings();
            InitSolutions();
        }

        private void InitSolutions()
        {
            Solutions.Instance(1);
        }

        private void InitInstitutionalWeightings()
        {
            //throw new NotImplementedException();
        }

        private void InitRoomHardConstraints()
        {
            //throw new NotImplementedException();
        }

        private void InitPeriodHardConstraints()
        {
            //throw new NotImplementedException();
        }

        private void InitRooms()
        {
            //throw new NotImplementedException();
        }

        private void InitPeriods()
        {
            //throw new NotImplementedException();
        }

        public void InitExaminations()
        {
            loader.Restart();

            if(!loader.NextLine())
                throw new Exception("There was an error reading the timetable file.");
            string token = loader.ReadToken();
            if (token == null || !token.Contains("Exams"))
                throw new Exception("There was an error reading the timetable file.");
            //string[] n_exams_s = Regex.Split(token, "\\d+");
            string n_exams_s = new Regex("\\d+").Match(token).Captures[0].Value;
            int n_exams = Convert.ToInt32(n_exams_s);
            
            examinations = Examinations.Instance(n_exams);

            for (int exam_id = 0; exam_id < n_exams && loader.NextLine(); exam_id++)
            {
                token = loader.ReadToken();
                int duration = Convert.ToInt32(token);
                List<int> students = new List<int>();

                while ((token = loader.ReadToken()) != null)
                {
                    students.Add(Convert.ToInt32(token));
                }
                examinations.Insert(new Examination(exam_id, duration, students));
            }
        }
    }
}
