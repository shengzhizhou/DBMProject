using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Chromely.Core;
using Chromely.Core.Helpers;
using Chromely.Core.Host;
using Chromely.Core.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CommandLine;
using System.Data.SqlClient;
using Unity;
using Chromely.CefGlue;
using Chromely.CefGlue.Winapi;
using Caliburn.Light;

namespace DBMProgram.src
{
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

        [Option('u', "ui", Required = false, HelpText = "Running Graphical User Interface")]
        public bool IsRunningUI { get; set; }
        public bool IsValidPath()
        {
            int fileNum = Directory.GetFiles(RootPath, "*", SearchOption.AllDirectories).Length;

            if (fileNum > 0)
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
                        sqlCommand.Parameters.AddWithValue("@table", "versions");
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
    public class ConfigService
    {
        private static ConfigService config;
        public Options opts;
        public IScriptExecutor scriptExecutor;
        public static ConfigService getConfig()
        {
            if (config == null) config = new ConfigService();
            return config;
        }
        public void SetUp(Options opts)
        {
            this.opts = opts;
            UnityContainer ScriptContainer = ContainerFactory.ConfigureContainer();
            this.scriptExecutor = ScriptContainer.Resolve<IScriptExecutor>();
        }

    }

    public class ContainerFactory
    {
        public static UnityContainer ConfigureContainer()
        {
            var container = new UnityContainer();
            container.RegisterType<IMessageWriter, ConsoleMessageWriter>();
            container.RegisterType<IScriptExecutor, SqlServerScriptExecutor>();
            return container;
        }
    }

    class ArgumentController
    {
        private Options opts;
        private ScriptController scriptController;
        public ArgumentController(Options opts, ScriptController scriptController)
        {
            this.scriptController = scriptController;
            this.opts = opts;
        }
        public void CheckArgsValidation()
        {
            if (!(opts.IsValidConn() && opts.IsValidPath() && opts.IsValidSubsituteList()))
            {
                scriptController.ExitFailureProgram($"Overall Status: failure\n{(opts.IsValidConn() ? null : "<ConnString> or <Dbname>")} {(opts.IsValidPath() ? null : "<RootPath>")} {(opts.IsValidSubsituteList() ? null : "<SubstituteList>")} Argument is not valid", 0);
            }
        }
        private void RunOnConsole()
        {
            scriptController.RunScript(opts);
        }
        private void ShowUI(string[] args)
        {
            try
            {
                HostHelpers.SetupDefaultExceptionHandlers();
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var startUrl = "file:///app/pages/messagerouterdemo.html";
                var config = ChromelyConfiguration
                                 .Create()
                                 .WithHostMode(WindowState.Normal)
                                 .WithHostTitle("DBMProject")
                                 .WithHostIconFile("chromely.ico")
                                 .WithHostBounds(1350, 834)
                                 .WithAppArgs(args)
                                 .UseDefaultResourceSchemeHandler("local", string.Empty)
                                 .WithLogSeverity(LogSeverity.Info)
                                .UseDefaultLogger("logs\\chromely_new.log")
                                .UseDefaultHttpSchemeHandler("http", "chromely.com")
                                  // .NET Core may not run in debug mode without publishing to exe
                                  // To overcome that, a subprocess need to be set.
                                  //.WithCustomSetting(CefSettingKeys.BrowserSubprocessPath, path_to_subprocess)
                                  .WithStartUrl(startUrl);

                using (var window = ChromelyWindow.Create(config))
                {
                    window.RegisterUrlScheme(new UrlScheme("https://github.com/chromelyapps/Chromely", true));
                    window.RegisterServiceAssembly(Assembly.GetExecutingAssembly());
                    window.ScanAssemblies();
                    window.Run(args);
                }
            }
            catch (Exception exception)
            {
                scriptController.ExitFailureProgram($"\nOverall Status: Unsuccess\n{exception}", 0);
            }
        }
        public void RunArgs(string[] args)
        {
            if (opts.IsSnapshot)
                scriptController.CreateSnapshotDatabase(opts.ConnString);
            if (!opts.IsRunningUI)
            {
                RunOnConsole();
            }
            else
            {
                ShowUI(args);
            }
        }
    }


    class Program
    {
        private static void RunOptionsAndReturnExitCode(Options opts, string[] args)
        {
            UnityContainer ScriptContainer = ContainerFactory.ConfigureContainer();
            ScriptContainer.RegisterInstance(opts);
            ScriptController scriptController = ScriptContainer.Resolve<ScriptController>();
            ConfigService configService = ConfigService.getConfig();
            configService.SetUp(opts);
            ArgumentController argumentController = new ArgumentController(opts, scriptController);
            argumentController.CheckArgsValidation();
            argumentController.RunArgs(args);
        }
        private static void HandleParseError(IEnumerable<Error> errs)
        {
            UnityContainer ScriptContainer = ContainerFactory.ConfigureContainer();
            ScriptController scriptController = ScriptContainer.Resolve<ScriptController>();
            scriptController.ExitFailureProgram("\nOverall Status: Unsuccess", 0);
        }
        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
        .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts, args))
        .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }
    }
}
