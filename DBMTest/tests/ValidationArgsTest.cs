using System;
using System.Collections.Generic;
using System.Text;
using DBMProgram.src;
using Xunit;

namespace DBMTest.tests
{
    public class ValidationArgsTest
    {
        readonly bool InvalidResult = false;
        readonly bool ValidResult = true;
        [Fact]
        public void IsValidConn_CorrectConnString_ValidResult()
        {
            Options options = new Options
            {
                ConnString = "Data source=US-NY-8W1RQ32;Initial Catalog=Version_test;Integrated Security=True;"
            };
            Assert.Equal(ValidResult,options.IsValidConn());
        }
        [Fact]
        public void IsValidConn_IncorrectDataSource_InvalidResult()
        {
            Options options = new Options
            {
                ConnString = "Data source=US-NY-8W1RQ;Initial Catalog=Version_test;Integrated Security=True;"
            };
            Assert.Equal(InvalidResult,options.IsValidConn());
        }

        [Fact]
        public void IsValidPath_CorrectLocalPath_ValidResult()
        {
            Options options = new Options
            {
                RootPath = "..\\..\\..\\ScriptTest"
            };
            Assert.Equal(ValidResult, options.IsValidPath());
        }
        [Fact]
        public void IsValidPath_RootPathWithoutDDLAndDML_InvalidResult()
        {
            Options options = new Options
            {
                RootPath = "..\\..\\..\\ScriptTest\\DDL"
            };
            Assert.Equal(InvalidResult,options.IsValidPath());
        }

        [Fact]
        public void IsValidSubstituteList_CorrectFormatVariable_ValidResult()
        {
            Options options = new Options
            {
                SubsituteList = new List<string>() { "var1:Version_test","var2:test2" }
            };
            Assert.Equal(ValidResult, options.IsValidSubsituteList());
        }

        [Fact]
        public void IsValidSubstituteList_IncorrectFormatVariable_InvalidResult()
        {
            Options options = new Options
            {
                SubsituteList = new List<string>() { "var1", "var2:test2" }
            };
            Assert.Equal(InvalidResult, options.IsValidSubsituteList());
        }
    }
}
