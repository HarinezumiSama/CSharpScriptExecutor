using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    public sealed class ScriptExecutionResult
    {
        #region Fields

        private static readonly IList<CompilerError> EmptyCompilerErrors = new List<CompilerError>().AsReadOnly();

        private readonly ScriptExecutionResultType _type;
        private readonly IScriptReturnValue _returnValue;
        private readonly string _message;
        private readonly string _consoleOut;
        private readonly string _consoleError;
        private readonly string _sourceCode;
        private readonly string _generatedCode;
        private readonly IList<CompilerError> _compilerErrors;
        private readonly int? _sourceCodeLineOffset;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptExecutionResult"/> class.
        /// </summary>
        private ScriptExecutionResult(
            ScriptExecutionResultType type,
            object returnValue,
            string message,
            string consoleOut,
            string consoleError,
            string sourceCode,
            string generatedCode,
            ICollection<CompilerError> compilerErrors,
            int? sourceCodeLineOffset)
        {
            #region Argument Check

            if (consoleOut == null)
            {
                throw new ArgumentNullException(nameof(consoleOut));
            }

            if (consoleError == null)
            {
                throw new ArgumentNullException(nameof(consoleError));
            }

            if (type != ScriptExecutionResultType.Success)
            {
                if (returnValue != null)
                {
                    throw new ArgumentException(
                        "Script could not return a value since it has failed to run.",
                        nameof(returnValue));
                }
            }

            if (type == ScriptExecutionResultType.CompilationError)
            {
                if (compilerErrors == null)
                {
                    throw new ArgumentNullException(nameof(compilerErrors));
                }
                if (!compilerErrors.Any())
                {
                    throw new ArgumentException(
                        "There must be at least one error in the collection.",
                        nameof(compilerErrors));
                }
                if (compilerErrors.Contains(null))
                {
                    throw new ArgumentException("The collection contains a null element.", nameof(compilerErrors));
                }
                if (compilerErrors.Any(item => item.IsWarning))
                {
                    throw new ArgumentException("The collection must contain errors only.", nameof(compilerErrors));
                }
                if (sourceCodeLineOffset == null)
                {
                    throw new ArgumentNullException(nameof(sourceCodeLineOffset));
                }
            }
            else
            {
                if (compilerErrors != null)
                {
                    throw new ArgumentException("The value must be null.", nameof(compilerErrors));
                }
            }

            #endregion

            _type = type;
            _returnValue = type == ScriptExecutionResultType.Success ? ScriptReturnValue.Create(returnValue) : null;
            _message = message;
            _consoleOut = consoleOut;
            _consoleError = consoleError;
            _sourceCode = sourceCode ?? string.Empty;
            _generatedCode = generatedCode ?? string.Empty;
            _compilerErrors = compilerErrors?.ToList().AsReadOnly() ?? EmptyCompilerErrors;
            _sourceCodeLineOffset = sourceCodeLineOffset;
        }

        #endregion

        #region Internal Methods

        internal static ScriptExecutionResult CreateError(
            ScriptExecutionResultType type,
            Exception exception,
            string sourceCode,
            string generatedSource,
            string consoleOut,
            string consoleError,
            ICollection<CompilerError> compilerErrors,
            int? sourceCodeLineOffset)
        {
            #region Argument Check

            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }
            if (type == ScriptExecutionResultType.Success)
            {
                throw new ArgumentException("Cannot create an error result from the success.", nameof(type));
            }

            #endregion

            if (exception is TargetInvocationException && (exception.InnerException != null))
            {
                exception = exception.InnerException;
            }

            var scriptExecutorException = exception as ScriptExecutorException;
            if (scriptExecutorException != null)
            {
                if (sourceCode == null)
                {
                    sourceCode = scriptExecutorException.SourceCode;
                }
                if (generatedSource == null)
                {
                    generatedSource = scriptExecutorException.GeneratedCode;
                }
            }

            return new ScriptExecutionResult(
                type,
                null,
                exception.ToString(),
                consoleOut,
                consoleError,
                sourceCode,
                generatedSource,
                compilerErrors,
                sourceCodeLineOffset);
        }

        internal static ScriptExecutionResult CreateSuccess(
            object returnValue,
            string sourceCode,
            string generatedSource,
            string consoleOut,
            string consoleError)
        {
            return new ScriptExecutionResult(
                ScriptExecutionResultType.Success,
                returnValue,
                null,
                consoleOut,
                consoleError,
                sourceCode,
                generatedSource,
                null,
                null);
        }

        #endregion

        #region Public Properties

        public ScriptExecutionResultType Type
        {
            [DebuggerStepThrough]
            get { return _type; }
        }

        public bool IsSuccess
        {
            [DebuggerStepThrough]
            get { return _type == ScriptExecutionResultType.Success; }
        }

        public IScriptReturnValue ReturnValue
        {
            [DebuggerStepThrough]
            get { return _returnValue; }
        }

        public string Message
        {
            [DebuggerStepThrough]
            get { return _message; }
        }

        public string ConsoleOut
        {
            [DebuggerStepThrough]
            get { return _consoleOut; }
        }

        public string ConsoleError
        {
            [DebuggerStepThrough]
            get { return _consoleError; }
        }

        /// <summary>
        ///     Gets the source code of a user.
        /// </summary>
        public string SourceCode
        {
            [DebuggerStepThrough]
            get { return _sourceCode; }
        }

        /// <summary>
        ///     Gets the code generated from the source script.
        /// </summary>
        public string GeneratedCode
        {
            [DebuggerStepThrough]
            get { return _generatedCode; }
        }

        public IList<CompilerError> CompilerErrors
        {
            [DebuggerStepThrough]
            get { return _compilerErrors; }
        }

        public int? SourceCodeLineOffset
        {
            [DebuggerStepThrough]
            get { return _sourceCodeLineOffset; }
        }

        #endregion

        #region Public Methods

        public static ScriptExecutionResult CreateInternalError(Exception exception, string sourceCode)
        {
            return CreateError(
                ScriptExecutionResultType.InternalError,
                exception,
                sourceCode,
                null,
                null,
                null,
                null,
                null);
        }

        #endregion
    }
}