using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using CommandLine;
using System.Collections;
using System.Data.SqlClient;
using Unity;

namespace DBMProgram.src
{

    public class ScriptController
    {

        private readonly IMessageWriter message;
        private readonly IScriptExecutor scriptExecutor;
        public ScriptController(IMessageWriter message, IScriptExecutor scriptExecutor)
        {
            this.message = message;
            this.scriptExecutor = scriptExecutor;
        }

        public void RunScript(Options opts)
        {
            // validaten parameters
            if (!(opts.IsValidConn() && opts.IsValidPath()))
                throw new ArgumentException("{0} Argument is not valid", opts.IsValidConn()?"Connection String":"RootPath");

            // assert ddl and dml exist        
            string[] dirs = Directory.GetDirectories(@opts.RootPath, "*", SearchOption.TopDirectoryOnly);
            foreach (string subDir in dirs)
            {
                opts.RootPath = subDir;
                List<UnexecutedScript> unexecutedScript = (List<UnexecutedScript>)scriptExecutor.GetUnexecutedScripts(opts);
                foreach (ScriptExecutionResult result in scriptExecutor.RunBatches(unexecutedScript, opts.ConnString))
                {
                    message.WriteMessage(result.ToString());
                    if (!result.IsSuccess)
                    {
                        message.WriteError($"Overall Status: failure\n{result.errorMessage}\n");
                        Environment.Exit(0);
                    }
                }
            }
            message.WriteMessage("\nOverall Status: success");
            Environment.Exit(0);
        }
    }

    public class Options
    {
        [Option('r', "root", Required = true, HelpText = "Input file to read.")]
        public string RootPath { get; set; }

        [Option('c', "conn", Required = true, HelpText = "Connection String to SQL Server")]
        public string ConnString { get; set; }
        public bool IsValidPath()
        {
            string[] subdirectoryEntries = Directory.GetDirectories(RootPath);
            if (subdirectoryEntries.Length == 2 && subdirectoryEntries.Contains(RootPath + "\\DDL") && subdirectoryEntries.Contains(RootPath + "\\DML"))
            {
                return true;
            }
            return false;
        }
        public bool IsValidConn()
        {
            try
            {
                SqlConnection sqlCon = new SqlConnection(ConnString);
                sqlCon.Open();
                sqlCon.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    public class Factory
    {
        public static UnityContainer ConfigureContainer()
        {
            var container = new UnityContainer();
            container.RegisterType<IMessageWriter, ConsoleMessageWriter>();
            container.RegisterType<IScriptExecutor, SqlServerScriptExecutor>();
            return container;
        }
    }

    class Program
    {
        private static void RunOptionsAndReturnExitCode(Options opts)
        {
            UnityContainer ScriptContainer = Factory.ConfigureContainer();
            ScriptController scriptController = ScriptContainer.Resolve<ScriptController>();
            scriptController.RunScript(opts);
        }
        private static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.Error.WriteLine("\nOverall Status: unsuccess");
        }
        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
    .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }


    }
}
