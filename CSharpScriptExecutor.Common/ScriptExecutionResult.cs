using System;
using System.CodeDom.Compiler;
using System.Collections;
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

        private static readonly IList<CompilerError> s_emptyCompilerErrors = new List<CompilerError>().AsReadOnly();

        private readonly ScriptExecutionResultType m_type;
        private readonly string m_message;
        private readonly string m_consoleOut;
        private readonly string m_consoleError;
        private readonly string m_sourceCode;
        private readonly string m_generatedCode;
        private readonly IList<CompilerError> m_compilerErrors;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptExecutionResult"/> class.
        /// </summary>
        private ScriptExecutionResult(
            ScriptExecutionResultType type,
            string message,
            string consoleOut,
            string consoleError,
            string sourceCode,
            string generatedCode,
            IEnumerable<CompilerError> compilerErrors)
        {
            #region Argument Check

            if (consoleOut == null)
            {
                throw new ArgumentNullException("consoleOut");
            }
            if (consoleError == null)
            {
                throw new ArgumentNullException("consoleError");
            }
            if (type == ScriptExecutionResultType.CompilationError)
            {
                if (compilerErrors == null)
                {
                    throw new ArgumentNullException("compilerErrors");
                }
                if (!compilerErrors.Any())
                {
                    throw new ArgumentException(
                        "There must be at least one error in the collection.",
                        "compilerErrors");
                }
                if (compilerErrors.Contains(null))
                {
                    throw new ArgumentException("The collection contains a null element.", "compilerErrors");
                }
                if (compilerErrors.Any(item => item.IsWarning))
                {
                    throw new ArgumentException("The collection must contain errors only.", "compilerErrors");
                }
            }
            else
            {
                if (compilerErrors != null)
                {
                    throw new ArgumentException("The value must be null.", "compilerErrors");
                }
            }

            #endregion

            m_type = type;
            m_message = message;
            m_consoleOut = consoleOut;
            m_consoleError = consoleError;
            m_sourceCode = sourceCode ?? string.Empty;
            m_generatedCode = generatedCode ?? string.Empty;
            m_compilerErrors = compilerErrors == null ? s_emptyCompilerErrors : compilerErrors.ToList().AsReadOnly();
        }

        #endregion

        #region Internal Methods

        internal static ScriptExecutionResult CreateError(
            ScriptExecutionResultType type,
            Exception exception,
            string consoleOut,
            string consoleError,
            string sourceCode,
            string generatedSource,
            IEnumerable<CompilerError> compilerErrors)
        {
            #region Argument Check

            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            if (type == ScriptExecutionResultType.Success)
            {
                throw new ArgumentException("Cannot create an error result from the success.", "type");
            }

            #endregion

            if ((exception is TargetInvocationException) && (exception.InnerException != null))
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
                exception.ToString(),
                consoleOut,
                consoleError,
                sourceCode,
                generatedSource,
                compilerErrors);
        }

        internal static ScriptExecutionResult CreateSuccess(
            string consoleOut,
            string consoleError,
            string sourceCode,
            string generatedSource)
        {
            return new ScriptExecutionResult(
                ScriptExecutionResultType.Success,
                null,
                consoleOut,
                consoleError,
                sourceCode,
                generatedSource,
                null);
        }

        #endregion

        #region Public Properties

        public ScriptExecutionResultType Type
        {
            [DebuggerStepThrough]
            get { return m_type; }
        }

        public bool IsSuccess
        {
            [DebuggerStepThrough]
            get { return m_type == ScriptExecutionResultType.Success; }
        }

        public string Message
        {
            [DebuggerStepThrough]
            get { return m_message; }
        }

        public string ConsoleOut
        {
            [DebuggerStepThrough]
            get { return m_consoleOut; }
        }

        public string ConsoleError
        {
            [DebuggerStepThrough]
            get { return m_consoleError; }
        }

        /// <summary>
        ///     Gets the source code of a user.
        /// </summary>
        public string SourceCode
        {
            [DebuggerStepThrough]
            get { return m_sourceCode; }
        }

        /// <summary>
        ///     Gets the full code generated from the source script.
        /// </summary>
        public string GeneratedCode
        {
            [DebuggerStepThrough]
            get { return m_generatedCode; }
        }

        public IList<CompilerError> CompilerErrors
        {
            [DebuggerStepThrough]
            get { return m_compilerErrors; }
        }

        #endregion

        #region Public Methods

        public static ScriptExecutionResult CreateInternalError(
            Exception exception,
            string consoleOut,
            string consoleError,
            string sourceCode,
            string generatedSource)
        {
            #region Argument Check

            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            #endregion

            if ((exception is TargetInvocationException) && (exception.InnerException != null))
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
                ScriptExecutionResultType.InternalError,
                exception.ToString(),
                consoleOut,
                consoleError,
                sourceCode,
                generatedSource,
                null);
        }

        #endregion
    }
}