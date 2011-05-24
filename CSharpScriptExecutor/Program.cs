using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CSharpScriptExecutor.Common;

namespace CSharpScriptExecutor
{
    internal static class Program
    {
        #region Constants

        private const string c_parameterPrefix = "/";
        private const string c_parameterPrefixAlt = "-";
        private const string c_parameterStopper = "--";

        private const string c_debugParameter = "Debug";
        private const string c_pauseParameter = "Pause";
        private const string c_guiParameter = "GUI";

        #endregion

        #region Fields

        private static readonly string s_programName = GetSoleAssemblyAttribute<AssemblyProductAttribute>().Product;
        private static readonly string s_programVersion =
            GetSoleAssemblyAttribute<AssemblyFileVersionAttribute>().Version;
        private static readonly string s_programCopyright =
            GetSoleAssemblyAttribute<AssemblyCopyrightAttribute>().Copyright;

        private static readonly string s_fullProgramName = string.Format(
            "{0} {1}. {2}",
            s_programName,
            s_programVersion,
            s_programCopyright);

        private static string[] s_switches;
        private static bool s_makePause;
        private static bool s_isDebugMode;

        #endregion

        #region Private Methods

        private static TAttribute GetSoleAssemblyAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            TAttribute[] attributes = (TAttribute[])assembly.GetCustomAttributes(typeof(TAttribute), true);
            if ((attributes == null) || (attributes.Length != 1))
            {
                throw new InvalidProgramException(string.Format(
                    "Invalid definition of '{0}' attribute.",
                    typeof(TAttribute).Name));
            }

            return attributes[0];
        }

