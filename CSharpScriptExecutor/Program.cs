using System;
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
    //// TODO: Split UI application into GUI and Console applications

    internal static class Program
    {
        public static readonly string ProgramName = GetSoleAssemblyAttribute<AssemblyProductAttribute>().Product;

        public static readonly string ProgramVersion =
            GetSoleAssemblyAttribute<AssemblyFileVersionAttribute>().Version;

        public static readonly string ProgramCopyright =
            GetSoleAssemblyAttribute<AssemblyCopyrightAttribute>().Copyright;

        public static readonly string FullProgramName = $@"{ProgramName} {ProgramVersion}. {ProgramCopyright}";

        public static readonly string UserAppDataPath = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData,
            Environment.SpecialFolderOption.None);

        public static readonly string ProgramDataPath = Path.Combine(UserAppDataPath, ProgramName);

        private const string ParameterPrefix = "/";
        private const string ParameterPrefixAlt = "-";
        private const string ParameterStopper = "--";

        private const string DebugParameter = "Debug";
        private const string PauseParameter = "Pause";
        private const string GuiParameter = "GUI";

        private static readonly string UnexpectedExceptionCaption = $@"Unexpected Exception — {ProgramName}";

        private static string[] _switches;
        private static bool _makePause;
        private static bool _isDebugMode;

        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        internal static int Main(string[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            if (arguments.Any(item => item == null))
            {
                throw new ArgumentException(@"The collection contains a null element.", nameof(arguments));
            }

            _switches = arguments
                .TakeWhile(
                    item =>
                        (item.StartsWith(ParameterPrefix, StringComparison.Ordinal)
                            || item.StartsWith(ParameterPrefixAlt, StringComparison.Ordinal))
                            && item != ParameterStopper)
                .Select(item => item.Substring(ParameterPrefix.Length))
                .ToArray();

            var actualArguments = arguments
                .SkipWhile((item, i) => i < _switches.Length || item == ParameterStopper)
                .ToArray();

            Func<string, string, bool> switchEqual =
                (@switch, parameter) => StringComparer.OrdinalIgnoreCase.Equals(@switch, parameter);

            var guiMode = false;
            foreach (var @switch in _switches)
            {
                if (switchEqual(@switch, GuiParameter))
                {
                    guiMode = true;
                    continue;
                }

                if (switchEqual(@switch, DebugParameter))
                {
                    _isDebugMode = true;
                    continue;
                }

                if (switchEqual(@switch, PauseParameter))
                {
                    _makePause = true;
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

                    return RunInGuiMode();
                }

                if (!InitializeConsole())
                {
                    return 255;
                }
            }
            catch (Exception ex)
            {
                ShowGuiError(ex.ToString(), ProgramName);
                return 255;
            }

            try
            {
                var consoleModeResult = RunInConsoleMode(actualArguments);
                AutoWaitForKey();
                return consoleModeResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(@"*** ERROR ***");
                Console.WriteLine(ex.ToString());
                Console.WriteLine();

                AutoWaitForKey();
                return 255;
            }
        }

        private static TAttribute GetSoleAssemblyAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            return assembly.GetSoleAttribute<TAttribute>();
        }

        private static void ShowHelp(bool isConsoleMode)
        {
            var sb = new StringBuilder()
                .AppendLine(FullProgramName)
                .AppendLine("Usage:")
                .AppendFormat(
                    "  {0} [{1}{2} | {1}{3}] [{1}{4}] <Script> [ScriptParameters...]",
                    ProgramName,
                    ParameterPrefix,
                    DebugParameter,
                    GuiParameter,
                    PauseParameter)
                .AppendLine();

            if (isConsoleMode)
            {
                Console.WriteLine();
                Console.WriteLine(sb.ToString());
                Console.WriteLine();
            }
            else
            {
                MessageBox.Show(sb.ToString(), ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static void ShowGuiError(string text, string caption)
            => MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        private static bool InitializeConsole()
        {
            ////if (!WinApi.AttachConsole(WinApi.AttachParentProcess))
            ////{
            ////    if (!WinApi.AllocConsole())
            ////    {
            ////        var lastError = Marshal.GetLastWin32Error();
            ////        string errorMessage = string.Format(
            ////            "Unable to switch to console mode: {0}",
            ////            new Win32Exception(lastError).Message);

            ////        ShowGuiError(errorMessage, ProgramName);
            ////        return false;
            ////    }
            ////}

            Console.CancelKeyPress += Console_CancelKeyPress;

            return true;
        }

        private static bool InitializeGui()
        {
            WinApi.AttachConsole(WinApi.AttachParentProcess);

            if (!WinApi.FreeConsole())
            {
                var error = Marshal.GetLastWin32Error();
                var errorMessage = $@"Unable to switch to GUI mode: {new Win32Exception(error).Message}";

                try
                {
                    Console.WriteLine(@"{0}: {1}", ProgramName, errorMessage);
                }
                catch (Exception)
                {
                    // Nothing to do
                }

                ShowGuiError(errorMessage, ProgramName);
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

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
            => MessageBox.Show(
                e.Exception.ToString(),
                UnexpectedExceptionCaption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

        private static void AutoWaitForKey()
        {
            if (!Debugger.IsAttached && !_makePause)
            {
                return;
            }

            Console.WriteLine(@"* Press any key to exit...");
            while (!Console.KeyAvailable)
            {
                // Nothing to do
            }

            Console.ReadKey(true);
        }

        private static int RunInGuiMode()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;

            //// TODO: Use pre-load of AvalonEdit's editor in background thread to speed up first load on user request

            //// Preloading causes InvalidOperationException with the message:
            //// 'The calling thread cannot access this object because a different thread owns it.' from time to time.
            //// It seems that ElementHost is the reason for that.

            ////            Thread preloadThread = new Thread(
            ////                () =>
            ////                {
            ////#pragma warning disable 618
            ////Debugger.Log(0, "", string.Format("Preload thread ID: {0}\n", AppDomain.GetCurrentThreadId()));
            ////#pragma warning restore 618

            ////                    using (var form = new ScriptForm())
            ////                    {
            ////                        // Nothing to do
            ////                    }
            ////                })
            ////            {
            ////                Name = "Preload thread",
            ////                IsBackground = true,
            ////                Priority = ThreadPriority.Lowest
            ////            };

            ////#pragma warning disable 618
            ////            Debugger.Log(0, "", string.Format("Main thread ID: {0}\n", AppDomain.GetCurrentThreadId()));
            ////#pragma warning restore 618

            ////            preloadThread.SetApartmentState(ApartmentState.STA);
            ////            preloadThread.Start();

            Application.Run(new MainForm());
            return 0;
        }

        private static int RunInConsoleMode(string[] arguments)
        {
            if (arguments.Length <= 0)
            {
                ////Console.WriteLine();
                ShowHelp(true);
                return 1;
            }

            Console.WriteLine();

            var scriptFilePath = Path.GetFullPath(arguments[0]);
            var scriptArguments = arguments.Skip(1).ToArray();
            var script = File.ReadAllText(scriptFilePath);
            var executorParameters = new ScriptExecutorParameters(script, scriptArguments, _isDebugMode);

            using (var scriptExecutor = ScriptExecutor.Create(executorParameters))
            {
                ScriptExecutionResult executionResult;
                try
                {
                    executionResult = scriptExecutor.Execute();
                }
                catch (Exception ex)
                {
                    executionResult = ScriptExecutionResult.CreateInternalError(ex, script);
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
                        Console.WriteLine(@"* Error processing script file ""{0}"":", scriptFilePath);
                        Console.WriteLine(@"* Error type: {0}", executionResult.Type);
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
                            Console.WriteLine(@" * Source code <START");
                            Console.WriteLine(executionResult.SourceCode);
                            Console.WriteLine(@" * Source code END>");
                        }

                        if (!string.IsNullOrEmpty(executionResult.GeneratedCode))
                        {
                            Console.WriteLine();
                            Console.WriteLine(@" * Generated code <START");
                            Console.WriteLine(executionResult.GeneratedCode);
                            Console.WriteLine(@" * Generated code END>");
                        }

                        Console.WriteLine();

                        return 200;
                    }

                    case ScriptExecutionResultType.Success:

                        // Nothing to do
                        break;

                    default:
                        throw new NotImplementedException(
                            $@"Not implemented script execution result type ({executionResult.Type}).");
                }
            }

            return 0;
        }
    }
}