using System;
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

        private readonly ScriptExecutionResultType m_type;
        private readonly string m_message;
        private readonly string m_consoleOut;
        private readonly string m_consoleError;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptExecutionResult"/> class.
        /// </summary>
        internal ScriptExecutionResult(
            ScriptExecutionResultType type,
            string message,
            string consoleOut,
            string consoleError)
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

            #endregion

            m_type = type;
            m_message = message;
            m_consoleOut = consoleOut;
            m_consoleError = consoleError;
        }

        #endregion

        #region Internal Methods

        internal static ScriptExecutionResult CreateError(
            ScriptExecutionResultType type,
            Exception exception,
            string consoleOut,
            string consoleError)
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

            return new ScriptExecutionResult(type, exception.ToString(), consoleOut, consoleError);
        }

        internal static ScriptExecutionResult CreateSuccess(string consoleOut, string consoleError)
        {
            return new ScriptExecutionResult(ScriptExecutionResultType.Success, null, consoleOut, consoleError);
        }

        #endregion

        #region Public Properties

        public ScriptExecutionResultType Type
        {
            [DebuggerStepThrough]
            get { return m_type; }
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

        #endregion
    }
}