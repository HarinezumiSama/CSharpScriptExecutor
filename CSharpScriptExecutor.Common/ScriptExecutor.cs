using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using Microsoft.CSharp.RuntimeBinder;

namespace CSharpScriptExecutor.Common
{
    public sealed class ScriptExecutor : MarshalByRefObject, IScriptExecutor
    {
        #region Constants

        private const string c_directivePrefix = "##";
        private const string c_codeDirective = "Code";
        private const string c_classDirective = "Class";

        // TODO: `##Reference` script directive (reference to an assembly)

        private const string c_predefinedTypeName = "ScriptExecutorWrapper";
        private const string c_predefinedMethodName = "Main";
        private const BindingFlags c_predefinedMethodBindingFlags = BindingFlags.Static | BindingFlags.Public
            | BindingFlags.NonPublic;
        private const string c_predefinedMethodParameterName = "arguments";
        private const BindingFlags c_allDecalredMemberBindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Static
            | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const string c_bracingStyle = "C";
        private const string c_indentString = "    ";

        private const ulong c_maxScriptFileSize = 8 * Constants.Megabyte;

        #endregion

        #region Fields

        #region Static

        private static readonly string s_sourceFileExtension = GetSourceFileExtension();
        private static readonly string s_userCodeIndentation = new string(' ', 8);

        private static readonly Regex s_prohibitedDirectiveRegex = new Regex(
            @"^\s* \# \s* line \b",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

        private static readonly StringComparer s_directiveComparer = StringComparer.OrdinalIgnoreCase;
        private static readonly string s_predefinedMainClass = c_predefinedTypeName + Type.Delimiter +
            c_predefinedMethodName;

        private static readonly char[] s_specialNameChars = new[] { '<', '>' };

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
            "System.Linq.Expressions",
            "System.Reflection",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Web",
            "System.Windows.Forms",
            "System.Xml"
        };

        private static readonly string[] s_predefinedReferences = new string[]
        {
            "System.dll",
            Assembly.GetAssembly(typeof(Enumerable)).Location,  // System.Linq.dll
            Assembly.GetAssembly(typeof(RuntimeBinderException)).Location,  // Microsoft.CSharp.dll
            "System.Data.dll",
            "System.Web.dll",
            "System.Windows.Forms.dll",
            "System.Xml.dll"
        };

        #endregion

        #region Instance

        private readonly Guid m_scriptId;
        private readonly AppDomain m_domain;
        private readonly string m_script;
        private readonly string[] m_arguments;
        private readonly bool m_isDebugMode;
        private readonly ScriptType m_scriptType;
        private readonly List<string> m_scriptLines;

        private ScriptExecutionResult m_executionResult;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptExecutor"/> class.
        /// </summary>
        private ScriptExecutor(Guid scriptId, AppDomain domain, ScriptExecutorParameters parameters)
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
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            #endregion

            m_scriptId = scriptId;
            m_domain = domain;
            m_script = parameters.Script;
            m_arguments = parameters.ScriptArguments.ToArray();
            m_isDebugMode = parameters.IsDebugMode;

            LoadData(m_script, out m_scriptType, out m_scriptLines);
        }

        #endregion

        #region Private Methods

        private static string GetSourceFileExtension()
        {
            var result = new CSharpCodeProvider().FileExtension;
            if (!result.StartsWith("."))
            {
                result = "." + result;
            }
            return result;
        }

