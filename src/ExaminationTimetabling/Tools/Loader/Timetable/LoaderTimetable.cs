using System;
using System.Collections;
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
    public class LoaderTimetable : Loader
    {
        private Examinations examinations;
        private PeriodHardConstraints period_hard_constraints;
        private Periods periods;
        private RoomHardConstraints room_hard_constraints;
        private Rooms rooms;
        private ModelWeightings model_weightings;
        private ConflictMatrix conflict_matrix;
        private Dictionary<int, List<int>> student_examinations;
        public Dictionary<int, List<int>> examination_examinations_conflicts; 

        //rivate readonly Loader loader;

        public LoaderTimetable(string path) : base(path)
        {
            
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
            NextLine();

            while (true)
            {
                string token = ReadCurrToken() ?? ReadNextToken();
                if (token == null)
                    break;
                if (token.Contains("Exams"))
                {
                    InitExaminations();
                }
                else if (token.Contains("Periods"))
                {
                    InitPeriods();
                }
                else if (token.Contains("Rooms"))
                {
                    InitRooms();
                }
                else if (token.Contains("PeriodHardConstraints"))
                {
                    InitPeriodHardConstraints();
                }
                else if (token.Contains("RoomHardConstraints"))
                {
                    InitRoomHardConstraints();
                }
                else if (token.Contains("InstitutionalWeightings"))
                {
                    InitInstitutionalWeightings();
                }
                else if (!NextLine())
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
            while (NextLine() && !(token = ReadNextToken()).StartsWith("["))
            {
                string type = token;
                token = ReadNextToken();

                if (type.Equals("TWOINAROW"))
                    imws.two_in_a_row = Convert.ToInt32(token);
                else if (type.Equals("TWOINADAY"))
                    imws.two_in_a_day = Convert.ToInt32(token);
                else if (type.Equals("PERIODSPREAD"))
                    imws.period_spread = Convert.ToInt32(token);
                else if (type.Equals("NONMIXEDDURATIONS"))
                    imws.non_mixed_durations = Convert.ToInt32(token);
                else if (type.Equals("FRONTLOAD"))
                    imws.front_load = new [] { Convert.ToInt32(token), Convert.ToInt32(ReadNextToken()), Convert.ToInt32(ReadNextToken()) };
            }

            model_weightings.Set(imws);
        }

        private void InitRoomHardConstraints()
        {
            room_hard_constraints = RoomHardConstraints.Instance(10);

            string token;
            for (int id = 0; NextLine() && !(token = ReadNextToken()).StartsWith("["); id++)
            {
                int examination = Convert.ToInt32(token);

                token = ReadNextToken();
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
            for (int id = 0; NextLine() && !(token = ReadNextToken()).StartsWith("["); id++)
            {
                int exam1 = Convert.ToInt32(token);

                token = ReadNextToken();
                PeriodHardConstraint.types phc_type = (Enum.GetValues(typeof (PeriodHardConstraint.types))).Cast<PeriodHardConstraint.types>().FirstOrDefault(type => token.Equals(type.ToString()));

                token = ReadNextToken();
                int exam2 = Convert.ToInt32(token);

                period_hard_constraints.Insert(new PeriodHardConstraint(id, exam1, phc_type, exam2));
            }

        }

        private void InitRooms()
        {
            string token = ReadCurrToken();
            string n_rooms_s = new Regex("\\d+").Match(token).Captures[0].Value;
            int n_rooms = Convert.ToInt32(n_rooms_s);

            rooms = Rooms.Instance(n_rooms);

            for (int room_id = 0; NextLine() && room_id < n_rooms; room_id++)
            {
                token = ReadNextToken();
                int capacity = Convert.ToInt32(token);

                token = ReadNextToken();
                int penalty = Convert.ToInt32(token);
                
                rooms.Insert(new Room(room_id, capacity, penalty));
            }
        }

        private void InitPeriods()
        {
            string token = ReadCurrToken();
            string n_periods_s = new Regex("\\d+").Match(token).Captures[0].Value;
            int n_periods = Convert.ToInt32(n_periods_s);

            periods = Periods.Instance(n_periods);

            for (int period_id = 0; NextLine() && period_id < n_periods; period_id++)
            {
                token = ReadNextToken().Replace(':', '/');
                token += " " + ReadNextToken();
                DateTime date = Convert.ToDateTime(token);

                token = ReadNextToken();
                int duration = Convert.ToInt32(token);

                token = ReadNextToken();
                int penalty = Convert.ToInt32(token);

                periods.Insert(new Period(period_id, date, duration, penalty));
            }
        }

        private void InitExaminations()
        {
            string token = ReadCurrToken();
            string n_exams_s = new Regex("\\d+").Match(token).Captures[0].Value;
            int n_exams = Convert.ToInt32(n_exams_s);
            
            examinations = Examinations.Instance(n_exams);
            student_examinations = new Dictionary<int, List<int>>();

            for (int exam_id = 0; NextLine() && exam_id < n_exams; exam_id++)
            {
                token = ReadNextToken();
                int duration = Convert.ToInt32(token);
                int student_count = 0;

                while ((token = ReadNextToken()) != null)
                {
                    int id = Convert.ToInt32(token);
                    if (!student_examinations.ContainsKey(id))
                    {
                        student_examinations.Add(id, new List<int>{exam_id});
                    }
                    else
                    {
                        student_examinations[id].Add(exam_id);
                    }
                    student_count++;
                }

                examinations.Insert(new Examination(exam_id, duration, student_count));
            
            }
        }

        private void InitConflictMatrix()
        {
            conflict_matrix = ConflictMatrix.Instance();
            int[,] matrix = new int[examinations.EntryCount(), examinations.EntryCount()];
            examination_examinations_conflicts = new Dictionary<int, List<int>>();

            foreach (KeyValuePair<int, List<int>> entry in student_examinations)
            {
                List<int> exams = entry.Value;
                for (int i = 0; i < exams.Count; i++)
                {
                    for (int j = i + 1; j < exams.Count; j++)
                    {
                        if (!examination_examinations_conflicts.ContainsKey(exams[i]))
                        {
                            examination_examinations_conflicts.Add(exams[i], new List<int> { exams[j] });
                        }
                        else if (!examination_examinations_conflicts[exams[i]].Contains(exams[j]))
                        {
                            examination_examinations_conflicts[exams[i]].Add(exams[j]);
                        }
                        if (!examination_examinations_conflicts.ContainsKey(exams[j]))
                        {
                            examination_examinations_conflicts.Add(exams[j], new List<int> { exams[i] });
                        }
                        else if (!examination_examinations_conflicts[exams[j]].Contains(exams[i]))
                        {
                            examination_examinations_conflicts[exams[j]].Add(exams[i]);
                        }
                        matrix[exams[i], exams[j]]++;
                        matrix[exams[j], exams[i]]++;
                        examinations.GetById(exams[i]).conflict++;
                        examinations.GetById(exams[j]).conflict++;
                    }
                }
            }
            conflict_matrix.Set(matrix);
        }
    }
}
