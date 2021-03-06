﻿
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace DBMProgram.src
{
    public class ScriptExecutionResult
    {
        public string scriptName;
        public bool IsSuccess;
        public int rowsEffected;
        public string errorMessage;
        public ScriptExecutionResult(string scriptName)
        {
            this.scriptName = scriptName;
        }
        public override string ToString()
        {
            return $"{scriptName} : {(IsSuccess ? "Success" : "Failure")}\nRowsEffected : {rowsEffected}\n";
        }
    }
    public interface IScriptExecutor
    {
        void AddScriptRecords(UnexecutedScript scripts, string ConnString);
        IEnumerable<ScriptExecutionResult> RunBatches(List<UnexecutedScript> unexecutedScripts, string ConnString, IEnumerable<string> substituteList);
        IEnumerable<UnexecutedScript> GetUnexecutedScripts(string rootPath, string connString);
        IEnumerable<UnexecutedScript> GetAllScripts(string rootPath);
        IEnumerable<string> GetExecutedScriptNames(string connString);
        ScriptExecutionResult RunSignleScriptBatchs(UnexecutedScript script, string ConnString, IEnumerable<string> substituteList);
    }

    public class SqlServerScriptExecutor : IScriptExecutor
    {
        private readonly IMessageWriter message;
        public SqlServerScriptExecutor(IMessageWriter message)
        {
            this.message = message;
        }
        //Add unexecuted script file name to Version table
        public void AddScriptRecords(UnexecutedScript scripts, string ConnString)
        {
            using (SqlConnection sqlCon = new SqlConnection(@ConnString))
            {
                sqlCon.Open();
                string sql = $"INSERT INTO [dbo].[versions]" +
                    $"VALUES (@scriptName,@AppliedDate)";
                using (SqlCommand command = new SqlCommand(sql, sqlCon))
                {
                    command.Parameters.AddWithValue("@scriptName", scripts.ScriptName);
                    SqlParameter parameter = command.Parameters.AddWithValue("@AppliedDate", SqlDbType.DateTime);
                    parameter.Value = DateTime.Now;
                    command.ExecuteNonQuery();
                }
            }
        }


        public ScriptExecutionResult RunSignleScriptBatchs(UnexecutedScript script, string ConnString, IEnumerable<string> substituteList)
        {
            var executionResult = new ScriptExecutionResult(script.ScriptName);
            try
            {
                using (var sqlCon = new SqlConnection(@ConnString))
                {
                    sqlCon.Open();
                    int rows = 0;
                    foreach (string batch in script.Batches)
                    {
                        if (!string.IsNullOrEmpty(batch))
                        {
                            string replacedBatch = batch;
                            //Substitute Variable
                            foreach (string sub in substituteList)
                            {
                                string[] var = sub.Split(':');
                                replacedBatch = replacedBatch.Replace($"${var[0]}$", var[1]);
                                }

                            using (var command = new SqlCommand(replacedBatch, sqlCon))
                            {
                                rows += command.ExecuteNonQuery() < 0 ? 0 : command.ExecuteNonQuery();
                            }
                        }
                    }
                    AddScriptRecords(script, ConnString);
                    executionResult.IsSuccess = true;
                    executionResult.rowsEffected = rows;
                }
                return executionResult;

            }
            catch (Exception ex)
            {
                executionResult.IsSuccess = false;
                executionResult.errorMessage = ex.ToString();
                return executionResult;
            }

        }

        //run batches of each unexecuted script file
        public IEnumerable<ScriptExecutionResult> RunBatches(List<UnexecutedScript> unexecutedScripts, string ConnString, IEnumerable<string> substituteList)
        {
            foreach (UnexecutedScript script in unexecutedScripts)
            {
                var executionResult = RunSignleScriptBatchs(script, ConnString, substituteList);
                yield return executionResult;
            }

        }

        //get all local unexecuted script file
        public IEnumerable<UnexecutedScript> GetUnexecutedScripts(string rootPath, string connString)
        {
            List<UnexecutedScript> unexecutedScript = new List<UnexecutedScript>();
            List<string> AppliedScript = (List<string>)GetExecutedScriptNames(connString);
            string output = "";
            foreach (UnexecutedScript script in GetAllScripts(rootPath))
            {
                if (!AppliedScript.Contains(script.ScriptName))
                {
                    script.LoadScript();

                    if (!script.IsSkip())
                    {
                        
                        unexecutedScript.Add(script);
                        
                    }
                    output = output + "  " + script.ScriptName;
                }
                script.IsExecuted = true;
            }
            unexecutedScript.Sort();
            message.WriteMessage($"Unexecuted Script: {output}\n");
            return unexecutedScript;
        }

        //get all local script file
        public IEnumerable<UnexecutedScript> GetAllScripts(string rootPath)
        {
            return Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories)
                .Select(f => new UnexecutedScript(f));

        }

        //retrieve script table from sql server
        public IEnumerable<string> GetExecutedScriptNames(String connString)
        {
            string sql = $"select * from versions";
            using (var sqlCon = new SqlConnection(@connString))
            {
                using (var command = new SqlCommand(sql, sqlCon))
                {
                    sqlCon.Open();
                    string output = "";
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        List<string> executedScript = new List<string>();
                        while (dataReader.Read())
                        {
                            int nameOrdinal = dataReader.GetOrdinal("script_name");
                            executedScript.Add(dataReader.GetValue(nameOrdinal).ToString());
                            output = output + "  " + dataReader.GetValue(nameOrdinal);
                        }
                        return executedScript;
                    }
                }
            }

        }

    }
}
