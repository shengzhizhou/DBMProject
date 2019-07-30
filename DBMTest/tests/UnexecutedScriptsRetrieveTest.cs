using System.Collections.Generic;
using Xunit;
using Unity;
using System.Linq;
using DBMProgram.src;

namespace DBMTest.tests
{
    public class UnexecutedScriptsRetrieveTest
    {
        readonly string subDirPath = "..\\..\\..\\ScriptTest\\DDL";
        readonly bool CorrectResult = true;

        [Fact]
        public void GetAllScriptsTest_CorrectLocalUnexecutedFiles_CorrectResult()
        {
            UnityContainer ScriptContainer = Factory.ConfigureContainer();
            IScriptExecutor scriptExecutor = ScriptContainer.Resolve<SqlServerScriptExecutor>();
            List<UnexecutedScript> actulScripts = (List<UnexecutedScript>)scriptExecutor.GetAllScripts(subDirPath);
            bool result = true;

            //List<Script> trueScript = new List<Script>() { new Script($"{relativePath}\\newScript"), new Script($"{relativePath}\\script_1"), new Script($"{relativePath}\\script_2") };
            foreach (UnexecutedScript script in actulScripts) {
                if (!(script.ScriptName == "script_1" || script.ScriptName == "script_2"))
                    result = false;
            }

            Assert.Equal(CorrectResult, result);
        } 
    }

    
}