        private static void ShowHelp(bool isConsoleMode)
        {
            StringBuilder sb = new StringBuilder()
                .AppendLine(s_fullProgramName)
                .AppendLine("Usage:")
                .AppendFormat(
                    "  {0} [{1}{2} | {1}{3}] [{1}{4}] <Script> [ScriptParameters...]",
                    s_programName,
                    c_parameterPrefix,
                    c_debugParameter,
                    c_guiParameter,
                    c_pauseParameter)
                .AppendLine();

            if (isConsoleMode)
            {
                Console.WriteLine();
                Console.WriteLine(sb.ToString());
                Console.WriteLine();
            }
            else
            {
                MessageBox.Show(sb.ToString(), s_programName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static void ShowGuiError(string text, string caption)
        {
            MessageBox.Show(
                text,
                caption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }

        private static bool InitializeConsole()
        {
            //if (!WinApi.AttachConsole(WinApi.ATTACH_PARENT_PROCESS))
            //{
            //    if (!WinApi.AllocConsole())
            //    {
            //        var lastError = Marshal.GetLastWin32Error();
            //        string errorMessage = string.Format(
            //            "Unable to switch to console mode: {0}",
            //            new Win32Exception(lastError).Message);

            //        ShowGuiError(errorMessage, s_programName);
            //        return false;
            //    }
            //}

            Console.CancelKeyPress += Console_CancelKeyPress;

            return true;
        }

        private static bool InitializeGui()
        {
            WinApi.AttachConsole(WinApi.ATTACH_PARENT_PROCESS);

            if (!WinApi.FreeConsole())
            {
                var error = Marshal.GetLastWin32Error();
                string errorMessage = string.Format(
                    "Unable to switch to GUI mode: {0}",
                    new Win32Exception(error).Message);

                try
                {
                    Console.WriteLine("{0}: {1}", s_programName, errorMessage);
                }
                catch (Exception)
                {
                    // Nothing to do
                }

                ShowGuiError(errorMessage, s_programName);
                return false;
            }

            return true;
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                e.Cancel = true;
            }
        }

        private static void AutoWaitForKey()
        {
            if (Debugger.IsAttached || s_makePause)
            {
                Console.WriteLine("* Press any key to exit...");
                while (!Console.KeyAvailable)
                {
                    // Nothing to do
                }
                Console.ReadKey(true);
            }
        }

        private static int RunInGuiMode(string[] arguments)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // TODO: Use pre-load of AvalonEdit's editor in background thread to speed up first load on user request

            // Preloading causes InvalidOperationException with the message:
            // 'The calling thread cannot access this object because a different thread owns it.' from time to time.
            // It seems that ElementHost is the reason for that.

            #region Preloading form and its components in background since AvalonEdit is a heavy component

            //            Thread preloadThread = new Thread(
            //                () =>
            //                {
            //#pragma warning disable 618
            //Debugger.Log(0, "", string.Format("Preload thread ID: {0}\n", AppDomain.GetCurrentThreadId()));
            //#pragma warning restore 618

            //                    using (var form = new ScriptForm())
            //                    {
            //                        // Nothing to do
            //                    }
            //                })
            //            {
            //                Name = "Preload thread",
            //                IsBackground = true,
            //                Priority = ThreadPriority.Lowest
            //            };

            //#pragma warning disable 618
            //            Debugger.Log(0, "", string.Format("Main thread ID: {0}\n", AppDomain.GetCurrentThreadId()));
            //#pragma warning restore 618

            //            preloadThread.SetApartmentState(ApartmentState.STA);
            //            preloadThread.Start();

            #endregion

            Application.Run(new MainForm());
            return 0;
        }

        private static int RunInConsoleMode(string[] arguments)
        {
            if (arguments.Length <= 0)
            {
                //Console.WriteLine();
                ShowHelp(true);
                return 1;
            }

            Console.WriteLine();

            string scriptFilePath = Path.GetFullPath(arguments[0]);
            var scriptArguments = arguments.Skip(1).ToArray();
            var scriptSource = File.ReadAllText(scriptFilePath);
            var executorParameters = new ScriptExecutorParameters(scriptSource, scriptArguments, s_isDebugMode);

            using (var scriptExecutor = ScriptExecutor.Create(executorParameters))
            {
                ScriptExecutionResult executionResult;
                try
                {
                    executionResult = scriptExecutor.Execute();
                }
                catch (Exception ex)
                {
                    executionResult = ScriptExecutionResult.CreateInternalError(
                        ex,
                        string.Empty,
                        string.Empty,
                        scriptSource,
                        null);
                }

                Console.WriteLine(executionResult.ConsoleOut);
                Console.WriteLine(executionResult.ConsoleError);

                switch (executionResult.Type)
                {
                    case ScriptExecutionResultType.InternalError:
                    case ScriptExecutionResultType.CompilationError:
                    case ScriptExecutionResultType.ExecutionError:
                        {
                            Console.WriteLine();
                            Console.WriteLine("* Error processing script file \"{0}\":", scriptFilePath);
                            Console.WriteLine("* Error type: {0}", executionResult.Type.ToString());
                            Console.WriteLine(executionResult.Message);
                            if (executionResult.Type == ScriptExecutionResultType.CompilationError)
                            {
                                foreach (var compilerError in executionResult.CompilerErrors)
                                {
                                    Console.WriteLine(compilerError.ToString());
                                }
                            }
                            if (!string.IsNullOrEmpty(executionResult.SourceCode))
                            {
                                Console.WriteLine();
                                Console.WriteLine(" * Source code <START");
                                Console.WriteLine(executionResult.SourceCode);
                                Console.WriteLine(" * Source code END>");
                            }
                            if (!string.IsNullOrEmpty(executionResult.GeneratedCode))
                            {
                                Console.WriteLine();
                                Console.WriteLine(" * Generated code <START");
                                Console.WriteLine(executionResult.GeneratedCode);
                                Console.WriteLine(" * Generated code END>");
                            }
                            Console.WriteLine();

                            return 200;
                        }

                    case ScriptExecutionResultType.Success:
                        // Nothing to do
                        break;

                    default:
                        throw new NotImplementedException(
                            string.Format(
                                "Not implemented script execution result type ({0}).",
                                executionResult.Type.ToString()));
                }
            }

            return 0;
        }

        #endregion

        #region Internal Properties

        internal static string ProgramName
        {
            [DebuggerStepThrough]
            get { return s_programName; }
        }

        internal static string FullProgramName
        {
            [DebuggerStepThrough]
            get { return s_fullProgramName; }
        }

        #endregion

        #region Public Methods

        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        internal static int Main(string[] arguments)
        {
            #region Argument Check

            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            if (arguments.Contains(null))
            {
                throw new ArgumentException("The collection contains a null element.", "arguments");
            }

            #endregion

            s_switches = arguments
                .TakeWhile(
                    item => (item.StartsWith(c_parameterPrefix) || item.StartsWith(c_parameterPrefixAlt))
                        && item != c_parameterStopper)
                .Select(item => item.Substring(c_parameterPrefix.Length))
                .ToArray();
            var actualArguments = arguments
                .SkipWhile((item, i) => i < s_switches.Length || item == c_parameterStopper)
                .ToArray();

            Func<string, string, bool> switchEqual =
                (@switch, parameter) => StringComparer.OrdinalIgnoreCase.Equals(@switch, parameter);

            var guiMode = false;
            foreach (var @switch in s_switches)
            {
                if (switchEqual(@switch, c_guiParameter))
                {
                    guiMode = true;
                    continue;
                }
                if (switchEqual(@switch, c_debugParameter))
                {
                    s_isDebugMode = true;
                    continue;
                }
                if (switchEqual(@switch, c_pauseParameter))
                {
                    s_makePause = true;
                    continue;
                }

                ShowHelp(false);
                return 1;
            }

            try
            {
                if (guiMode)
                {
                    if (!InitializeGui())
                    {
                        return 255;
                    }

                    return RunInGuiMode(actualArguments);
                }

                if (!InitializeConsole())
                {
                    return 255;
                }
            }
            catch (Exception ex)
            {
                ShowGuiError(ex.ToString(), s_programName);
                return 255;
            }

            try
            {
                var consoleModeResult = RunInConsoleMode(actualArguments);
                Program.AutoWaitForKey();
                return consoleModeResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("*** ERROR ***");
                Console.WriteLine(ex.ToString());
                Console.WriteLine();

                Program.AutoWaitForKey();
                return 255;
            }
        }

        #endregion
    }
}