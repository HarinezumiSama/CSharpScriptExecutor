using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CSharpScriptExecutor
{
    public static class WinApi
    {
        public static class Messages
        {
            public const uint WmCopydata = 0x004A;
            public const uint WmDropfiles = 0x0233;
            public const uint WmDropFilesInternalRelated = 0x0049;
        }

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
        public struct ChangeFilterStruct
        {
            public uint Size;
            public MessageFilterInfo Info;
        }

        private const string Kernel32 = "kernel32.dll";

        public const uint AttachParentProcess = 0xFFFFFFFF;

        public static void AssertWinApiResult(this bool callResult)
        {
            if (!callResult)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        [DllImport(Kernel32, SetLastError = true)]
        public static extern bool AllocConsole();

        [DllImport(Kernel32, SetLastError = true, ExactSpelling = true)]
        public static extern bool FreeConsole();

        [DllImport(Kernel32, SetLastError = true)]
        public static extern bool AttachConsole(uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern unsafe bool ChangeWindowMessageFilterEx(
            IntPtr windowHandle,
            uint msg,
            ChangeWindowMessageFilterExAction action,
            ChangeFilterStruct* changeInfo);

        public static unsafe bool ChangeWindowMessageFilterEx(
            IntPtr windowHandle,
            uint msg,
            ChangeWindowMessageFilterExAction action)
        {
            return ChangeWindowMessageFilterEx(windowHandle, msg, action, null);
        }
    }
}