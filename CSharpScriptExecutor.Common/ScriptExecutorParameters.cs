using System;
using System.Collections;
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
            string scriptFilePath,
            IEnumerable<string> scriptArguments,
            bool isDebugMode)
        {
            #region Argument Check

            if (string.IsNullOrEmpty(scriptFilePath))
            {
                throw new ArgumentException("The value can be neither empty string nor null.", "scriptFilePath");
            }
            if (scriptArguments == null)
            {
                throw new ArgumentNullException("scriptArguments");
            }
            if (scriptArguments.Contains(null))
            {
                throw new ArgumentException("The collection contains a null element.", "scriptArguments");
            }

            #endregion

            this.ScriptFilePath = scriptFilePath;
            this.ScriptArguments = new List<string>(scriptArguments).AsReadOnly();
            this.IsDebugMode = isDebugMode;
        }

        #endregion

        #region Public Properties

        public string ScriptFilePath
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