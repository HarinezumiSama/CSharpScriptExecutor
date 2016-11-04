using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    public sealed class ScriptExecutorParameters
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptExecutorParameters"/> class.
        /// </summary>
        public ScriptExecutorParameters(
            string script,
            ICollection<string> scriptArguments,
            bool isDebugMode)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                throw new ArgumentException("The value can be neither empty string nor null.", nameof(script));
            }

            if (scriptArguments == null)
            {
                throw new ArgumentNullException(nameof(scriptArguments));
            }

            if (scriptArguments.Any(item => item == null))
            {
                throw new ArgumentException(@"The collection contains a null element.", nameof(scriptArguments));
            }

            Script = script;
            ScriptArguments = new ReadOnlyCollection<string>(scriptArguments.ToArray());
            IsDebugMode = isDebugMode;
        }

        public string Script
        {
            get;
            private set;
        }

        public ReadOnlyCollection<string> ScriptArguments
        {
            get;
            private set;
        }

        public bool IsDebugMode
        {
            get;
            private set;
        }
    }
}