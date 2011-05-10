using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace CSharpScriptExecutor
{
    public static class WinApi
    {
        #region Nested Types
        #endregion

        #region Constants

        private const string c_kernel32 = "kernel32.dll";

        public const UInt32 ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

        #endregion

        #region Fields
        #endregion

        #region Public Methods

        [DllImport(c_kernel32, SetLastError = true)]
        public static extern bool AllocConsole();

        [DllImport(c_kernel32, SetLastError = true, ExactSpelling = true)]
        public static extern bool FreeConsole();

        [DllImport(c_kernel32, SetLastError = true)]
        public static extern bool AttachConsole(UInt32 dwProcessId);

        #endregion
    }
}