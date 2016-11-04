using System;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    public enum ScriptExecutionResultType
    {
        InternalError,
        CompilationError,
        ExecutionError,
        Success
    }
}