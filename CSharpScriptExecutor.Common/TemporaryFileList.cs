using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    internal sealed class TemporaryFileList : IDisposable
    {
        #region Fields

        private readonly object m_syncLock = new object();
        private readonly HashSet<string> m_filePaths;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemporaryFileList"/> class.
        /// </summary>
        private TemporaryFileList(IEnumerable<string> filePaths)
        {
            #region Argument Check

            if (filePaths == null)
            {
                throw new ArgumentNullException("filePaths");
            }
            if (filePaths.Contains(null))
            {
                throw new ArgumentException("The collection contains a null element.", "filePaths");
            }

            #endregion

            m_filePaths = new HashSet<string>(filePaths, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemporaryFileList"/> class.
        /// </summary>
        internal TemporaryFileList()
            : this(Enumerable.Empty<string>())
        {
            // Nothing to do
        }

        #endregion

        #region Public Properties

        public object SyncLock
        {
            [DebuggerStepThrough]
            get { return m_syncLock; }
        }

        #endregion

        #region Public Methods

        public void Add(string filePath)
        {
            #region Argument Check

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("The value can be neither empty string nor null.", "filePath");
            }

            #endregion

            lock (this.SyncLock)
            {
                m_filePaths.Add(filePath);
            }
        }

        public void Delete()
        {
            lock (this.SyncLock)
            {
                var filePaths = m_filePaths.ToList();
                m_filePaths.Clear();

                foreach (var filePath in filePaths)
                {
                    try
                    {
                        if (!File.Exists(filePath))
                        {
                            continue;
                        }

                        File.SetAttributes(filePath, FileAttributes.Normal);
                        File.Delete(filePath);
                    }
                    catch (Exception)
                    {
                        m_filePaths.Add(filePath);
                    }
                }
            }
        }

        public TemporaryFileList Copy()
        {
            return new TemporaryFileList(m_filePaths);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Delete();
        }

        #endregion
    }
}