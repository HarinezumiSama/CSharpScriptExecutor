using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpScriptExecutor.Common;

namespace CSharpScriptExecutor
{
    internal static class Program
    {
        #region Fields

        private static readonly string s_programName = GetSoleAssemblyAttribute<AssemblyProductAttribute>().Product;
        private static readonly string s_programVersion =
            GetSoleAssemblyAttribute<AssemblyFileVersionAttribute>().Version;
        private static readonly string s_programCopyright =
            GetSoleAssemblyAttribute<AssemblyCopyrightAttribute>().Copyright;

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

        private static void ShowHelp()
        {
            Console.WriteLine("{0} {1}. {2}", s_programName, s_programVersion, s_programCopyright);
            Console.WriteLine("Usage:");
            Console.WriteLine("  {0} <Script> [ScriptParameters...]", s_programName);
            Console.WriteLine();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                e.Cancel = true;
            }
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

        [LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static int Main(string[] arguments)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            Console.WriteLine();

            try
            {
                if ((arguments == null) || (arguments.Length <= 0))
                {
                    ShowHelp();
                    return 1;
                }

                string scriptFilePath = Path.GetFullPath(arguments[0]);
                string[] actualArguments = new string[arguments.Length - 1];
                Array.Copy(arguments, 1, actualArguments, 0, actualArguments.Length);

                ScriptExecutionResult executionResult;
                using (IScriptExecutor scriptExecutor = ScriptExecutorProxy.Create(scriptFilePath, actualArguments))
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
                        throw new NotImplementedException(string.Format(
                            "Not implemented script execution result type ({0}).",
                            executionResult.Type.ToString()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("*** INTERNAL ERROR ***");
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