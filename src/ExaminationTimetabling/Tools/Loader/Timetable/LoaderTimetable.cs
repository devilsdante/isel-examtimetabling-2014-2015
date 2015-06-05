using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Business;
using DAL;
using DAL.Models;

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
        private ConflictMatrix conflict_matrix;

        private readonly Loader loader;

        public LoaderTimetable(string path)
        {
            loader = new Loader(path);

        }

        public void Unload()
        {
            Examinations.Kill();
            PeriodHardConstraints.Kill();
            Periods.Kill();
            RoomHardConstraints.Kill();
            Rooms.Kill();
            ModelWeightings.Kill();
            ConflictMatrix.Kill();
        }

        public void Load()
        {
            loader.NextLine();

            while (true)
            {
                string token = loader.ReadCurrToken() ?? loader.ReadNextToken();
                if (token == null)
                    break;
                if (token.Contains("Exams"))
                {
                    InitExaminations();
                    continue;
                }
                else if (token.Contains("Periods"))
                {
                    InitPeriods();
                    continue;
                }
                else if (token.Contains("Rooms"))
                {
                    InitRooms();
                    continue;
                }
                else if (token.Contains("PeriodHardConstraints"))
                {
                    InitPeriodHardConstraints();
                    continue;
                }
                else if (token.Contains("RoomHardConstraints"))
                {
                    InitRoomHardConstraints();
                    continue;
                }
                else if (token.Contains("InstitutionalWeightings"))
                {
                    InitInstitutionalWeightings();
                    continue;
                }

                if (!loader.NextLine())
                    break;
            }

            InitSolutions();
            InitConflictMatrix();
        }

        private void InitSolutions()
        {
            Solutions.Instance(1);
        }

        private void InitInstitutionalWeightings()
        {
            model_weightings = ModelWeightings.Instance();

            string token;
            InstitutionalModelWeightings imws = new InstitutionalModelWeightings();
            while (loader.NextLine() && !(token = loader.ReadNextToken()).StartsWith("["))
            {
                string type = token;
                token = loader.ReadNextToken();

                if (type.Equals("TWOINAROW"))
                    imws.two_in_a_row = Convert.ToInt32(token);
                else if (type.Equals("TWOINADAY"))
                    imws.two_in_a_day = Convert.ToInt32(token);
                else if (type.Equals("PERIODSPREAD"))
                    imws.period_spread = Convert.ToInt32(token);
                else if (type.Equals("NONMIXEDDURATIONS"))
                    imws.non_mixed_durations = Convert.ToInt32(token);
                else if (type.Equals("FRONTLOAD"))
                    imws.front_load = new [] { Convert.ToInt32(token), Convert.ToInt32(loader.ReadNextToken()), Convert.ToInt32(loader.ReadNextToken()) };
            }

            model_weightings.Set(imws);
        }

        private void InitRoomHardConstraints()
        {
            room_hard_constraints = RoomHardConstraints.Instance(10);

            string token;
            for (int id = 0; loader.NextLine() && !(token = loader.ReadNextToken()).StartsWith("["); id++)
            {
                int examination = Convert.ToInt32(token);

                token = loader.ReadNextToken();
                RoomHardConstraint.types rhc_type = default(RoomHardConstraint.types);
                foreach (RoomHardConstraint.types type in (Enum.GetValues(typeof(RoomHardConstraint.types))).Cast<RoomHardConstraint.types>().Where(type => token.Equals(type.ToString())))
                {
                    rhc_type = type;
                    break;
                }

                room_hard_constraints.Insert(new RoomHardConstraint(id, examination, rhc_type));
            }
        }

        private void InitPeriodHardConstraints()
        {
            period_hard_constraints = PeriodHardConstraints.Instance(10);

            string token;
            for (int id = 0; loader.NextLine() && !(token = loader.ReadNextToken()).StartsWith("["); id++)
            {
                int exam1 = Convert.ToInt32(token);

                token = loader.ReadNextToken();
                PeriodHardConstraint.types phc_type = (Enum.GetValues(typeof (PeriodHardConstraint.types))).Cast<PeriodHardConstraint.types>().FirstOrDefault(type => token.Equals(type.ToString()));

                token = loader.ReadNextToken();
                int exam2 = Convert.ToInt32(token);

                period_hard_constraints.Insert(new PeriodHardConstraint(id, exam1, phc_type, exam2));
            }

        }

        private void InitRooms()
        {
            string token = loader.ReadCurrToken();
            string n_rooms_s = new Regex("\\d+").Match(token).Captures[0].Value;
            int n_rooms = Convert.ToInt32(n_rooms_s);

            rooms = Rooms.Instance(n_rooms);

            for (int room_id = 0; loader.NextLine() && room_id < n_rooms; room_id++)
            {
                token = loader.ReadNextToken();
                int capacity = Convert.ToInt32(token);

                token = loader.ReadNextToken();
                int penalty = Convert.ToInt32(token);
                
                rooms.Insert(new Room(room_id, capacity, penalty));
            }
        }

        private void InitPeriods()
        {
            string token = loader.ReadCurrToken();
            string n_periods_s = new Regex("\\d+").Match(token).Captures[0].Value;
            int n_periods = Convert.ToInt32(n_periods_s);

            periods = Periods.Instance(n_periods);

            for (int period_id = 0; loader.NextLine() && period_id < n_periods; period_id++)
            {
                token = loader.ReadNextToken().Replace(':', '/');
                token += " " + loader.ReadNextToken();
                DateTime date = Convert.ToDateTime(token);

                token = loader.ReadNextToken();
                int duration = Convert.ToInt32(token);

                token = loader.ReadNextToken();
                int penalty = Convert.ToInt32(token);

                periods.Insert(new Period(period_id, date, duration, penalty));
            }
        }

        public void InitExaminations()
        {
            string token = loader.ReadCurrToken();
            string n_exams_s = new Regex("\\d+").Match(token).Captures[0].Value;
            int n_exams = Convert.ToInt32(n_exams_s);
            
            examinations = Examinations.Instance(n_exams);

            for (int exam_id = 0; loader.NextLine() && exam_id < n_exams; exam_id++)
            {
                token = loader.ReadNextToken();
                int duration = Convert.ToInt32(token);
                List<int> students = new List<int>();

                while ((token = loader.ReadNextToken()) != null)
                {
                    students.Add(Convert.ToInt32(token));
                }
                //There are so exceptions in some examination files in which student ordering 'rule' is not accomplished
                students.Sort();
                examinations.Insert(new Examination(exam_id, duration, students));
            }
        }

        private void InitConflictMatrix()
        {
            conflict_matrix = ConflictMatrix.Instance();

            //Stopwatch watch = new Stopwatch();
            //watch.Restart();

            int[,] matrix = new int[examinations.EntryCount(), examinations.EntryCount()];

            // student conflicts //
            for (int exam1_id = 0; exam1_id < examinations.EntryCount(); exam1_id += 1)
            {
                for (int exam2_id = exam1_id; exam2_id < examinations.EntryCount(); exam2_id += 1)
                {
                    if (exam1_id == exam2_id)
                        matrix[exam1_id, exam2_id] = 0;
                    else
                    {
                        Examination exam1 = examinations.GetById(exam1_id);
                        Examination exam2 = examinations.GetById(exam2_id);
                        int conflicts = examinations.NoOfConflicts(exam1, exam2);
                        matrix[exam1_id, exam2_id] += conflicts;
                        matrix[exam2_id, exam1_id] += conflicts;
                        exam1.conflict += conflicts;
                        exam2.conflict += conflicts;
                    }
                }
            }
            
            conflict_matrix.Set(matrix);
            //Console.WriteLine("Conflict Matrix: " + watch.ElapsedMilliseconds);
        }
    }
}
