using System;
using System.Runtime.CompilerServices;

namespace CSharpScriptExecutor.Common
{
    /// <summary>
    ///     ScriptExecutorProxy class.
    /// </summary>
    public sealed class ScriptExecutorProxy : IScriptExecutor
    {
        private AppDomain _domain;
        private ScriptExecutor _scriptExecutor;

        private ScriptExecutionResult _executionResult;
        private bool _isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptExecutorProxy"/> class.
        /// </summary>
        internal ScriptExecutorProxy(Guid scriptId, AppDomain domain, ScriptExecutor scriptExecutor)
        {
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            if (scriptExecutor == null)
            {
                throw new ArgumentNullException(nameof(scriptExecutor));
            }

            ScriptId = scriptId;
            _domain = domain;
            _scriptExecutor = scriptExecutor;
        }

        internal Guid ScriptId
        {
            get;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        ScriptExecutionResult IScriptExecutor.Execute()
        {
            EnsureNotDisposed();

            if (_executionResult != null)
            {
                throw new InvalidOperationException("The script has been already executed by the current executor.");
            }

            try
            {
                _domain.DoCallBack(_scriptExecutor.ExecuteInternal);
                _executionResult = _scriptExecutor.ExecutionResult;
            }
            catch (Exception ex)
            {
                _executionResult = ScriptExecutionResult.CreateError(
                    ScriptExecutionResultType.InternalError,
                    ex,
                    _scriptExecutor.Script,
                    null,
                    null,
                    null,
                    null,
                    null);
            }

            if (_executionResult == null)
            {
                throw new InvalidOperationException("Execution result is not assigned after execution.");
            }

            return _executionResult;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void IDisposable.Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            TemporaryFileList temporaryFiles = null;
            if (_scriptExecutor != null)
            {
                temporaryFiles = _scriptExecutor.GetTemporaryFiles();

                try
                {
                    _scriptExecutor.Dispose();
                }
                catch (Exception)
                {
                    // Nothing to do
                }

                _scriptExecutor = null;
            }

            if (_domain != null)
            {
                AppDomain.Unload(_domain);
                _domain = null;
            }

            // After the domain is unloaded we may try to delete the temporary files
            if (temporaryFiles != null)
            {
                temporaryFiles.Delete();
            }

            // Finally, setting the flag
            _isDisposed = true;
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}