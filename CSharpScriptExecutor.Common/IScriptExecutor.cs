using System;

namespace CSharpScriptExecutor.Common
{
    public interface IScriptExecutor : IDisposable
    {
        #region Methods

        ScriptExecutionResult Execute();

        #endregion
    }
}