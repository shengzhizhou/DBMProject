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
        readonly bool SuccessResult = true;

        [Fact]
        public void GetAllScripts_CorrectLocalUnexecutedFiles_AssertSuccessResult()
        {
            UnityContainer ScriptContainer = Factory.ConfigureContainer();
            IScriptExecutor scriptExecutor = ScriptContainer.Resolve<SqlServerScriptExecutor>();
            foreach (UnexecutedScript script in scriptExecutor.GetAllScripts(subDirPath)) {
                if (!(script.ScriptName == "newScript"||script.ScriptName == "script_1" || script.ScriptName == "script_2"))
                    Assert.False(SuccessResult);
            }
            Assert.True(SuccessResult);
        }

        [Fact]
        public void SkipScript_GivenSkipScript_AssertSuccessResult()
        {
            var script1 = new UnexecutedScript("X_MCRA_10101-A");
            if(!script1.IsSkip())
                Assert.False(SuccessResult);
            Assert.True(SuccessResult);
        }

    }

    
}
