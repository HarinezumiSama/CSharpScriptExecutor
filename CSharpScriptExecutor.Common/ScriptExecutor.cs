﻿using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp;

namespace CSharpScriptExecutor.Common
{
    public sealed class ScriptExecutor : MarshalByRefObject, IScriptExecutor
    {
        #region Constants

        private const string c_directivePrefix = "##";
        private const string c_codeDirective = "Code";
        private const string c_classDirective = "Class";

        private const string c_predefinedTypeName = "ScriptExecutorWrapper";
        private const string c_predefinedMethodName = "Main";
        private const BindingFlags c_predefinedMethodBindingFlags = BindingFlags.Static | BindingFlags.Public |
            BindingFlags.NonPublic;
        private const string c_bracingStyle = "C";
        private const string c_indentString = "    ";

        private const ulong c_maxScriptFileSize = 8 * Constants.Megabyte;

        #endregion

        #region Fields

        #region Static

        private static readonly StringComparer s_directiveComparer = StringComparer.OrdinalIgnoreCase;
        private static readonly string s_predefinedMainClass = c_predefinedTypeName + Type.Delimiter +
            c_predefinedMethodName;

        private static readonly string[] s_predefinedImports = new string[]
        {
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Collections.ObjectModel",
            "System.Data",
            "System.Diagnostics",
            "System.IO",
            "System.Linq",
            "System.Reflection",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Web",
            "System.Xml"
        };

        private static readonly string[] s_predefinedReferences = new string[]
        {
            "System.dll",
            Assembly.GetAssembly(typeof(Enumerable)).Location,
            "System.Data.dll",
            "System.Web.dll",
            "System.Xml.dll"
        };

        #endregion

        #region Instance

        private readonly Guid m_scriptId;
        private readonly AppDomain m_domain;
        private readonly string m_scriptFilePath;
        private readonly string[] m_arguments;
        private readonly ScriptType m_scriptType;
        private readonly List<string> m_scriptLines;

        private ScriptExecutionResult m_executionResult;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptExecutor"/> class.
        /// </summary>
        private ScriptExecutor(Guid scriptId, AppDomain domain, string scriptFilePath, string[] arguments)
        {
            #region Argument Check
            if (Guid.Empty.Equals(scriptId))
            {
                throw new ArgumentException("Script ID cannot be empty.", "scriptId");
            }
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            if (scriptFilePath == null)
            {
                throw new ArgumentNullException("scriptFilePath");
            }
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            #endregion

            m_scriptId = scriptId;
            m_domain = domain;
            m_scriptFilePath = scriptFilePath;
            m_arguments = arguments;

            LoadData(out m_scriptType, out m_scriptLines);
        }

        #endregion

        #region Private Methods

        private void LoadData(out ScriptType outputScriptType, out List<string> outputScriptLines)
        {
            FileInfo scriptFileInfo = new FileInfo(m_scriptFilePath);
            if (!scriptFileInfo.Exists)
            {
                throw new FileNotFoundException("Script file is not found.", scriptFileInfo.FullName);
            }

            outputScriptType = ScriptType.Default;

            string[] scriptLines = File.ReadAllLines(scriptFileInfo.FullName);
            int startLineIndex = 0;
            for (int lineIndex = 0; lineIndex < scriptLines.Length; lineIndex++)
            {
                string scriptLine = scriptLines[lineIndex];

                string fixedLine = scriptLine.Trim();
                if (!fixedLine.StartsWith(c_directivePrefix))
                {
                    startLineIndex = lineIndex;
                    break;
                }

                string directive = fixedLine.Substring(c_directivePrefix.Length);
                if (s_directiveComparer.Equals(directive, c_codeDirective))
                {
                    outputScriptType = ScriptType.Code;
                }
                else if (s_directiveComparer.Equals(directive, c_classDirective))
                {
                    outputScriptType = ScriptType.Class;
                }
                else
                {
                    throw new ScriptExecutorException(string.Format(
                        "Invalid script directive: \"{0}\" at line {1}.",
                        directive,
                        lineIndex + 1));
                }
            }

            outputScriptLines = new List<string>(scriptLines.Length - startLineIndex);
            bool hasFirstNotEmptyLine = false;
            for (int index = startLineIndex; index < scriptLines.Length; index++)
            {
                if (!hasFirstNotEmptyLine && string.IsNullOrEmpty(scriptLines[index].Trim()))
                {
                    continue;
                }

                hasFirstNotEmptyLine = true;
                outputScriptLines.Add(scriptLines[index]);
            }
            for (int index = outputScriptLines.Count - 1; index >= 0; index--)
            {
                if (!string.IsNullOrEmpty(outputScriptLines[index].Trim()))
                {
                    break;
                }

                outputScriptLines.RemoveAt(index);
            }
            outputScriptLines.TrimExcess();
        }

        private void ExecuteCodeScript()
        {
            bool isDebuggable = Debugger.IsAttached;

            CompilerParameters compilerParameters = new CompilerParameters()
            {
                GenerateExecutable = false,
                GenerateInMemory = !isDebuggable,
                IncludeDebugInformation = isDebuggable,
                TreatWarningsAsErrors = false
            };
            if (isDebuggable)
            {
                compilerParameters.OutputAssembly = Path.Combine(
                    Path.GetTempPath(),
                    string.Format("{0}_{1:N}.dll", typeof(ScriptExecutor).Name, m_scriptId));
            }
            compilerParameters.ReferencedAssemblies.AddRange(s_predefinedReferences);

            CodeSnippetStatement mainMethodBody = new CodeSnippetStatement(
                string.Join(Environment.NewLine, m_scriptLines.Select(line => "        " + line).ToArray()));

            CodeMemberMethod wrapperMethod = new CodeMemberMethod()
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Static,
                Name = c_predefinedMethodName,
                ReturnType = new CodeTypeReference(typeof(void))
            };
            wrapperMethod.Statements.Add(mainMethodBody);

