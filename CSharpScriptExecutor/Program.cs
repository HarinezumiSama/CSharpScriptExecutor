using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CSharpScriptExecutor.Common;

namespace CSharpScriptExecutor
{
    internal static class Program
    {
        #region Constants

        private const string c_debugParameter = "/Debug";
        private const string c_guiParameter = "/GUI";

        #endregion

        #region Fields

        private static readonly string s_programName = GetSoleAssemblyAttribute<AssemblyProductAttribute>().Product;
        private static readonly string s_programVersion =
            GetSoleAssemblyAttribute<AssemblyFileVersionAttribute>().Version;
        private static readonly string s_programCopyright =
            GetSoleAssemblyAttribute<AssemblyCopyrightAttribute>().Copyright;
        private static readonly string s_fullProgramName = string.Format(
            "{0} {1}",
            s_programName,
            s_programVersion);

        #endregion

        #region Private Methods

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeConsole();

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

        private static void ShowHelp()
        {
            Console.WriteLine("{0}. {1}", s_fullProgramName, s_programCopyright);
            Console.WriteLine("Usage:");
            Console.WriteLine("  {0} [/Debug | /GUI] <Script> [ScriptParameters...]", s_programName);
            Console.WriteLine();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                e.Cancel = true;
            }
        }

        private static int RunInGuiMode()
        {
            if (!FreeConsole())
            {
                MessageBox.Show(
                    "Unable to switch to GUI mode.",
                    s_fullProgramName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return 255;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
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

        #region Internal Methods

        internal static void PauseIfUnderDebugger()
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine("* Press any key to exit...");
                while (!Console.KeyAvailable)
                {
                    // Nothing to do
                }
                Console.ReadKey(true);
            }
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

            #endregion

            try
            {
                if (arguments.Length <= 0)
                {
                    Console.WriteLine();
                    ShowHelp();
                    return 1;
                }

                if (StringComparer.OrdinalIgnoreCase.Equals(arguments.First(), c_guiParameter))
                {
                    return RunInGuiMode();
                }

                Console.CancelKeyPress += Console_CancelKeyPress;
                Console.WriteLine();

                bool isDebugMode = false;

                int argumentOffset = 0;
                if (StringComparer.OrdinalIgnoreCase.Equals(arguments.First(), c_debugParameter))
                {
                    isDebugMode = true;
                    argumentOffset++;
                }
                string scriptFilePath = Path.GetFullPath(arguments[argumentOffset]);
                var actualArguments = arguments.Skip(argumentOffset + 1).ToArray();
                var script = File.ReadAllText(scriptFilePath);
                var executorParameters = new ScriptExecutorParameters(script, actualArguments, isDebugMode);

                ScriptExecutionResult executionResult;
                using (IScriptExecutor scriptExecutor = ScriptExecutorProxy.Create(executorParameters))
                {
                    executionResult = scriptExecutor.Execute();
                }

                switch (executionResult.Type)
                {
                    case ScriptExecutionResultType.InternalError:
                    case ScriptExecutionResultType.CompileError:
                    case ScriptExecutionResultType.ExecutionError:
                        {
                            Console.WriteLine();
                            Console.WriteLine("* Error processing script file \"{0}\":", scriptFilePath);
                            Console.WriteLine("* Error type: {0}", executionResult.Type.ToString());
                            Console.WriteLine(executionResult.Message);
                            Console.WriteLine();

                            Program.PauseIfUnderDebugger();
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
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("*** ERROR ***");
                Console.WriteLine(ex.ToString());
                Console.WriteLine();

                Program.PauseIfUnderDebugger();
                return 255;
            }
            finally
            {
                ScriptExecutorProxy.TempFiles.Delete();
            }

            Program.PauseIfUnderDebugger();
            return 0;
        }

        #endregion
    }
}