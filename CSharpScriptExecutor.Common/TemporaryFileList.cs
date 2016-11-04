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
        private readonly object _syncLock = new object();
        private readonly HashSet<string> _filePaths;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemporaryFileList"/> class.
        /// </summary>
        internal TemporaryFileList()
            : this(new string[0])
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemporaryFileList"/> class.
        /// </summary>
        private TemporaryFileList(ICollection<string> filePaths)
        {
            if (filePaths == null)
            {
                throw new ArgumentNullException(nameof(filePaths));
            }

            if (filePaths.Contains(null))
            {
                throw new ArgumentException("The collection contains a null element.", nameof(filePaths));
            }

            _filePaths = new HashSet<string>(filePaths, StringComparer.OrdinalIgnoreCase);
        }

        public object SyncLock
        {
            [DebuggerStepThrough]
            get
            {
                return _syncLock;
            }
        }

        public void Add(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("The value can be neither empty string nor null.", nameof(filePath));
            }

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

        public TemporaryFileList Copy() => new TemporaryFileList(_filePaths);

        public void Dispose() => Delete();
    }
}