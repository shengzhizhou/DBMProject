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

        public void ExitSuccessProgram(string exitMessage, int exitCode)
        {
            message.WriteMessage(exitMessage);
            Environment.Exit(exitCode);
        }

        private void ExitFailureProgram(string exitMessage, int exitCode)
        {
            message.WriteError(exitMessage);
            Environment.Exit(exitCode);
        }

        private void RunOnlyDdlOrDmlScript(Options opts)
        {
            message.WriteMessage(opts.RootPath);
            List<UnexecutedScript> unexecutedScript = (List<UnexecutedScript>)scriptExecutor.GetUnexecutedScripts(opts.RootPath, opts.ConnString);
            foreach (ScriptExecutionResult result in scriptExecutor.RunBatches(unexecutedScript, opts.ConnString))
            {
                message.WriteMessage(result.ToString());
                if (!result.IsSuccess)
                {
                    ExitFailureProgram($"Overall Status: failure\n{result.errorMessage}\n", 0);
                }
            }
        }

        public void RunScript(Options opts)
        {
            string rootPath = opts.RootPath;
            // validaten parameters
            if (!(opts.IsValidConn() && opts.IsValidPath()))
            {
                ExitFailureProgram($"Overall Status: failure\n{(opts.IsValidConn() ? "RootPath" : "Connection String")} Argument is not valid", 0);
            }
            // assert ddl and dml exist     
            opts.RootPath = rootPath + "\\ddl";
            RunOnlyDdlOrDmlScript(opts);
            opts.RootPath = rootPath + "\\dml";
            RunOnlyDdlOrDmlScript(opts);
            ExitSuccessProgram("\nOverall Status: success", 0);
        }
    }

    public class Options
    {
        [Option('r', "root", Required = true, HelpText = "Input file to read.")]
        public string RootPath { get; set; }

        [Option('c', "conn", Required = true, HelpText = "Connection String to SQL Server")]
        public string ConnString { get; set; }

        [Option('d', "dbname", Required = false, HelpText = "Specific name of Database that contains Version table")]
        public string DbName { get; set; }

        public bool IsValidPath()
        {
            string[] subdirectoryEntries = Directory.GetDirectories(RootPath);
            if (subdirectoryEntries.Length == 2 && subdirectoryEntries.Contains(RootPath + "\\ddl") && subdirectoryEntries.Contains(RootPath + "\\dml"))
            {
                return true;
            }
            return false;
        }
        public bool IsValidConn()
        {
            try
            {
                if (DbName != null)
                {
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnString);
                    builder.InitialCatalog = DbName;
                    ConnString = builder.ConnectionString;
                }
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
