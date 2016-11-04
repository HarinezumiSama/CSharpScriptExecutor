using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    public static class ScriptReturnValueExtensions
    {
        public static bool IsNull(this IScriptReturnValue value) => value == null || value.IsNull;
    }
}