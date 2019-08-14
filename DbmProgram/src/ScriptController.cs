using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Chromely.Core.RestfulService;


namespace DBMProgram.src
{
    
    public class ScriptController : ChromelyController
    {
        private readonly IMessageWriter message;
        public readonly IScriptExecutor scriptExecutor;
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

        public void CreateSnapshotDatabase(string connString)
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
                        string DataDirectory = System.IO.Path.GetDirectoryName(dataReader.GetValue(1).ToString());
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


        public void RunScript(Options opts)
        {
            message.WriteMessage("\nAutomation Begin:\n");
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
            ExitSuccessProgram("\nOverall Status: Success", 0);
        }

        }
    }

