using System;

namespace CSharpScriptExecutor.Common
{
    public interface IScriptExecutor : IDisposable
    {
        ScriptExecutionResult Execute();
    }
}