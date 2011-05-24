using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    public static class IScriptReturnValueExtensions
    {
        #region Public Methods

        public static bool IsNull(this IScriptReturnValue value)
        {
            return value == null || value.IsNull;
        }

        #endregion
    }
}