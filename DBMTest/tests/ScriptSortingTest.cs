using DBMProgram.src;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DBMTest.tests
{
    public class ScriptSortingTest
    {
        readonly bool SuccessSorting = true;
        [Fact]
        public void SortingUnexecutedScript_GivenDifferentExecutionOrderScript_SuccessResult() {
            var script1 = new UnexecutedScript("MCRTECH_10101-udpate");
            var script2 = new UnexecutedScript("MCRTECH_10102-udpate");
            var script3 = new UnexecutedScript("MCRTECH_10103-udpate");
            List<UnexecutedScript> unexecutedScripts = new List<UnexecutedScript>();
            unexecutedScripts.Add(script3);
            unexecutedScripts.Add(script2);
            unexecutedScripts.Add(script1);
            unexecutedScripts.Sort();
            if (!(unexecutedScripts.IndexOf(script1) == 0 && unexecutedScripts.IndexOf(script2) == 1 && unexecutedScripts.IndexOf(script3) == 2)) {
                Assert.False(SuccessSorting);
            }
            Assert.True(SuccessSorting);
        }

        [Fact]
        public void SortingUnexecutedScript_GivenDifferentTicketScript_SuccessResult()
        {
            var script1 = new UnexecutedScript("MCRA_10101-udpate");
            var script2 = new UnexecutedScript("MCRB_10102-udpate");
            var script3 = new UnexecutedScript("MCRC_10103-udpate");
            List<UnexecutedScript> unexecutedScripts = new List<UnexecutedScript>();
            unexecutedScripts.Add(script3);
            unexecutedScripts.Add(script2);
            unexecutedScripts.Add(script1);
            unexecutedScripts.Sort();
            if (!(unexecutedScripts.IndexOf(script1) == 0 && unexecutedScripts.IndexOf(script2) == 1 && unexecutedScripts.IndexOf(script3) == 2))
            {
                Assert.False(SuccessSorting);
            }
            Assert.True(SuccessSorting);
        }

        [Fact]
        public void SortingUnexecutedScript_GivenDifferentDescriptionScript_SuccessResult()
        {
            var script1 = new UnexecutedScript("MCRA_10101-A");
            var script2 = new UnexecutedScript("MCRA_10101-B");
            var script3 = new UnexecutedScript("MCRA_10101-C");
            List<UnexecutedScript> unexecutedScripts = new List<UnexecutedScript>();
            unexecutedScripts.Add(script3);
            unexecutedScripts.Add(script2);
            unexecutedScripts.Add(script1);
            unexecutedScripts.Sort();
            if (!(unexecutedScripts.IndexOf(script1) == 0 && unexecutedScripts.IndexOf(script2) == 1 && unexecutedScripts.IndexOf(script3) == 2))
            {
                Assert.False(SuccessSorting);
            }
            Assert.True(SuccessSorting);
        }

        [Fact]
        public void SortingUnexecutedScript_GivenMixScript_SuccessResult()
        {
            var script1 = new UnexecutedScript("MCRA_");
            var script2 = new UnexecutedScript("MCRB_10101-B");
            var script3 = new UnexecutedScript("MCRC_10");
            List<UnexecutedScript> unexecutedScripts = new List<UnexecutedScript>();
            unexecutedScripts.Add(script3);
            unexecutedScripts.Add(script2);
            unexecutedScripts.Add(script1);
            unexecutedScripts.Sort();
            if (!(unexecutedScripts.IndexOf(script1) == 0 && unexecutedScripts.IndexOf(script2) == 1 && unexecutedScripts.IndexOf(script3) == 2))
            {
                Assert.False(SuccessSorting);
            }
            Assert.True(SuccessSorting);
        }
    }
}
