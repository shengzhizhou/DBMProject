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

        public UnexecutedScript(string filePath)
        {
            FilePath = filePath;
            ScriptName = Path.GetFileNameWithoutExtension(filePath);

        }
        private void SplitNameConvention()
        {
            TicketName = ScriptName.Substring(0, ScriptName.IndexOf('_'));
            ExecutionOrder = Convert.ToInt32(ScriptName.Substring(ScriptName.IndexOf('_') + 1, ScriptName.IndexOf('-') - ScriptName.IndexOf('_') - 1));

            Description = ScriptName.Substring(ScriptName.IndexOf('-'));

        }

        private void SplitBothNameConvention(UnexecutedScript script2) {
            SplitNameConvention();
            script2.SplitNameConvention();
        }

        public bool MatchesNameConvention()
        {
            Regex rgx = new Regex(@"^([a-zA-Z0-9]+)(_\d+)(-[a-zA-Z0-9]+)$");
            return rgx.IsMatch(ScriptName);
        }
        public bool Skip()
        {
            return ScriptName.StartsWith('X');
        }

        public void LoadScript()
        {
            string text = File.ReadAllText(FilePath);
            Batches = text.Split("GO");
        }


        public int CompareTo(UnexecutedScript script2)
        {
            if (!(MatchesNameConvention() && script2.MatchesNameConvention()))
            {
                return string.Compare(ScriptName, script2.ScriptName);
            }
            else
            {
                SplitBothNameConvention(script2);
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
