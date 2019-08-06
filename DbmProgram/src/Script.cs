using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DBMProgram.src
{
    public class UnexecutedScript : IComparable<UnexecutedScript>
    {
        readonly string FilePath;
        public string ScriptName { get; set; }
        public string[] Batches;
        private int ExecutionOrder;
        private string TicketName;
        private string Description;
        readonly bool IsMatch;
        public UnexecutedScript(string filePath)
        {
            FilePath = filePath;
            ScriptName = Path.GetFileNameWithoutExtension(filePath);
            if (IsMatchesNameConvention())
            {
                SplitNameByConvention();
                IsMatch = true;
            }
            else IsMatch = false;
        }
        private void SplitNameByConvention()
        {
            TicketName = ScriptName.Substring(0, ScriptName.IndexOf('_'));
            ExecutionOrder = Convert.ToInt32(ScriptName.Substring(ScriptName.IndexOf('_') + 1, ScriptName.IndexOf('-') - ScriptName.IndexOf('_') - 1));
            Description = ScriptName.Substring(ScriptName.IndexOf('-'));
        }


        public bool IsMatchesNameConvention()
        {
            Regex rgx = new Regex(@"^([a-zA-Z0-9]+)(_\d+)(-[a-zA-Z0-9]+)$");
            return rgx.IsMatch(ScriptName);
        }
        public bool IsSkip()
        {
            return ScriptName.StartsWith('X') || ScriptName.StartsWith('x');
        }

        public void LoadScript()
        {
            
            string text = File.ReadAllText(FilePath);
            Batches = Regex.Split(text, "GO", RegexOptions.IgnoreCase);

        }


        public int CompareTo(UnexecutedScript script2)
        {
            if (!(IsMatch && script2.IsMatch))
            {
                return string.Compare(ScriptName, script2.ScriptName);
            }
            else
            {
                int result = TicketName.CompareTo(script2.TicketName);
                if (result == 0)
                    result = ExecutionOrder.CompareTo(script2.ExecutionOrder);
                if (result == 0)
                    result = Description.CompareTo(script2.Description);
                return result;
            }
        }
    }

    public class ExecutedScript
    {
        readonly string ScriptID;
        public string ScriptName;
        readonly DateTime AppliedDate;

        public ExecutedScript(string id, string name, string date)
        {
            ScriptID = id;
            ScriptName = name;
            AppliedDate = Convert.ToDateTime(date);
        }


    }
}
