using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    public sealed class ScriptExecutorParameters
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptExecutorParameters"/> class.
        /// </summary>
        public ScriptExecutorParameters(
            string script,
            ICollection<string> scriptArguments,
            bool isDebugMode)
        {
            #region Argument Check

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

            #endregion

            Script = script;
            ScriptArguments = new List<string>(scriptArguments).AsReadOnly();
            IsDebugMode = isDebugMode;
        }

        #endregion

        #region Public Properties

        public string Script
        {
            get;
            private set;
        }

        public IList<string> ScriptArguments
        {
            get;
            private set;
        }

        public bool IsDebugMode
        {
            get;
            private set;
        }

        #endregion
    }
}