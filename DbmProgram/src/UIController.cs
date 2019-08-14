using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using Chromely.Core.RestfulService;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace DBMProgram.src
{
    public static class NotepadHelper
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        private static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        public static void ShowMessage(string message = null, string title = null)
        {
            Process notepad = Process.Start(new ProcessStartInfo("notepad.exe"));
            if (notepad != null)
            {
                notepad.WaitForInputIdle();
                if (!string.IsNullOrEmpty(title))
                    SetWindowText(notepad.MainWindowHandle, title);
                if (!string.IsNullOrEmpty(message))
                {
                    IntPtr child = FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), "Edit", null);
                    SendMessage(child, 0x000C, 0, message);
                }
            }
        }
    }


    [ControllerProperty(Name = "UIController", Route = "uicontroller")]
    public class UIController : ChromelyController
    {
        public UIController()
        {
            RegisterGetRequest("/uicontroller/getUnexecuted", GetUnexecutedScripts);
            RegisterGetRequest("/uicontroller/getAll", GetAll);
            RegisterPostRequest("/uicontroller/run", runScripts);
            RegisterPostRequest("/uicontroller/openFile", OpenFile);
        }

        private ChromelyResponse runScripts(ChromelyRequest request)
        {
            var filePath = request.PostData.ToString();
            Console.WriteLine(filePath);
            UnexecutedScript script = new UnexecutedScript(filePath);
            Options opts = ConfigService.getConfig().opts;
            var scriptExecutor = ConfigService.getConfig().scriptExecutor;
            script.LoadScript();
            var result = scriptExecutor.RunSignleScriptBatchs(script, opts.ConnString, opts.SubsituteList);
            var response = new ChromelyResponse(request.Id);
            response.Data = result;

            return response;
        }

        private ChromelyResponse OpenFile(ChromelyRequest request)
        {
            var filePath = request.PostData.ToString();
            string text = File.ReadAllText(filePath);
            string name = System.IO.Path.GetFileNameWithoutExtension(filePath);
            NotepadHelper.ShowMessage(text, $"Copy - {name}");
            var response = new ChromelyResponse(request.Id);
            response.Data = null;
            return response;
        }
        private ChromelyResponse GetAll(ChromelyRequest request)
        {
            Options opts = ConfigService.getConfig().opts;
            var scriptExecutor = ConfigService.getConfig().scriptExecutor;
            var allScriptCount = scriptExecutor.GetAllScripts(opts.RootPath).ToList().Count;
            return new ChromelyResponse(request.Id)
            {
                Data = allScriptCount
            };

        }
        private ChromelyResponse GetUnexecutedScripts(ChromelyRequest request)
        {
            Options opts = ConfigService.getConfig().opts;
            var scriptExecutor = ConfigService.getConfig().scriptExecutor;
            var scriptInfos = (List<UnexecutedScript>)scriptExecutor.GetUnexecutedScripts(opts.RootPath, opts.ConnString);
            return new ChromelyResponse(request.Id)
            {
                Data = scriptInfos
            };
        }


    }
}
