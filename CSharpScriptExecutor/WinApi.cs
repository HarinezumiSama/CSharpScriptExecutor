using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace CSharpScriptExecutor
{
    public static class WinApi
    {
        #region Nested Types

        #region Messages

        public static class Messages
        {
            #region Constants

            public const uint WM_COPYDATA = 0x004A;
            public const uint WM_DROPFILES = 0x0233;
            public const uint WM_DropFilesInternalRelated = 0x0049;

            #endregion
        }

        #endregion

        public enum MessageFilterInfo : uint
        {
            None = 0,
            AlreadyAllowed = 1,
            AlreadyDisAllowed = 2,
            AllowedHigher = 3
        }

        public enum ChangeWindowMessageFilterExAction : uint
        {
            Reset = 0,
            Allow = 1,
            Disallow = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CHANGEFILTERSTRUCT
        {
            public uint size;
            public MessageFilterInfo info;
        }

        #endregion

        #region Constants

        private const string c_kernel32 = "kernel32.dll";

        public const UInt32 ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

        #endregion

        #region Fields

        #endregion

        #region Public Methods

        public static void AssertWinApiResult(this bool callResult)
        {
            if (!callResult)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        [DllImport(c_kernel32, SetLastError = true)]
        public static extern bool AllocConsole();

        [DllImport(c_kernel32, SetLastError = true, ExactSpelling = true)]
        public static extern bool FreeConsole();

        [DllImport(c_kernel32, SetLastError = true)]
        public static extern bool AttachConsole(UInt32 dwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern unsafe bool ChangeWindowMessageFilterEx(
            IntPtr hWnd,
            uint msg,
            ChangeWindowMessageFilterExAction action,
            CHANGEFILTERSTRUCT* changeInfo);

        public static unsafe bool ChangeWindowMessageFilterEx(
            IntPtr hWnd,
            uint msg,
            ChangeWindowMessageFilterExAction action)
        {
            return ChangeWindowMessageFilterEx(hWnd, msg, action, null);
        }

        #endregion
    }
}