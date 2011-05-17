using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    public static class ScriptReturnValueExtensions
    {
        #region Public Methods

        public static bool IsNull(this ScriptReturnValue value)
        {
            return value == null || value.IsNull;
        }

        #endregion
    }
}