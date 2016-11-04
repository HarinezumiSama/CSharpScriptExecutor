using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CSharpScriptExecutor.Common;

namespace CSharpScriptExecutor
{
    internal static class LocalHelper
    {
        [DebuggerNonUserCode]
        public static string TryLoadScript(string scriptFilePath)
        {
            if (string.IsNullOrEmpty(scriptFilePath))
            {
                return string.Empty;
            }

            try
            {
                if (!File.Exists(scriptFilePath))
                {
                    return string.Empty;
                }

                return File.ReadAllText(scriptFilePath);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        [DebuggerNonUserCode]
        public static bool TrySaveScript(string scriptFilePath, string script)
        {
            if (string.IsNullOrEmpty(scriptFilePath))
            {
                return false;
            }

            try
            {
                var directory = Path.GetDirectoryName(scriptFilePath).EnsureNotNull();
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var lastScriptFile = new FileInfo(scriptFilePath);
                if (lastScriptFile.Exists)
                {
                    lastScriptFile.Attributes = FileAttributes.Normal;
                }

                if (string.IsNullOrWhiteSpace(script))
                {
                    lastScriptFile.Refresh();
                    if (lastScriptFile.Exists)
                    {
                        lastScriptFile.Delete();
                    }

                    return true;
                }

                File.WriteAllText(scriptFilePath, script, Encoding.UTF8);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}