using System;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    public enum ScriptExecutionResultType
    {
        InternalError,
        CompileError,
        ExecutionError,
        Success
    }
}