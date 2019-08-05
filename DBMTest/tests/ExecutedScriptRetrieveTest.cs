using System.Collections.Generic;
using Xunit;
using Unity;
using DBMProgram.src;

namespace DBMTest.tests
{
    public class ExecutedScriptRetrieveTest
    {
        //readonly string connString = "Data source=US-NY-8W1RQ32;Initial Catalog=Version_test;Integrated Security=True;";
        //readonly bool CorrectResult = true;

        //[Fact]
        //public void GetExecutedScriptName_CorrectExecutedFile_CorrectResult()
        //{
        //    UnityContainer ScriptContainer = Factory.ConfigureContainer();
        //    IScriptExecutor scriptExecutor = ScriptContainer.Resolve<SqlServerScriptExecutor>();
        //    List<string> actulScripts = (List<string>)scriptExecutor.GetExecutedScriptNames(connString);
        //    bool result= true;
        //    foreach (string scriptName in actulScripts)
        //    {
        //        if (!(scriptName == "newScript"))
        //            result = false;
        //    }
        //    Assert.Equal(CorrectResult, result);
        //}
    }
}
