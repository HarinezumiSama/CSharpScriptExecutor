using System;
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

        private readonly object _syncLock = new object();
        private readonly HashSet<string> _filePaths;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemporaryFileList"/> class.
        /// </summary>
        private TemporaryFileList(ICollection<string> filePaths)
        {
            #region Argument Check

            if (filePaths == null)
            {
                throw new ArgumentNullException(nameof(filePaths));
            }

            if (filePaths.Contains(null))
            {
                throw new ArgumentException("The collection contains a null element.", nameof(filePaths));
            }

            #endregion

            _filePaths = new HashSet<string>(filePaths, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemporaryFileList"/> class.
        /// </summary>
        internal TemporaryFileList()
            : this(new string[0])
        {
            // Nothing to do
        }

        #endregion

        #region Public Properties

        public object SyncLock
        {
            [DebuggerStepThrough]
            get
            {
                return _syncLock;
            }
        }

        #endregion

        #region Public Methods

        public void Add(string filePath)
        {
            #region Argument Check

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("The value can be neither empty string nor null.", nameof(filePath));
            }

            #endregion

            lock (SyncLock)
            {
                _filePaths.Add(filePath);
            }
        }

        public void Delete()
        {
            lock (SyncLock)
            {
                var filePaths = _filePaths.ToList();
                _filePaths.Clear();

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
                        _filePaths.Add(filePath);
                    }
                }
            }
        }

        public TemporaryFileList Copy()
        {
            return new TemporaryFileList(_filePaths);
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