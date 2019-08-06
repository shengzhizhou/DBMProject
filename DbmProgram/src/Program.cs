using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CommandLine;
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

        public void ExitFailureProgram(string exitMessage, int exitCode)
        {
            message.WriteError(exitMessage);
            Environment.Exit(exitCode);
        }

        private void CreateSnapshotDatabase(string connString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connString);
            string databaseName = builder.InitialCatalog;
            string snapshotName = $"{databaseName}_Snapshot";

            using (SqlConnection sqlCon = new SqlConnection(@connString))
            using (var command = new SqlCommand())
            {
                command.Connection = sqlCon;
                command.CommandText = "SELECT name,physical_name FROM sys.database_files WHERE type_desc<>'LOG'";
                sqlCon.Open();
                string databaseInfos = "";
                List<string> nameList = new List<string>();
                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        string DataDirectory = Path.GetDirectoryName(dataReader.GetValue(1).ToString());
                        string dataFileName = dataReader.GetValue(0).ToString();
                        string dataFilePath = $"{DataDirectory}\\{databaseName}_Snapshot.ss";
                        databaseInfos += $"(NAME={dataFileName},FILENAME='{dataFilePath}'),";
                        
                    }
                }
                databaseInfos = databaseInfos.Remove(databaseInfos.Length - 1, 1);
                command.CommandText = $"IF EXISTS (SELECT name FROM sys.databases WHERE name = '{snapshotName}' ) DROP DATABASE {snapshotName};" +
                    $"CREATE DATABASE {snapshotName} ON  " +
                    $"{databaseInfos}  AS SNAPSHOT OF {databaseName};";
                command.ExecuteNonQuery();
            }
            message.WriteMessage($"\n{snapshotName} Database: Created");
        }

        private void RunOnlyDdlOrDmlScript(Options opts)
        {
            message.WriteMessage(opts.RootPath);
            List<UnexecutedScript> unexecutedScript = (List<UnexecutedScript>)scriptExecutor.GetUnexecutedScripts(opts.RootPath, opts.ConnString);
            foreach (ScriptExecutionResult result in scriptExecutor.RunBatches(unexecutedScript, opts.ConnString, opts.SubsituteList))
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
            if (!(opts.IsValidConn() && opts.IsValidPath() && opts.IsValidSubsituteList()))
            {
                ExitFailureProgram($"Overall Status: failure\n{(opts.IsValidConn() ? null : "<ConnString> or <Dbname>")} {(opts.IsValidPath() ? null : "<RootPath>")} {(opts.IsValidSubsituteList() ? null : "<SubstituteList>")} Argument is not valid", 0);
            }
            if (opts.IsSnapshot)
                CreateSnapshotDatabase(opts.ConnString);

            //assert ddl and dml exist
            message.WriteMessage("\nAutomation Begin:\n");
            opts.RootPath = rootPath + "\\ddl";
            RunOnlyDdlOrDmlScript(opts);
            opts.RootPath = rootPath + "\\dml";
            RunOnlyDdlOrDmlScript(opts);
            ExitSuccessProgram("\nOverall Status: Success", 0);
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

        [Option('s', "snapshot", Required = false, HelpText = "Do you want to recover your data?")]
        public bool IsSnapshot { get; set; }

        [Option('v', "sub", Required = false, HelpText = "Subtitute Variable")]
        public IEnumerable<string> SubsituteList { get; set; }

        public bool IsValidPath()
        {
            string[] subdirectoryEntries = Directory.GetDirectories(RootPath);
            if (subdirectoryEntries.Length == 2 && subdirectoryEntries.Contains(RootPath + "\\ddl") && subdirectoryEntries.Contains(RootPath + "\\dml"))
            {
                return true;
            }
            return false;
        }

        public bool IsValidSubsituteList()
        {
            try
            {
                foreach (string var in SubsituteList)
                {
                    string[] pairedVariable = var.Split(":");
                    if (pairedVariable.Length != 2)
                        return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool IsValidConn()
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnString);
                if (DbName != null)
                {
                    builder.InitialCatalog = DbName;
                    ConnString = builder.ConnectionString;
                }
                using (SqlConnection sqlCon = new SqlConnection(@ConnString))
                {
                    sqlCon.Open();
                    string command = @"IF EXISTS( SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @table) SELECT 1 ELSE SELECT 0";
                    using (SqlCommand sqlCommand = new SqlCommand(command, sqlCon))
                    {
                        sqlCommand.Parameters.AddWithValue("@table", "version");
                        if ((int)sqlCommand.ExecuteScalar() != 1)
                            return false;
                        return true;
                    }
                }

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
            UnityContainer ScriptContainer = Factory.ConfigureContainer();
            ScriptController scriptController = ScriptContainer.Resolve<ScriptController>();
            scriptController.ExitFailureProgram("\nOverall Status: Unsuccess", 0);
        }
        [STAThread]
        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
    .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }
    }
}