        private static void LoadData(
            string script,
            out ScriptType outputScriptType,
            out List<string> outputScriptLines)
        {
            #region Argument Check

            if (script == null)
            {
                throw new ArgumentNullException("script");
            }

            #endregion

            outputScriptType = ScriptType.Default;

            List<string> scriptLines = new List<string>();
            using (var reader = new StringReader(script))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    if (s_prohibitedDirectiveRegex.IsMatch(line))
                    {
                        throw new ScriptExecutorException(
                            "Directive '#line' is prohibited to use in a script"
                                + " (even in the multiline comment block).");
                    }

                    scriptLines.Add(line);
                }
            }

            int startLineIndex = 0;
            for (int lineIndex = 0; lineIndex < scriptLines.Count; lineIndex++)
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
                    throw new ScriptExecutorException(
                        string.Format(
                            "Invalid script directive: \"{0}\" at line {1}.",
                            directive,
                            lineIndex + 1),
                        script);
                }
            }

            outputScriptLines = scriptLines.Skip(startLineIndex).ToList();
            outputScriptLines.TrimExcess();
        }

        private void ExecuteCodeScript()
        {
            bool isDebuggable = Debugger.IsAttached || m_isDebugMode;

            var compilerParameters = new CompilerParameters()
            {
                GenerateExecutable = false,
                GenerateInMemory = !isDebuggable,
                IncludeDebugInformation = isDebuggable,
                TreatWarningsAsErrors = false,
                CompilerOptions = string.Format("/unsafe+ /optimize{0}", isDebuggable ? "-" : "+")
            };
            if (isDebuggable)
            {
                compilerParameters.OutputAssembly = Path.Combine(
                    Path.GetTempPath(),
                    string.Format(
                        "{0}__{1:yyyy'-'MM'-'dd__HH'-'mm'-'ss}__{2}.dll",
                        typeof(ScriptExecutor).Name,
                        DateTime.Now,
                        m_scriptId.GetHashCode().ToString("X8").ToLowerInvariant()));
            }
            compilerParameters.ReferencedAssemblies.AddRange(s_predefinedReferences);

            Func<string> generateRandomId = () => Guid.NewGuid().ToString("N");
            var offsetWarningId = string.Join(string.Empty, Enumerable.Range(0, 4).Select(i => generateRandomId()));

            var formattedScriptLinesString = string.Join(
                Environment.NewLine,
                m_scriptLines.Select(line => s_userCodeIndentation + line));

            var userCodeSnippetStatement = new CodeSnippetStatement(
                string.Format(
                    "#warning [For internal purposes] {0}{1}"
                        + "{2}{1}"
                        + "{1}"
                        + "{3}; // Auto-generated",
                    offsetWarningId,
                    Environment.NewLine,
                    formattedScriptLinesString,
                    s_userCodeIndentation));
            {
                //StartDirectives = { new CodeRegionDirective(CodeRegionMode.Start, "User's code snippet") },
                //EndDirectives = { new CodeRegionDirective(CodeRegionMode.End, string.Empty) }
            };

            var wrapperMethod = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Assembly | MemberAttributes.Static,
                Name = c_predefinedMethodName,
                ReturnType = new CodeTypeReference(typeof(void)),
                Parameters =
                {
                    new CodeParameterDeclarationExpression(typeof(string[]), c_predefinedMethodParameterName)
                    {
                        CustomAttributes =
                        {
                            new CodeAttributeDeclaration(new CodeTypeReference(typeof(ParamArrayAttribute)))
                        }
                    }
                }
            };

            if (m_isDebugMode)
            {
                var isAttachedName =
                    ((MemberExpression)((Expression<Func<bool>>)(() => Debugger.IsAttached)).Body).Member.Name;

                var debuggerBreakStatement = new CodeExpressionStatement(
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(Debugger)),
                            new Action(Debugger.Break).Method.Name)));
                var debuggerLaunchStatement = new CodeExpressionStatement(
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(Debugger)),
                            new Func<bool>(Debugger.Launch).Method.Name)));
                wrapperMethod.Statements.Add(
                    new CodeConditionStatement(
                        new CodePropertyReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(Debugger)),
                            isAttachedName),
                            new[] { debuggerBreakStatement },
                            new[] { debuggerLaunchStatement })
                    {
                        StartDirectives =
                        {
                            new CodeRegionDirective(
                                CodeRegionMode.Start,
                                string.Format(
                                    "This statement is generated by {0} since the debug mode is active.",
                                    this.GetType().Name))
                        },
                        EndDirectives =
                        {
                            new CodeRegionDirective(CodeRegionMode.End, string.Empty)
                        }
                    });
                wrapperMethod.Statements.Add(new CodeSnippetStatement(Environment.NewLine));
            }
            wrapperMethod.Statements.Add(userCodeSnippetStatement);

            var predefinedConstructor = new CodeConstructor()
            {
                CustomAttributes =
                {
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(DebuggerStepThroughAttribute))),
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(CompilerGeneratedAttribute)))
                },
                Statements = { new CodeCommentStatement("Nothing to do") },
                StartDirectives =
                {
                    new CodeRegionDirective(CodeRegionMode.Start, "Auto-generated code")
                },
                EndDirectives = { new CodeRegionDirective(CodeRegionMode.End, string.Empty) }
            };

            var rootType = new CodeTypeDeclaration(c_predefinedTypeName)
            {
                Attributes = MemberAttributes.Public,
                TypeAttributes = TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.NotPublic,
                Members = { predefinedConstructor, wrapperMethod }
            };

            var rootNamespace = new CodeNamespace(string.Empty);
            rootNamespace.Imports.AddRange(
                s_predefinedImports.Select(item => new CodeNamespaceImport(item)).ToArray());
            rootNamespace.Types.Add(rootType);

            var compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(rootNamespace);

            CompilerResults compilerResults;
            using (var codeProvider = new CSharpCodeProvider())
            {
                string sourceContent = null;
                Func<string> getSourceContent =
                    () =>
                    {
                        if (!string.IsNullOrWhiteSpace(sourceContent))
                        {
                            return sourceContent;
                        }

                        var sourceBuilder = new StringBuilder();
                        using (var sw = new StringWriter(sourceBuilder))
                        {
                            var options = new CodeGeneratorOptions()
                            {
                                BlankLinesBetweenMembers = true,
                                BracingStyle = c_bracingStyle,
                                ElseOnClosing = false,
                                IndentString = c_indentString,
                                VerbatimOrder = true
                            };
                            codeProvider.GenerateCodeFromCompileUnit(compileUnit, sw, options);
                        }

                        sourceContent = sourceBuilder.ToString();
                        return sourceContent;
                    };

                if (isDebuggable)
                {
                    var filePath = Path.ChangeExtension(compilerParameters.OutputAssembly, s_sourceFileExtension);
                    File.WriteAllText(filePath, getSourceContent(), Encoding.UTF8);

                    compilerResults = codeProvider.CompileAssemblyFromFile(compilerParameters, filePath);
                }
                else
                {
                    compilerResults = codeProvider.CompileAssemblyFromDom(compilerParameters, compileUnit);
                }

                if (compilerResults.Errors.HasErrors)
                {
                    var sb = new StringBuilder();
                    //sb.AppendLine("*** Error compiling script ***");
                    sb.AppendLine("Errors:");
                    var onlyErrors = compilerResults
                        .Errors
                        .Cast<CompilerError>()
                        .Where(error => !error.IsWarning)
                        .ToList();

                    foreach (CompilerError error in onlyErrors)
                    {
                        sb.AppendLine(string.Format("  {0}", error));
                    }
                    //sb.AppendLine("Source:");
                    //sb.Append(getSourceContent());
                    //sb.AppendLine();

                    var offsetWarning = compilerResults
                        .Errors
                        .Cast<CompilerError>()
                        .Where(item => item.IsWarning && item.ErrorText.Contains(offsetWarningId))
                        .Single();
                    var sourceCodeLineOffset = offsetWarning.Line;

                    m_executionResult = ScriptExecutionResult.CreateError(
                        ScriptExecutionResultType.CompilationError,
                        new ScriptExecutorException(sb.ToString()),
                        string.Empty,
                        string.Empty,
                        m_script,
                        getSourceContent(),
                        onlyErrors,
                        sourceCodeLineOffset);
                    return;
                }

                string outputAssembly = compilerParameters.OutputAssembly;
                if (!string.IsNullOrEmpty(outputAssembly))
                {
                    var files = Directory.GetFiles(
                        Path.GetDirectoryName(outputAssembly),
                        Path.ChangeExtension(Path.GetFileName(outputAssembly), ".*"),
                        SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        ScriptExecutorProxy.TempFiles.AddFile(file, false);
                    }
                }

                var compiledType = compilerResults.CompiledAssembly.GetType(c_predefinedTypeName);
                if (compiledType == null)
                {
                    throw new ScriptExecutorException(
                        string.Format("Cannot obtain the predefined type \"{0}\".", c_predefinedTypeName),
                        m_script,
                        getSourceContent());
                }

                var compiledMethod = compiledType.GetMethod(
                    c_predefinedMethodName,
                    c_predefinedMethodBindingFlags);
                if (compiledMethod == null)
                {
                    throw new ScriptExecutorException(
                        string.Format(
                            "Cannot obtain the predefined method \"{0}\" in the type \"{1}\".",
                            c_predefinedMethodName,
                            c_predefinedTypeName),
                        m_script,
                        getSourceContent());
                }

                var compiledPredefinedConstructor = compiledType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    Type.EmptyTypes,
                    null);
                if (compiledPredefinedConstructor == null)
                {
                    throw new ScriptExecutorException(
                        string.Format(
                            "Cannot obtain the predefined parameterless constructor in the type \"{0}\".",
                            c_predefinedTypeName),
                        m_script,
                        getSourceContent());
                }

                var allDeclaredMembers = compiledType.GetMembers(c_allDecalredMemberBindingFlags).ToList();
                var unexpectedMembers = allDeclaredMembers
                    .Where(
                        item => item != compiledMethod && item != compiledPredefinedConstructor
                            && item.Name.IndexOfAny(s_specialNameChars) < 0)
                    .ToList();
                if (unexpectedMembers.Any())
                {
                    throw new ScriptExecutorException(
                        "The script must not contain any members and must be just a code snippet.",
                        m_script,
                        getSourceContent());
                }

                var methodDelegate = (Action<string[]>)Delegate.CreateDelegate(
                    typeof(Action<string[]>),
                    compiledMethod,
                    true);
                if (methodDelegate == null)
                {
                    throw new ScriptExecutorException(
                        string.Format(
                            "Cannot create a delegate from the predefined method \"{0}{1}{2}\".",
                            compiledMethod.DeclaringType.FullName,
                            Type.Delimiter,
                            compiledMethod.Name),
                        m_script,
                        getSourceContent());
                }

                var consoleOutBuilder = new StringBuilder();
                var consoleErrorBuilder = new StringBuilder();
                var originalConsoleOut = Console.Out;
                var originalConsoleError = Console.Error;
                try
                {
                    using (StringWriter outWriter = new StringWriter(consoleOutBuilder),
                        errorWriter = new StringWriter(consoleErrorBuilder))
                    {
                        Console.SetOut(outWriter);
                        Console.SetError(errorWriter);

                        try
                        {
                            methodDelegate(m_arguments);
                        }
                        catch (Exception ex)
                        {
                            m_executionResult = ScriptExecutionResult.CreateError(
                                ScriptExecutionResultType.ExecutionError,
                                ex,
                                consoleOutBuilder.ToString(),
                                consoleErrorBuilder.ToString(),
                                m_script,
                                getSourceContent(),
                                null,
                                null);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_executionResult = ScriptExecutionResult.CreateError(
                        ScriptExecutionResultType.InternalError,
                        ex,
                        string.Empty,
                        string.Empty,
                        m_script,
                        getSourceContent(),
                        null,
                        null);
                    return;
                }
                finally
                {
                    Console.SetOut(originalConsoleOut);
                    Console.SetError(originalConsoleError);
                }

                m_executionResult = ScriptExecutionResult.CreateSuccess(
                    consoleOutBuilder.ToString(),
                    consoleErrorBuilder.ToString(),
                    m_script,
                    getSourceContent());
            }
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
                        throw new NotImplementedException(
                            string.Format(
                                "Support of the script type '{0}' is not implemented yet.",
                                m_scriptType.ToString()));
                }
            }
            catch (Exception ex)
            {
                m_executionResult = ScriptExecutionResult.CreateError(
                    ScriptExecutionResultType.InternalError,
                    ex,
                    string.Empty,
                    string.Empty,
                    m_script,
                    null,
                    null,
                    null);
            }

            if (m_executionResult == null)
            {
                throw new InvalidOperationException("Execution result is not assigned after execution.");
            }
        }

        #endregion

        #region Public Properties

        public static string SourceFileExtension
        {
            [DebuggerStepThrough]
            get { return s_sourceFileExtension; }
        }

        public string Script
        {
            [DebuggerStepThrough]
            get { return m_script; }
        }

        #endregion

        #region Public Methods

        public static IScriptExecutor Create(ScriptExecutorParameters parameters)
        {
            #region Argument Check

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            #endregion

            ScriptExecutorProxy result;

            Guid scriptId = Guid.NewGuid();
            AppDomain domain = AppDomain.CreateDomain(
                string.Format("{0}_Domain_{1:N}", typeof(ScriptExecutor).Name, scriptId));
            try
            {
                Type scriptExecutorType = typeof(ScriptExecutor);
                ScriptExecutor scriptExecutor = (ScriptExecutor)domain.CreateInstanceAndUnwrap(
                    scriptExecutorType.Assembly.FullName,
                    scriptExecutorType.FullName,
                    false,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    (System.Reflection.Binder)null,
                    new object[] { scriptId, domain, parameters },
                    (CultureInfo)null,
                    (object[])null);

                result = new ScriptExecutorProxy(scriptId, domain, scriptExecutor);
            }
            catch (Exception)
            {
                AppDomain.Unload(domain);
                throw;
            }

            return result;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region IScriptExecutor Members

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ScriptExecutionResult Execute()
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        #endregion

        #region IDisposable Members

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            // Nothing to do
        }

        #endregion
    }
}