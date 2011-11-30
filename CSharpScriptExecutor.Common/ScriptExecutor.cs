using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Assemblies;
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
    // TODO: Implement Compile feature (compile only without execution)

    // TODO: Implement Referenced .cs files (allowed to contain types) that can be referenced and used from main code

    // TODO: Add public API to ScriptExecutor for parsing and using custom directives (to use in GUI)

    public sealed class ScriptExecutor : MarshalByRefObject, IScriptExecutor
    {
        #region Nested Types

        #region ScriptOptions Class

        private sealed class ScriptOptions
        {
            #region Fields

            private readonly Dictionary<string, string> m_assemblyReferences;

            #endregion

            #region Constructors

            internal ScriptOptions()
            {
                m_assemblyReferences = s_predefinedReferences
                    .Select(
                        item => new
                        {
                            Name = Path.GetFileNameWithoutExtension(item),
                            Path = item
                        })
                    .ToDictionary(item => item.Name, item => item.Path, StringComparer.OrdinalIgnoreCase);

                this.NamespaceImports = new HashSet<string>(s_predefinedImports);  // Case-sensitive
            }

            #endregion

            #region Public Properties

            public HashSet<string> NamespaceImports
            {
                get;
                private set;
            }

            #endregion

            #region Public Methods

            public void AddAssemblyReferencePath(string assemblyPath)
            {
                #region Argument Check

                if (string.IsNullOrEmpty(assemblyPath))
                {
                    throw new ArgumentException("The value can be neither empty string nor null.", "assemblyPath");
                }

                #endregion

                var name = Path.GetFileNameWithoutExtension(assemblyPath);
                m_assemblyReferences[name] = assemblyPath;
            }

            public string[] GetAllAssemblyReferencePaths()
            {
                return m_assemblyReferences.Values.ToArray();
            }

            public string GetAssemblyReferencePath(string name)
            {
                #region Argument Check

                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("The value can be neither empty string nor null.", "name");
                }

                #endregion

                return m_assemblyReferences.GetValueOrDefault(name);
            }

            #endregion
        }

        #endregion

        #endregion

        #region Constants

        private const string c_customDirectivePrefix = "//##";
        private const string c_customDirectiveNameGroupName = "Name";
        private const string c_customDirectiveValueGroupName = "Value";
        private const string c_referenceDirective = "Ref";
        private const string c_usingDirective = "Using";

        private const BindingFlags c_anyMemberBindingFlags = BindingFlags.Static | BindingFlags.Instance
            | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags c_predefinedConstructorBindingFlags = BindingFlags.Instance | BindingFlags.Public
            | BindingFlags.NonPublic;
        private const BindingFlags c_predefinedMembersBindingFlags = BindingFlags.Static | BindingFlags.Public
            | BindingFlags.NonPublic;

        private const string c_wrapperTypeName = "ScriptExecutorWrapper";
        private const string c_wrapperMethodName = "Main";
        private const string c_wrapperMethodParameterName = "arguments";
        private const string c_debugMethodName = "Debug";

        private const BindingFlags c_allDeclaredMembersBindingFlags =
            BindingFlags.DeclaredOnly | c_anyMemberBindingFlags;

        private const string c_bracingStyle = "C";

        private const string c_assemblyFileExtension = ".dll";

        //TODO: Check max script size
        //private const ulong c_maxScriptFileSize = 8 * Constants.Megabyte;

        #endregion

        #region Fields

        #region Static

        private static readonly string s_sourceFileExtension = GetSourceFileExtension();
        private static readonly string s_scriptFileExtension = ".cssx";
        private static readonly string s_indentString = new string(' ', 4);
        private static readonly string s_userCodeIndentation = new string(' ', 8);

        private static readonly Regex s_prohibitedDirectiveRegex = new Regex(
            @"^ \s* \# \s* line \b",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

        private static readonly Regex s_customDirectiveRegex = new Regex(
            string.Format(
                @"^ \s* {0}(?<{1}>\S*) \s+ (?<{2}>.*?) \s* $",
                Regex.Escape(c_customDirectivePrefix),
                c_customDirectiveNameGroupName,
                c_customDirectiveValueGroupName),
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

        private static readonly Dictionary<string, Func<ScriptOptions, string, string>> s_customDirectiveActionMap =
            new Dictionary<string, Func<ScriptOptions, string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                { c_referenceDirective, AddCustomAssemblyReference },
                { c_usingDirective, AddCustomNamespaceImport }
            };

        private static readonly char[] s_specialNameChars = new[] { '<', '>' };

        private static readonly string[] s_predefinedImports = new string[]
        {
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Collections.ObjectModel",
            "System.Data",
            "System.Data.Common",
            "System.Data.Linq",
            "System.Data.SqlClient",
            "System.Diagnostics",
            "System.IO",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Reflection",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Threading",
            "System.Web",
            "System.Windows.Forms",
            "System.Xml"
        };

        private static readonly IList<string> s_predefinedReferences =
            new List<string>
            {
                "System.dll",
                Assembly.GetAssembly(typeof(Enumerable)).Location,  // System.Linq.dll
                "System.Data.dll",
                Assembly.GetAssembly(typeof(System.Data.Linq.DataContext)).Location,  // System.Data.Linq.dll
                "System.Web.dll",
                "System.Windows.Forms.dll",
                "System.Xml.dll",
                Assembly.GetAssembly(typeof(RuntimeBinderException)).Location  // Microsoft.CSharp.dll, for 'dynamic'
            }
                .AsReadOnly();

        #endregion

        #region Instance

        private readonly Guid m_scriptId;
        private readonly string m_script;
        private readonly string[] m_arguments;
        private readonly bool m_isDebugMode;
        private readonly List<string> m_scriptLines;
        private readonly ScriptOptions m_options = new ScriptOptions();
        private readonly TemporaryFileList m_tempFiles = new TemporaryFileList();

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
            m_script = parameters.Script;
            m_arguments = parameters.ScriptArguments.ToArray();
            m_isDebugMode = parameters.IsDebugMode;

            domain.AssemblyResolve += this.Domain_AssemblyResolve;

            LoadData(m_script, m_options, out m_scriptLines);
        }

        #endregion

        #region Private Methods

        private static string GetSourceFileExtension()
        {
            var result = new CSharpCodeProvider().UseDisposable(obj => obj.FileExtension);
            if (!result.StartsWith("."))
            {
                result = "." + result;
            }
            return result;
        }

        private static string GetLineNumberPrefix(int lineNumber)
        {
            return string.Format("Line {0}: ", lineNumber);
        }

        private static string AddCustomAssemblyReference(ScriptOptions options, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "[Reference Directive] Assembly path or name must be specified.";
            }

            string errorMessage;
            string fixedValue = value.TryExtractFromQuotes(out errorMessage);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return "[Reference Directive] " + errorMessage;
            }

            options.AddAssemblyReferencePath(fixedValue);
            return null;
        }

        private static string AddCustomNamespaceImport(ScriptOptions options, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "[Using Directive] Namespace import must be specified.";
            }

            options.NamespaceImports.Add(value);
            return null;
        }

        private static void LoadData(
            string script,
            ScriptOptions options,
            out List<string> outputScriptLines)
        {
            #region Argument Check

            if (script == null)
            {
                throw new ArgumentNullException("script");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            #endregion

            List<string> scriptLines = new List<string>();
            using (var reader = new StringReader(script))
            {
                var customDirectiveAllowed = true;
                var lineNumber = 0;
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    lineNumber++;

                    if (s_prohibitedDirectiveRegex.IsMatch(line))
                    {
                        throw new ScriptExecutorException(
                            string.Format(
                                "{0}Directive '#line' is prohibited to use in a script"
                                    + " (even in the multiline comment block).",
                                GetLineNumberPrefix(lineNumber)));
                    }

                    var isCustomDirective = false;
                    if (customDirectiveAllowed)
                    {
                        var customDirectiveMatch = s_customDirectiveRegex.Match(line);
                        if (customDirectiveMatch != null && customDirectiveMatch.Success)
                        {
                            isCustomDirective = true;

                            var customDirectiveName = customDirectiveMatch
                                .Groups[c_customDirectiveNameGroupName]
                                .Value;
                            var customDirectiveValue = customDirectiveMatch
                                .Groups[c_customDirectiveValueGroupName]
                                .Value;

                            var action = s_customDirectiveActionMap.GetValueOrDefault(customDirectiveName);
                            if (action == null)
                            {
                                throw new ScriptExecutorException(
                                    string.Format(
                                        "{0}Unknown custom directive \"{1}\".",
                                        GetLineNumberPrefix(lineNumber),
                                        customDirectiveName));
                            }

                            var errorMessage = action(options, customDirectiveValue);
                            if (!string.IsNullOrWhiteSpace(errorMessage))
                            {
                                throw new ScriptExecutorException(GetLineNumberPrefix(lineNumber) + errorMessage);
                            }
                        }
                    }

                    if (customDirectiveAllowed && !isCustomDirective && !string.IsNullOrWhiteSpace(line))
                    {
                        customDirectiveAllowed = false;
                    }

                    scriptLines.Add(line);
                }
            }

            outputScriptLines = scriptLines;
            outputScriptLines.TrimExcess();
        }

        private Assembly Domain_AssemblyResolve(object sender, ResolveEventArgs e)
        {
            var assemblyName = new AssemblyName(e.Name);
            var assemblyPath = m_options.GetAssemblyReferencePath(assemblyName.Name);

            try
            {
                if (!File.Exists(assemblyPath))
                {
                    return null;
                }

                var publicKeyToken = assemblyName.GetPublicKeyToken();

                var assembly = Assembly.LoadFrom(assemblyPath);
                if (assembly != null && assembly.GetName().Version == assemblyName.Version)
                {
                    var loadedAssemblyName = assembly.GetName();
                    if (assembly != null && loadedAssemblyName.Version == assemblyName.Version
                        && publicKeyToken.BytesEqual(loadedAssemblyName.GetPublicKeyToken()))
                    {
                        return assembly;
                    }
                }
            }
            catch (Exception)
            {
                // Nothing to do
            }

            return null;
        }

        private CodeConstructor CreatePredefinedConstructor()
        {
            var result = new CodeConstructor
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
                }
            };
            if (!m_isDebugMode)
            {
                result.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, string.Empty));
            }
            return result;
        }

        private CodeMemberMethod CreateDebugMethod()
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

            return new CodeMemberMethod
            {
                Name = c_debugMethodName,
                Attributes = MemberAttributes.Assembly | MemberAttributes.Static,
                CustomAttributes =
                {
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(DebuggerStepThroughAttribute))),
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(CompilerGeneratedAttribute)))
                },
                EndDirectives = { new CodeRegionDirective(CodeRegionMode.End, string.Empty) },
                ReturnType = new CodeTypeReference(typeof(void)),
                Statements =
                {
                    new CodeConditionStatement(
                        new CodePropertyReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(Debugger)),
                            isAttachedName),
                            new[] { debuggerBreakStatement },
                            new[] { debuggerLaunchStatement })
                }
            };
        }

        private CodeMemberMethod CreateUserCodeWrapperMethod(CodeTypeDeclaration declaringType, string offsetWarningId)
        {
            #region Argument Check

            if (declaringType == null)
            {
                throw new ArgumentNullException("declaringType");
            }

            #endregion

            var formattedScriptLinesString = string.Join(
                Environment.NewLine,
                m_scriptLines.Select(line => s_userCodeIndentation + line));

            var userCodeSnippetStatement = new CodeSnippetStatement(
                string.Format(
                    "#warning [For internal purposes] {1}{0}"
                        + "{2}{0}"
                        + "{0}"
                        + "{3}; return null; // Auto-generated",
                    Environment.NewLine,
                    offsetWarningId,
                    formattedScriptLinesString,
                    s_userCodeIndentation));

            var result = new CodeMemberMethod
            {
                Name = c_wrapperMethodName,
                Attributes = MemberAttributes.Assembly | MemberAttributes.Static,
                ReturnType = new CodeTypeReference(typeof(object)),
                Parameters =
                {
                    new CodeParameterDeclarationExpression(typeof(string[]), c_wrapperMethodParameterName)
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
                result.Statements.Add(
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                null,
                                c_debugMethodName)))
                    {
                        StartDirectives =
                        {
                            new CodeRegionDirective(
                                CodeRegionMode.Start,
                                string.Format(
                                    "This statement is generated by {0} since the debug mode is active.",
                                    this.GetType().FullName))
                        },
                        EndDirectives =
                        {
                            new CodeRegionDirective(CodeRegionMode.End, string.Empty)
                        }
                    });
                result.Statements.Add(new CodeSnippetStatement(Environment.NewLine));
            }
            result.Statements.Add(userCodeSnippetStatement);

            return result;
        }

        private void GenerateAndRunScript()
        {
            bool isDebuggable = Debugger.IsAttached || m_isDebugMode;

            var compilerParameters = new CompilerParameters()
            {
                GenerateExecutable = false,
                GenerateInMemory = !isDebuggable,
                IncludeDebugInformation = isDebuggable,
                TreatWarningsAsErrors = false,
                // TODO: Implement `unsafe` option
                CompilerOptions = string.Format("/unsafe- /optimize{0}", isDebuggable ? "-" : "+")
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

            compilerParameters.ReferencedAssemblies.AddRange(m_options.GetAllAssemblyReferencePaths());

            Func<string> generateRandomId = () => Guid.NewGuid().ToString("N");
            var offsetWarningId = string.Join(string.Empty, Enumerable.Range(0, 4).Select(i => generateRandomId()));

            var wrapperCodeType = new CodeTypeDeclaration(c_wrapperTypeName)
            {
                Attributes = MemberAttributes.Public,
                TypeAttributes = TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.NotPublic
            };

            var predefinedConstructor = CreatePredefinedConstructor();
            var userCodeWrapperMethod = CreateUserCodeWrapperMethod(wrapperCodeType, offsetWarningId);

            wrapperCodeType.Members.Add(predefinedConstructor);
            if (m_isDebugMode)
            {
                var debugMethod = CreateDebugMethod();
                wrapperCodeType.Members.Add(debugMethod);
            }
            wrapperCodeType.Members.Add(userCodeWrapperMethod);

            var rootNamespace = new CodeNamespace(string.Empty);
            rootNamespace.Imports.AddRange(
                m_options
                    .NamespaceImports
                    .Select(item => new CodeNamespaceImport(item))
                    .ToArray());
            rootNamespace.Types.Add(wrapperCodeType);

            var compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(rootNamespace);

            CompilerResults compilerResults;
            using (var codeProvider = new CSharpCodeProvider())
            {
                var generatedCodeBuilder = new StringBuilder();
                using (var sw = new StringWriter(generatedCodeBuilder))
                {
                    var options = new CodeGeneratorOptions()
                    {
                        BlankLinesBetweenMembers = true,
                        BracingStyle = c_bracingStyle,
                        ElseOnClosing = false,
                        IndentString = s_indentString,
                        VerbatimOrder = true
                    };
                    codeProvider.GenerateCodeFromCompileUnit(compileUnit, sw, options);
                }

                var generatedCode = generatedCodeBuilder.ToString();

                if (isDebuggable)
                {
                    var filePath = Path.ChangeExtension(compilerParameters.OutputAssembly, s_sourceFileExtension);
                    File.WriteAllText(filePath, generatedCode, Encoding.UTF8);

                    compilerResults = codeProvider.CompileAssemblyFromFile(compilerParameters, filePath);
                }
                else
                {
                    compilerResults = codeProvider.CompileAssemblyFromSource(compilerParameters, generatedCode);
                }

                if (compilerResults.Errors.HasErrors)
                {
                    var onlyErrors = compilerResults
                        .Errors
                        .Cast<CompilerError>()
                        .Where(error => !error.IsWarning)
                        .ToList();

                    var offsetWarning = compilerResults
                        .Errors
                        .Cast<CompilerError>()
                        .Where(item => item.IsWarning && item.ErrorText.Contains(offsetWarningId))
                        .SingleOrDefault();
                    var sourceCodeLineOffset = offsetWarning == null ? 0 : offsetWarning.Line;

                    m_executionResult = ScriptExecutionResult.CreateError(
                        ScriptExecutionResultType.CompilationError,
                        new ScriptExecutorException("Error compiling script"),
                        m_script,
                        generatedCode,
                        string.Empty,
                        string.Empty,
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
                        m_tempFiles.Add(file);
                    }
                }

                var compiledWrapperType = compilerResults.CompiledAssembly.GetType(c_wrapperTypeName);
                if (compiledWrapperType == null)
                {
                    throw new ScriptExecutorException(
                        string.Format("Cannot obtain the wrapper type \"{0}\".", c_wrapperTypeName),
                        m_script,
                        generatedCode);
                }

                var compiledPredefinedConstructor = compiledWrapperType.GetConstructor(
                    c_predefinedConstructorBindingFlags,
                    null,
                    Type.EmptyTypes,
                    null);
                if (compiledPredefinedConstructor == null)
                {
                    throw new ScriptExecutorException(
                        string.Format(
                            "Cannot obtain the predefined parameterless constructor in the type \"{0}\".",
                            c_wrapperTypeName),
                        m_script,
                        generatedCode);
                }

                MethodInfo compiledDebugMethod = null;
                if (m_isDebugMode)
                {
                    compiledDebugMethod = compiledWrapperType.GetMethod(
                        c_debugMethodName,
                        c_predefinedMembersBindingFlags);
                    if (compiledDebugMethod == null)
                    {
                        throw new ScriptExecutorException(
                            string.Format(
                                "Cannot obtain the debug method \"{0}\" in the type \"{1}\".",
                                c_debugMethodName,
                                c_wrapperTypeName),
                            m_script,
                            generatedCode);
                    }
                }

                var compiledWrapperMethod = compiledWrapperType.GetMethod(
                    c_wrapperMethodName,
                    c_predefinedMembersBindingFlags);
                if (compiledWrapperMethod == null)
                {
                    throw new ScriptExecutorException(
                        string.Format(
                            "Cannot obtain the wrapper method \"{0}\" in the type \"{1}\".",
                            c_wrapperMethodName,
                            c_wrapperTypeName),
                        m_script,
                        generatedCode);
                }

                var allDeclaredMembers = compiledWrapperType.GetMembers(c_allDeclaredMembersBindingFlags).ToList();
                var unexpectedMembers = allDeclaredMembers
                    .Where(
                        item => item != compiledWrapperMethod && item != compiledPredefinedConstructor
                            && item != compiledDebugMethod && item.Name.IndexOfAny(s_specialNameChars) < 0)
                    .ToList();
                if (unexpectedMembers.Any())
                {
                    throw new ScriptExecutorException(
                        "The script must not contain any members and must be just a code snippet.",
                        m_script,
                        generatedCode);
                }

                var methodDelegate = (Func<string[], object>)Delegate.CreateDelegate(
                    typeof(Func<string[], object>),
                    compiledWrapperMethod,
                    true);
                if (methodDelegate == null)
                {
                    throw new ScriptExecutorException(
                        string.Format(
                            "Cannot create a delegate from the wrapper method \"{0}{1}{2}\".",
                            compiledWrapperMethod.DeclaringType.FullName,
                            Type.Delimiter,
                            compiledWrapperMethod.Name),
                        m_script,
                        generatedCode);
                }

                object scriptReturnValue;

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
                            scriptReturnValue = methodDelegate(m_arguments);
                        }
                        catch (Exception ex)
                        {
                            m_executionResult = ScriptExecutionResult.CreateError(
                                ScriptExecutionResultType.ExecutionError,
                                ex,
                                m_script,
                                generatedCode,
                                consoleOutBuilder.ToString(),
                                consoleErrorBuilder.ToString(),
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
                        m_script,
                        generatedCode,
                        null,
                        null,
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
                    scriptReturnValue,
                    m_script,
                    generatedCode,
                    consoleOutBuilder.ToString(),
                    consoleErrorBuilder.ToString());
            }
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
            if (m_executionResult != null)
            {
                throw new InvalidOperationException(string.Format("'{0}' cannot be reused.", GetType().FullName));
            }

            try
            {
                GenerateAndRunScript();
            }
            catch (Exception ex)
            {
                m_executionResult = ScriptExecutionResult.CreateError(
                    ScriptExecutionResultType.InternalError,
                    ex,
                    m_script,
                    null,
                    null,
                    null,
                    null,
                    null);
            }

            if (m_executionResult == null)
            {
                throw new InvalidOperationException("Execution result is not assigned after execution.");
            }
        }

        internal TemporaryFileList GetTemporaryFiles()
        {
            return m_tempFiles.Copy();
        }

        #endregion

        #region Public Properties

        public static string SourceFileExtension
        {
            [DebuggerStepThrough]
            get { return s_sourceFileExtension; }
        }

        public static string ScriptFileExtension
        {
            [DebuggerStepThrough]
            get { return s_scriptFileExtension; }
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

            var scriptId = Guid.NewGuid();
            var domainName = string.Format("{0}_Domain_{1:N}", typeof(ScriptExecutor).Name, scriptId);
            var domain = AppDomain.CreateDomain(domainName);
            try
            {
                var scriptExecutorType = typeof(ScriptExecutor);
                var scriptExecutor = (ScriptExecutor)domain.CreateInstanceAndUnwrap(
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