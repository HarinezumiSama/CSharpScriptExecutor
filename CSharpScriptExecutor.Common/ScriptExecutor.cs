using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        #region Constants and Fields

        private const string CustomDirectivePrefix = "//##";
        private const string CustomDirectiveNameGroupName = "Name";
        private const string CustomDirectiveValueGroupName = "Value";
        private const string ReferenceDirective = "Ref";
        private const string UsingDirective = "Using";

        private const BindingFlags AnyMemberBindingFlags = BindingFlags.Static | BindingFlags.Instance
            | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags PredefinedConstructorBindingFlags = BindingFlags.Instance | BindingFlags.Public
            | BindingFlags.NonPublic;

        private const BindingFlags PredefinedMembersBindingFlags = BindingFlags.Static | BindingFlags.Public
            | BindingFlags.NonPublic;

        private const string WrapperTypeName = "ScriptExecutorWrapper";
        private const string WrapperMethodName = "Main";
        private const string WrapperMethodParameterName = "arguments";
        private const string DebugMethodName = "Debug";

        private const BindingFlags AllDeclaredMembersBindingFlags =
            BindingFlags.DeclaredOnly | AnyMemberBindingFlags;

        private const string BracingStyle = "C";

        //TODO: Check max script size
        //private const ulong c_maxScriptFileSize = 8 * Constants.Megabyte;

        private static readonly string SourceFileExtensionValue = GetSourceFileExtension();
        private const string ScriptFileExtensionValue = ".cssx";
        private static readonly string IndentString = new string(' ', 4);
        private static readonly string UserCodeIndentation = new string(' ', 8);

        private static readonly Regex ProhibitedDirectiveRegex = new Regex(
            @"^ \s* \# \s* line \b",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

        private static readonly Regex CustomDirectiveRegex = new Regex(
            $@"^ \s* {Regex.Escape(CustomDirectivePrefix)}(?<{CustomDirectiveNameGroupName}>\S*) \s+ (?<{
                CustomDirectiveValueGroupName}>.*?) \s* $",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

        private static readonly Dictionary<string, Func<ScriptOptions, string, string>> CustomDirectiveActionMap =
            new Dictionary<string, Func<ScriptOptions, string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                { ReferenceDirective, AddCustomAssemblyReference },
                { UsingDirective, AddCustomNamespaceImport }
            };

        private static readonly char[] SpecialNameChars = { '<', '>' };

        private static readonly string[] PredefinedImports =
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

        private static readonly ReadOnlyCollection<string> PredefinedReferences =
            new ReadOnlyCollection<string>(
                new[]
                {
                    "System.dll",
                    Assembly.GetAssembly(typeof(Enumerable)).Location, // System.Linq.dll
                    "System.Data.dll",
                    Assembly.GetAssembly(typeof(System.Data.Linq.DataContext)).Location, // System.Data.Linq.dll
                    "System.Web.dll",
                    "System.Windows.Forms.dll",
                    "System.Xml.dll",
                    Assembly.GetAssembly(typeof(RuntimeBinderException)).Location

                    // Microsoft.CSharp.dll, for 'dynamic'
                });

        private readonly Guid _scriptId;
        private readonly string _script;
        private readonly string[] _arguments;
        private readonly bool _isDebugMode;
        private readonly List<string> _scriptLines;
        private readonly ScriptOptions _options = new ScriptOptions();
        private readonly TemporaryFileList _tempFiles = new TemporaryFileList();

        private ScriptExecutionResult _executionResult;

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
                throw new ArgumentException("Script ID cannot be empty.", nameof(scriptId));
            }
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            #endregion

            _scriptId = scriptId;
            _script = parameters.Script;
            _arguments = parameters.ScriptArguments.ToArray();
            _isDebugMode = parameters.IsDebugMode;

            domain.AssemblyResolve += Domain_AssemblyResolve;

            LoadData(_script, _options, out _scriptLines);
        }

        #endregion

        #region Private Methods

        private static string GetSourceFileExtension()
        {
            var result = new CSharpCodeProvider().UseDisposable(obj => obj.FileExtension);
            if (!result.StartsWith(".", StringComparison.Ordinal))
            {
                result = "." + result;
            }

            return result;
        }

        private static string GetLineNumberPrefix(int lineNumber) => $@"Line {lineNumber}: ";

        private static string AddCustomAssemblyReference(ScriptOptions options, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "[Reference Directive] Assembly path or name must be specified.";
            }

            string errorMessage;
            var fixedValue = value.TryExtractFromQuotes(out errorMessage);
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
                throw new ArgumentNullException(nameof(script));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            #endregion

            var scriptLines = new List<string>();
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

                    if (ProhibitedDirectiveRegex.IsMatch(line))
                    {
                        throw new ScriptExecutorException(
                            $@"{GetLineNumberPrefix(lineNumber)}Directive '#line' is prohibited to use in a script (even in the multiline comment block).");
                    }

                    var isCustomDirective = false;
                    if (customDirectiveAllowed)
                    {
                        var customDirectiveMatch = CustomDirectiveRegex.Match(line);
                        if (customDirectiveMatch.Success)
                        {
                            isCustomDirective = true;

                            var customDirectiveName = customDirectiveMatch
                                .Groups[CustomDirectiveNameGroupName]
                                .Value;
                            var customDirectiveValue = customDirectiveMatch
                                .Groups[CustomDirectiveValueGroupName]
                                .Value;

                            var action = CustomDirectiveActionMap.GetValueOrDefault(customDirectiveName);
                            if (action == null)
                            {
                                throw new ScriptExecutorException(
                                    $@"{GetLineNumberPrefix(lineNumber)}Unknown custom directive ""{
                                        customDirectiveName}"".");
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
            var assemblyPath = _options.GetAssemblyReferencePath(assemblyName.Name);

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
                    if (loadedAssemblyName.Version == assemblyName.Version
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
            if (!_isDebugMode)
            {
                result.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, string.Empty));
            }
            return result;
        }

        private static CodeMemberMethod CreateDebugMethod()
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
                Name = DebugMethodName,
                //// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags - Microsoft design
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
                        new CodeStatement[] { debuggerBreakStatement },
                        new CodeStatement[] { debuggerLaunchStatement })
                }
            };
        }

        private CodeMemberMethod CreateUserCodeWrapperMethod(
            CodeTypeDeclaration declaringType,
            string offsetWarningId)
        {
            #region Argument Check

            if (declaringType == null)
            {
                throw new ArgumentNullException(nameof(declaringType));
            }

            #endregion

            var formattedScriptLinesString = string.Join(
                Environment.NewLine,
                _scriptLines.Select(line => UserCodeIndentation + line));

            var userCodeSnippetStatement = new CodeSnippetStatement(
                string.Format(
                    "#warning [For internal purposes] {1}{0}"
                        + "{2}{0}"
                        + "{0}"
                        + "{3}; return null; // Auto-generated",
                    Environment.NewLine,
                    offsetWarningId,
                    formattedScriptLinesString,
                    UserCodeIndentation));

            var result = new CodeMemberMethod
            {
                Name = WrapperMethodName,
                //// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags - Microsoft design
                Attributes = MemberAttributes.Assembly | MemberAttributes.Static,
                ReturnType = new CodeTypeReference(typeof(object)),
                Parameters =
                {
                    new CodeParameterDeclarationExpression(typeof(string[]), WrapperMethodParameterName)
                    {
                        CustomAttributes =
                        {
                            new CodeAttributeDeclaration(new CodeTypeReference(typeof(ParamArrayAttribute)))
                        }
                    }
                }
            };

            if (_isDebugMode)
            {
                result.Statements.Add(
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                null,
                                DebugMethodName)))
                    {
                        StartDirectives =
                        {
                            new CodeRegionDirective(
                                CodeRegionMode.Start,
                                $@"This statement is generated by {GetType().FullName} since the debug mode is active.")
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
            var isDebuggable = Debugger.IsAttached || _isDebugMode;

            var compilerParameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = !isDebuggable,
                IncludeDebugInformation = isDebuggable,
                TreatWarningsAsErrors = false,

                // TODO: Implement `unsafe` option
                CompilerOptions = $@"/unsafe- /optimize{(isDebuggable ? "-" : "+")}"
            };

            if (isDebuggable)
            {
                compilerParameters.OutputAssembly = Path.Combine(
                    Path.GetTempPath(),
                    $@"{typeof(ScriptExecutor).Name}__{DateTime.Now:yyyy'-'MM'-'dd__HH'-'mm'-'ss}__{
                        _scriptId.GetHashCode().ToString("X8").ToLowerInvariant()}.dll");
            }

            compilerParameters.ReferencedAssemblies.AddRange(_options.GetAllAssemblyReferencePaths());

            Func<string> generateRandomId = () => Guid.NewGuid().ToString("N");
            var offsetWarningId = string.Join(string.Empty, Enumerable.Range(0, 4).Select(i => generateRandomId()));

            var wrapperCodeType = new CodeTypeDeclaration(WrapperTypeName)
            {
                Attributes = MemberAttributes.Public,
                TypeAttributes = TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.NotPublic
            };

            var predefinedConstructor = CreatePredefinedConstructor();
            var userCodeWrapperMethod = CreateUserCodeWrapperMethod(wrapperCodeType, offsetWarningId);

            wrapperCodeType.Members.Add(predefinedConstructor);
            if (_isDebugMode)
            {
                var debugMethod = CreateDebugMethod();
                wrapperCodeType.Members.Add(debugMethod);
            }
            wrapperCodeType.Members.Add(userCodeWrapperMethod);

            var rootNamespace = new CodeNamespace(string.Empty);
            rootNamespace.Imports.AddRange(
                _options
                    .NamespaceImports
                    .Select(item => new CodeNamespaceImport(item))
                    .ToArray());
            rootNamespace.Types.Add(wrapperCodeType);

            var compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(rootNamespace);

            using (var codeProvider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider())
            {
                var generatedCodeBuilder = new StringBuilder();
                using (var sw = new StringWriter(generatedCodeBuilder))
                {
                    var options = new CodeGeneratorOptions
                    {
                        BlankLinesBetweenMembers = true,
                        BracingStyle = BracingStyle,
                        ElseOnClosing = false,
                        IndentString = IndentString,
                        VerbatimOrder = true
                    };
                    codeProvider.GenerateCodeFromCompileUnit(compileUnit, sw, options);
                }

                var generatedCode = generatedCodeBuilder.ToString();

                CompilerResults compilerResults;
                if (isDebuggable)
                {
                    var filePath = Path.ChangeExtension(compilerParameters.OutputAssembly, SourceFileExtensionValue);
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
                        .SingleOrDefault(item => item.IsWarning && item.ErrorText.Contains(offsetWarningId));

                    var sourceCodeLineOffset = offsetWarning?.Line ?? 0;

                    _executionResult = ScriptExecutionResult.CreateError(
                        ScriptExecutionResultType.CompilationError,
                        new ScriptExecutorException("Error compiling script"),
                        _script,
                        generatedCode,
                        string.Empty,
                        string.Empty,
                        onlyErrors,
                        sourceCodeLineOffset);
                    return;
                }

                var outputAssembly = compilerParameters.OutputAssembly;
                if (!string.IsNullOrEmpty(outputAssembly))
                {
                    var files = Directory.GetFiles(
                        Path.GetDirectoryName(outputAssembly).EnsureNotNull(),
                        Path.ChangeExtension(Path.GetFileName(outputAssembly), ".*"),
                        SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        _tempFiles.Add(file);
                    }
                }

                var compiledWrapperType = compilerResults.CompiledAssembly.GetType(WrapperTypeName);
                if (compiledWrapperType == null)
                {
                    throw new ScriptExecutorException(
                        $@"Cannot obtain the wrapper type ""{WrapperTypeName}"".",
                        _script,
                        generatedCode);
                }

                var compiledPredefinedConstructor = compiledWrapperType.GetConstructor(
                    PredefinedConstructorBindingFlags,
                    null,
                    Type.EmptyTypes,
                    null);
                if (compiledPredefinedConstructor == null)
                {
                    throw new ScriptExecutorException(
                        $@"Cannot obtain the predefined parameterless constructor in the type ""{WrapperTypeName}"".",
                        _script,
                        generatedCode);
                }

                MethodInfo compiledDebugMethod = null;
                if (_isDebugMode)
                {
                    compiledDebugMethod = compiledWrapperType.GetMethod(
                        DebugMethodName,
                        PredefinedMembersBindingFlags);
                    if (compiledDebugMethod == null)
                    {
                        throw new ScriptExecutorException(
                            $@"Cannot obtain the debug method ""{DebugMethodName}"" in the type ""{WrapperTypeName}"".",
                            _script,
                            generatedCode);
                    }
                }

                var compiledWrapperMethod = compiledWrapperType.GetMethod(
                    WrapperMethodName,
                    PredefinedMembersBindingFlags);
                if (compiledWrapperMethod == null)
                {
                    throw new ScriptExecutorException(
                        $@"Cannot obtain the wrapper method ""{WrapperMethodName}"" in the type ""{WrapperTypeName}"".",
                        _script,
                        generatedCode);
                }

                var allDeclaredMembers = compiledWrapperType.GetMembers(AllDeclaredMembersBindingFlags).ToList();
                var unexpectedMembers = allDeclaredMembers
                    .Where(
                        item => item != compiledWrapperMethod && item != compiledPredefinedConstructor
                            && item != compiledDebugMethod && item.Name.IndexOfAny(SpecialNameChars) < 0)
                    .ToList();
                if (unexpectedMembers.Any())
                {
                    throw new ScriptExecutorException(
                        "The script must not contain any members and must be just a code snippet.",
                        _script,
                        generatedCode);
                }

                var methodDelegate = (Func<string[], object>)Delegate.CreateDelegate(
                    typeof(Func<string[], object>),
                    compiledWrapperMethod,
                    true);
                if (methodDelegate == null)
                {
                    throw new ScriptExecutorException(
                        $@"Cannot create a delegate from the wrapper method ""{
                            compiledWrapperMethod.DeclaringType.EnsureNotNull().FullName}{Type.Delimiter}{
                            compiledWrapperMethod.Name}"".",
                        _script,
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
                            scriptReturnValue = methodDelegate(_arguments);
                        }
                        catch (Exception ex)
                        {
                            _executionResult = ScriptExecutionResult.CreateError(
                                ScriptExecutionResultType.ExecutionError,
                                ex,
                                _script,
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
                    _executionResult = ScriptExecutionResult.CreateError(
                        ScriptExecutionResultType.InternalError,
                        ex,
                        _script,
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

                _executionResult = ScriptExecutionResult.CreateSuccess(
                    scriptReturnValue,
                    _script,
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
            get
            {
                return _executionResult;
            }
        }

        #endregion

        #region Internal Methods

        internal void ExecuteInternal()
        {
            if (_executionResult != null)
            {
                throw new InvalidOperationException($@"'{GetType().FullName}' cannot be reused.");
            }

            try
            {
                GenerateAndRunScript();
            }
            catch (Exception ex)
            {
                _executionResult = ScriptExecutionResult.CreateError(
                    ScriptExecutionResultType.InternalError,
                    ex,
                    _script,
                    null,
                    null,
                    null,
                    null,
                    null);
            }

            if (_executionResult == null)
            {
                throw new InvalidOperationException("Execution result is not assigned after execution.");
            }
        }

        internal TemporaryFileList GetTemporaryFiles() => _tempFiles.Copy();

        #endregion

        #region Public Properties

        public static string SourceFileExtension
        {
            [DebuggerStepThrough]
            get
            {
                return SourceFileExtensionValue;
            }
        }

        public static string ScriptFileExtension
        {
            [DebuggerStepThrough]
            get
            {
                return ScriptFileExtensionValue;
            }
        }

        public string Script
        {
            [DebuggerStepThrough]
            get
            {
                return _script;
            }
        }

        #endregion

        #region Public Methods

        public static IScriptExecutor Create(ScriptExecutorParameters parameters)
        {
            #region Argument Check

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            #endregion

            ScriptExecutorProxy result;

            var scriptId = Guid.NewGuid();
            var domainName = $@"{typeof(ScriptExecutor).Name}_Domain_{scriptId:N}";
            var domain = AppDomain.CreateDomain(domainName);
            try
            {
                var scriptExecutorType = typeof(ScriptExecutor);

                var scriptExecutor = (ScriptExecutor)domain.CreateInstanceAndUnwrap(
                    scriptExecutorType.Assembly.FullName,
                    scriptExecutorType.FullName,
                    false,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new object[] { scriptId, domain, parameters },
                    null,
                    null);

                result = new ScriptExecutorProxy(scriptId, domain, scriptExecutor);
            }
            catch (Exception)
            {
                AppDomain.Unload(domain);
                throw;
            }

            return result;
        }

        public override object InitializeLifetimeService() => null;

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

        #region ScriptOptions Class

        private sealed class ScriptOptions
        {
            #region Fields

            private readonly Dictionary<string, string> _assemblyReferences;

            #endregion

            #region Constructors

            internal ScriptOptions()
            {
                _assemblyReferences = PredefinedReferences
                    .Select(
                        item => new
                        {
                            Name = Path.GetFileNameWithoutExtension(item),
                            Path = item
                        })
                    .ToDictionary(item => item.Name, item => item.Path, StringComparer.OrdinalIgnoreCase);

                NamespaceImports = new HashSet<string>(PredefinedImports); // Case-sensitive
            }

            #endregion

            #region Public Properties

            public HashSet<string> NamespaceImports
            {
                get;
            }

            #endregion

            #region Public Methods

            public void AddAssemblyReferencePath(string assemblyPath)
            {
                #region Argument Check

                if (string.IsNullOrEmpty(assemblyPath))
                {
                    throw new ArgumentException(
                        "The value can be neither empty string nor null.",
                        nameof(assemblyPath));
                }

                #endregion

                var name = Path.GetFileNameWithoutExtension(assemblyPath);
                _assemblyReferences[name] = assemblyPath;
            }

            public string[] GetAllAssemblyReferencePaths() => _assemblyReferences.Values.ToArray();

            public string GetAssemblyReferencePath(string name)
            {
                #region Argument Check

                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("The value can be neither empty string nor null.", nameof(name));
                }

                #endregion

                return _assemblyReferences.GetValueOrDefault(name);
            }

            #endregion
        }

        #endregion
    }
}