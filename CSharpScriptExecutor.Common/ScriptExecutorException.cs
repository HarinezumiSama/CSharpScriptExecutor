using System;
using System.Runtime.Serialization;
using System.Text;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    public sealed class ScriptExecutorException : Exception
    {
        #region Constructors

        internal ScriptExecutorException(string message, string sourceCode = null, string generatedCode = null)
            : base(message)
        {
            this.SourceCode = sourceCode ?? string.Empty;
            this.GeneratedCode = generatedCode ?? string.Empty;
        }

        private ScriptExecutorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Nothing to do
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the source code of a user.
        /// </summary>
        public string SourceCode
        {
            get;
            private set;
        }

        /// <summary>
        ///     Gets the full code generated from the source script.
        /// </summary>
        public string GeneratedCode
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            var resultBuilder = new StringBuilder(base.ToString());
            if (!string.IsNullOrEmpty(this.SourceCode))
            {
                resultBuilder.AppendLine();
                resultBuilder.AppendLine("Source code:");
                resultBuilder.AppendLine(this.SourceCode);
            }
            if (!string.IsNullOrEmpty(this.GeneratedCode))
            {
                resultBuilder.AppendLine();
                resultBuilder.AppendLine("Generated source:");
                resultBuilder.AppendLine(this.GeneratedCode);
            }
            return resultBuilder.ToString();
        }

        #endregion
    }
}