using System;
using System.Runtime.Serialization;

namespace CSharpScriptExecutor.Common
{
    #region ScriptExecutorException Class

    [Serializable]
    public sealed class ScriptExecutorException : Exception
    {
        #region Constructors

        public ScriptExecutorException()
            : base()
        {
            // Nothing to do
        }

        public ScriptExecutorException(string message)
            : base(message)
        {
            // Nothing to do
        }

        public ScriptExecutorException(string message, Exception inner)
            : base(message, inner)
        {
            // Nothing to do
        }

        private ScriptExecutorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Nothing to do
        }

        #endregion
    }

    #endregion
}