            CodeTypeDeclaration rootType = new CodeTypeDeclaration(c_predefinedTypeName);
            rootType.Attributes = MemberAttributes.Public;
            rootType.TypeAttributes = TypeAttributes.Class | TypeAttributes.Sealed;
            rootType.Members.Add(wrapperMethod);

            CodeNamespace rootNamespace = new CodeNamespace(string.Empty);
            rootNamespace.Imports.AddRange(s_predefinedImports
                .Select(import => new CodeNamespaceImport(import))
                .ToArray());
            rootNamespace.Types.Add(rootType);

            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(rootNamespace);

            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerResults compilerResults = codeProvider.CompileAssemblyFromDom(compilerParameters, compileUnit);

            if (compilerResults.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("*** Error compiling script ***");
                sb.AppendLine("Errors:");
                IEnumerable<CompilerError> onlyErrors = compilerResults
                    .Errors
                    .Cast<CompilerError>()
                    .Where(error => !error.IsWarning);

                foreach (CompilerError error in onlyErrors)
                {
                    sb.AppendLine(string.Format("  {0}", error));
                }
                sb.AppendLine("Source:");
                using (StringWriter sw = new StringWriter(sb))
                {
                    CodeGeneratorOptions options = new CodeGeneratorOptions()
                    {
                        BlankLinesBetweenMembers = true,
                        BracingStyle = c_bracingStyle,
                        ElseOnClosing = false,
                        IndentString = c_indentString,
                        VerbatimOrder = true
                    };
                    codeProvider.GenerateCodeFromCompileUnit(compileUnit, sw, options);
                }
                sb.AppendLine();

                m_executionResult = new ScriptExecutionResult(ScriptExecutionResultType.CompileError, sb.ToString());
                return;
            }

            string outputAssembly = compilerParameters.OutputAssembly;
            if (!string.IsNullOrEmpty(outputAssembly))
            {
                string[] files = Directory.GetFiles(
                    Path.GetDirectoryName(outputAssembly),
                    Path.ChangeExtension(Path.GetFileName(outputAssembly), ".*"),
                    SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    ScriptExecutorProxy.TempFiles.AddFile(file, false);
                }
            }

            Type compiledType = compilerResults.CompiledAssembly.GetType(c_predefinedTypeName);
            if (compiledType == null)
            {
                throw new ScriptExecutorException(string.Format(
                    "Cannot obtain the predefined type \"{0}\".",
                    c_predefinedTypeName));
            }

            MethodInfo compiledMethod = compiledType.GetMethod(
                c_predefinedMethodName,
                c_predefinedMethodBindingFlags);
            if (compiledMethod == null)
            {
                throw new ScriptExecutorException(string.Format(
                    "Cannot obtain the predefined method \"{0}\" in the type \"{1}\".",
                    c_predefinedMethodName,
                    c_predefinedTypeName));
            }

            Action methodDelegate = (Action)Delegate.CreateDelegate(typeof(Action), compiledMethod, true);
            if (methodDelegate == null)
            {
                throw new ScriptExecutorException(string.Format(
                    "Cannot create a delegate from the predefined method \"{0}{1}{2}\".",
                    compiledMethod.DeclaringType.FullName,
                    Type.Delimiter,
                    compiledMethod.Name));
            }

            try
            {
                methodDelegate();
            }
            catch (Exception ex)
            {
                m_executionResult = ScriptExecutionResult.Create(ScriptExecutionResultType.ExecutionError, ex);
                return;
            }

            m_executionResult = ScriptExecutionResult.Success;
        }

        private void ExecuteClassScript()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Internal Properties

        internal ScriptExecutionResult ExecutionResult
        {
            [DebuggerStepThrough]
            get { return m_executionResult; }
        }

        #endregion

        #region Internal Methods

        internal static ScriptExecutor Create(
            Guid scriptId,
            AppDomain domain,
            string scriptFilePath,
            string[] arguments)
        {
            #region Argument Check
            if (Guid.Empty.Equals(scriptId))
            {
                throw new ArgumentException("Script ID cannot be empty.", "scriptId");
            }
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            if (scriptFilePath == null)
            {
                throw new ArgumentNullException("scriptFilePath");
            }
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            #endregion

            Type scriptExecutorType = typeof(ScriptExecutor);
            ScriptExecutor result = (ScriptExecutor)domain.CreateInstanceAndUnwrap(
                scriptExecutorType.Assembly.FullName,
                scriptExecutorType.FullName,
                false,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new object[] { scriptId, domain, scriptFilePath, arguments },
                null,
                null);
            return result;
        }

        internal void ExecuteInternal()
        {
            try
            {
                switch (m_scriptType)
                {
                    case ScriptType.None:
                        throw new InvalidOperationException("Script type is not assigned after initialization.");
                    case ScriptType.Code:
                        ExecuteCodeScript();
                        break;
                    case ScriptType.Class:
                        ExecuteClassScript();
                        break;
                    default:
                        throw new NotImplementedException(string.Format(
                            "Support of the script type '{0}' is not implemented yet.",
                            m_scriptType.ToString()));
                }
            }
            catch (Exception ex)
            {
                m_executionResult = ScriptExecutionResult.Create(ScriptExecutionResultType.InternalError, ex);
            }

            if (m_executionResult == null)
            {
                throw new InvalidOperationException("Execution result is not assigned after execution.");
            }
        }

        #endregion

        #region Public Methods

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region IScriptExecutor Members

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ScriptExecutionResult Execute()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDisposable Members

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}