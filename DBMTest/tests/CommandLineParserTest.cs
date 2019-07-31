using System;
using DBMProgram.src;
using CommandLine;
using Xunit;
using System.Collections.Generic;

namespace DBMTest.tests
{
    public class ArgsParserTest
    {
        readonly bool FailurePaser = false;
        readonly bool SuccessParser = true;
        
        [Fact]
        public void CommandLineParser_TwoCorrectArgs_SuccessResult()
        {
            string[] args =new string[] { "-r", "\\ScriptTest", "-c", "Data source = US - NY - 8W1RQ32;Initial Catalog = Version_test;Integrated Security = True;" };
            bool result=false;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => { result = true; })
    .WithNotParsed<Options>((errs) => { result = false; });
            Assert.Equal(SuccessParser,result);
        }

        [Fact]
        public void CommandLineParser_MissingRootPath_FailureResult()
        {
            string[] args = new string[] { "-c", "Data source = US - NY - 8W1RQ32;Initial Catalog = Version_test;Integrated Security = True;" };
            bool result = false;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => { result = true; })
    .WithNotParsed<Options>((errs) => { result = false; });
            Assert.Equal(FailurePaser, result);
        }

        [Fact]
        public void CommandLineParser_MissingConnectionString_FailureResult()
        {
            string[] args = new string[] { "-r", "\\ScriptTest" };
            bool result = false;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => { result = true; })
    .WithNotParsed<Options>((errs) => { result = false; });
            Assert.Equal(FailurePaser, result);
        }

        [Fact]
        public void CommandLineParser_EmptyArgs_FailureResult()
        {
            string[] args = new string[] { "" };
            bool result = false;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => { result = true; })
    .WithNotParsed<Options>((errs) => { result = false; });
            Assert.Equal(FailurePaser, result);
        }

        [Fact]
        public void CommandLineParser_TwoWrongArgs_FailureResult()
        {
            string[] args = new string[] { "-f", "\\script", "-d", "Data source = US - NY - 8W1RQ32;Initial Catalog = Version_test;Integrated Security = True;" };
            bool result = false;
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => { result = true; })
    .WithNotParsed<Options>((errs) => { result = false; });
            Assert.Equal(FailurePaser, result);
        }
    }
}
