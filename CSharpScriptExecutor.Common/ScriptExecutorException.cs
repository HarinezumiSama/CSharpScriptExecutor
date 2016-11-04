using System;
using System.Runtime.Serialization;
using System.Text;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    public sealed class ScriptExecutorException : Exception
    {
        internal ScriptExecutorException(string message, string sourceCode = null, string generatedCode = null)
            : base(message)
        {
            SourceCode = sourceCode ?? string.Empty;
            GeneratedCode = generatedCode ?? string.Empty;
        }

        private ScriptExecutorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Gets the source code of a user.
        /// </summary>
        public string SourceCode
        {
            get;
        }

        /// <summary>
        ///     Gets the full code generated from the source script.
        /// </summary>
        public string GeneratedCode
        {
            get;
        }

        public override string ToString()
        {
            var resultBuilder = new StringBuilder(base.ToString());
            if (!string.IsNullOrEmpty(SourceCode))
            {
                resultBuilder.AppendLine();
                resultBuilder.AppendLine("Source code:");
                resultBuilder.AppendLine(SourceCode);
            }

            if (!string.IsNullOrEmpty(GeneratedCode))
            {
                resultBuilder.AppendLine();
                resultBuilder.AppendLine("Generated source:");
                resultBuilder.AppendLine(GeneratedCode);
            }

            return resultBuilder.ToString();
        }
    }
}