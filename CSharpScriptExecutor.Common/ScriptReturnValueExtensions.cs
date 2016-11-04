using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    public static class ScriptReturnValueExtensions
    {
        #region Public Methods

        public static bool IsNull(this IScriptReturnValue value) => value == null || value.IsNull;

        #endregion
    }
}