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

        private static readonly ScriptExecutionResult s_success = new ScriptExecutionResult(
            ScriptExecutionResultType.Success,
            null);

        private readonly ScriptExecutionResultType m_type;
        private readonly string m_message;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptExecutionResult"/> class.
        /// </summary>
        internal ScriptExecutionResult(ScriptExecutionResultType type, string message)
        {
            m_type = type;
            m_message = message;
        }

        #endregion

        #region Internal Methods

        internal static ScriptExecutionResult Create(ScriptExecutionResultType type, Exception exception)
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

            return new ScriptExecutionResult(type, exception.ToString());
        }

        #endregion

        #region Public Properties

        public static ScriptExecutionResult Success
        {
            [DebuggerStepThrough]
            get { return s_success; }
        }

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

        #endregion
    }
}