using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tools.Loader
{
    public class Loader
    {
        private string path;
        private string phrase;
        private string[] tokens;
        private int idx;
        private StreamReader file;
        private string curr_token;

        public Loader(string path)
        {
            idx = 0;
            this.path = path;
            this.curr_token = null;
            if (!File.Exists(path))
            {
                throw new Exception("File to load does not exist.");
            }
            file = new StreamReader(path);
        }

        public void Restart()
        {
            file.Close();
            file = new StreamReader(path);
        }

        public bool NextLine()
        {
            phrase = file.ReadLine();
            if (phrase == null) 
                return false;
            tokens = Regex.Split(phrase, ",");
            idx = 0;
            curr_token = null;
            return true;
        }

        public string ReadNextToken()
        {
            return idx < tokens.Length ? curr_token = tokens[idx++].Trim() : null;
        }

        public string ReadCurrToken()
        {
            return curr_token;
        }
    }
}
