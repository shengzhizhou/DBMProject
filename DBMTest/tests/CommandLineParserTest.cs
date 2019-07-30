using System;
using DBMProgram.src;
using CommandLine;
using Xunit;
using System.Collections.Generic;

namespace DBMTest.tests
{
    public class ArgsParserTest
    {
        readonly bool BadResult = false;
        readonly bool CorrectResult = true;
        
        [Fact]
        public void CommandLineParser_TwoCorrectArgs_CorrectResult()
        {
            string[] args =new string[] { "-r", "\\ScriptTest", "-c", "Data source = US - NY - 8W1RQ32;Initial Catalog = Version_test;Integrated Security = True;" };
            bool result=false;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => { result = true; })
    .WithNotParsed<Options>((errs) => { result = false; });
            Assert.Equal(CorrectResult,result);
        }

        [Fact]
        public void CommandLineParser_MissingRootPath_BadResult()
        {
            string[] args = new string[] { "-c", "Data source = US - NY - 8W1RQ32;Initial Catalog = Version_test;Integrated Security = True;" };
            bool result = false;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => { result = true; })
    .WithNotParsed<Options>((errs) => { result = false; });
            Assert.Equal(BadResult, result);
        }

        [Fact]
        public void CommandLineParser_MissingConnectionString_BadResult()
        {
            string[] args = new string[] { "-r", "\\ScriptTest" };
            bool result = false;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => { result = true; })
    .WithNotParsed<Options>((errs) => { result = false; });
            Assert.Equal(BadResult, result);
        }

        [Fact]
        public void CommandLineParser_EmptyArgs_BadResult()
        {
            string[] args = new string[] { "" };
            bool result = false;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => { result = true; })
    .WithNotParsed<Options>((errs) => { result = false; });
            Assert.Equal(BadResult, result);
        }

        [Fact]
        public void CommandLineParser_TwoWrongArgs_BadResult()
        {
            string[] args = new string[] { "-f", "\\script", "-d", "Data source = US - NY - 8W1RQ32;Initial Catalog = Version_test;Integrated Security = True;" };
            bool result = false;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => { result = true; })
    .WithNotParsed<Options>((errs) => { result = false; });
            Assert.Equal(BadResult, result);
        }
    }
}
