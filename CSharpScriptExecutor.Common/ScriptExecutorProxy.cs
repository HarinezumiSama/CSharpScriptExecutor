using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CSharpScriptExecutor.Common
{
    /// <summary>
    ///     ScriptExecutorProxy class.
    /// </summary>
    public sealed class ScriptExecutorProxy : IScriptExecutor
    {
        #region Fields

        [ThreadStatic]
        private static TempFileCollection s_tempFiles;

        private readonly Guid m_scriptId;
        private AppDomain m_domain;
        private ScriptExecutor m_scriptExecutor;

        private ScriptExecutionResult m_executionResult;
        private bool m_isDisposed;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptExecutorProxy"/> class.
        /// </summary>
        private ScriptExecutorProxy(Guid scriptId, AppDomain domain, ScriptExecutor scriptExecutor)
        {
            #region Argument Check
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            if (scriptExecutor == null)
            {
                throw new ArgumentNullException("scriptExecutor");
            }
            #endregion

            m_scriptId = scriptId;
            m_domain = domain;
            m_scriptExecutor = scriptExecutor;
        }

        #endregion

        #region Private Methods

        private void EnsureNotDisposed()
        {
            if (m_isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        #endregion

        #region Public Properties

        public static TempFileCollection TempFiles
        {
            [DebuggerNonUserCode]
            get
            {
                if (s_tempFiles == null)
                {
                    s_tempFiles = new TempFileCollection();
                }

                return s_tempFiles;
            }
        }

        #endregion

        #region Public Methods

        public static IScriptExecutor Create(ScriptExecutorParameters parameters)
        {
            #region Argument Check

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            #endregion

            ScriptExecutorProxy result;

            Guid scriptId = Guid.NewGuid();
            AppDomain domain = AppDomain.CreateDomain(string.Format(
                "{0}_Domain_{1:N}",
                typeof(ScriptExecutor).Name,
                scriptId));
            try
            {
                ScriptExecutor scriptExecutor = ScriptExecutor.Create(scriptId, domain, parameters);
                result = new ScriptExecutorProxy(scriptId, domain, scriptExecutor);
            }
            catch (Exception)
            {
                AppDomain.Unload(domain);
                throw;
            }

            return result;
        }

        #endregion

        #region IScriptExecutor Members

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ScriptExecutionResult Execute()
        {
            EnsureNotDisposed();

            if (m_executionResult != null)
            {
                throw new InvalidOperationException("The script has been already executed by the current executor.");
            }

            try
            {
                m_domain.DoCallBack(m_scriptExecutor.ExecuteInternal);
                m_executionResult = m_scriptExecutor.ExecutionResult;
            }
            catch (Exception ex)
            {
                m_executionResult = ScriptExecutionResult.Create(ScriptExecutionResultType.InternalError, ex);
            }

            if (m_executionResult == null)
            {
                throw new InvalidOperationException("Execution result is not assigned after execution.");
            }

            return m_executionResult;
        }

        #endregion

        #region IDisposable Members

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            if (m_isDisposed)
            {
                return;
            }

            m_scriptExecutor = null;
            AppDomain.Unload(m_domain);
            m_domain = null;

            // Finally, setting the flag
            m_isDisposed = true;
        }

        #endregion
    }
}