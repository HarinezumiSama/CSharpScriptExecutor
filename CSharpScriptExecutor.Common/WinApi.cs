////using System;
////using System.Collections.Generic;
////using System.Diagnostics;
////using System.Linq;
////using System.Linq.Expressions;
////using System.Runtime.InteropServices;
////using System.ComponentModel;

////namespace CSharpScriptExecutor.Common
////{
////    public static class WinApi
////    {
////        #region Nested Types

////        #region Native Class

////        private static unsafe class Native
////        {
////            #region Nested Types

////            #region Delegates

////            [UnmanagedFunctionPointer(CallingConvention.Winapi)]
////            public delegate bool CheckTokenMembershipMethod(IntPtr tokenHandle, IntPtr sidToCheck, out bool isMember);

////            #endregion

////            #region Libraries Class

////            private static class Libraries
////            {
////                #region Constants

////                public const string Kernel32 = "kernel32.dll";
////                public const string Advapi32 = "advapi32.dll";

////                #endregion
////            }

////            #endregion

////            #endregion

////            #region Fields

////            public static readonly CheckTokenMembershipMethod CheckTokenMembership =
////                GetNativeMethod<CheckTokenMembershipMethod>(Libraries.Advapi32, "CheckTokenMembership");

////            #endregion

////            #region Private Methods

////            private static TMethod GetNativeMethod<TMethod>(string library, string methodName)
////            {
////                #region Argument Check

////                if (string.IsNullOrEmpty(library))
////                {
////                    throw new ArgumentException("The value can be neither empty string nor null.", "library");
////                }
////                if (string.IsNullOrEmpty(methodName))
////                {
////                    throw new ArgumentException("The value can be neither empty string nor null.", "methodName");
////                }
////                if (!typeof(Delegate).IsAssignableFrom(typeof(TMethod)))
////                {
////                    throw new ArgumentException("The parameter must be a delegate.", "TMethod");
////                }

////                #endregion

////                var libraryHandle = LoadLibrary(library);
////                if (libraryHandle == IntPtr.Zero)
////                {
////                    return default(TMethod);
////                }

////                var functionPointer = GetProcAddress(libraryHandle, methodName);
////                if (functionPointer == IntPtr.Zero)
////                {
////                    FreeLibrary(libraryHandle);
////                    return default(TMethod);
////                }

////                return (TMethod)(object)Marshal.GetDelegateForFunctionPointer(functionPointer, typeof(TMethod));
////            }

////            #endregion

////            #region Public Methods

////            [DllImport(Libraries.Kernel32, ExactSpelling = true)]
////            public static extern void SetLastError(uint dwErrCode);

////            [DllImport(Libraries.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
////            public static extern IntPtr LoadLibrary(string lpFileName);

////            [DllImport(Libraries.Kernel32, SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
////            public static extern bool FreeLibrary(IntPtr hModule);

////            [DllImport(Libraries.Kernel32, SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
////            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

////            #endregion
////        }

////        #endregion

////        #region ErrorCode Class

////        public static class ErrorCode
////        {
////            #region Constants

////            public const uint ERROR_SUCCESS = 0;
////            public const uint ERROR_INVALID_FUNCTION = 0x00000001;

////            #endregion
////        }

////        #endregion

////        #endregion

////        #region Public Methods

////        public static bool CheckTokenMembership(IntPtr tokenHandle, IntPtr sidToCheck)
////        {
////            bool result = false;

////            var errorCode = Native.CheckTokenMembership == null
////                ? ErrorCode.ERROR_INVALID_FUNCTION
////                : (Native.CheckTokenMembership(tokenHandle, sidToCheck, out result)
////                    ? ErrorCode.ERROR_SUCCESS
////                    : (uint)Marshal.GetLastWin32Error());

////            if (errorCode != ErrorCode.ERROR_SUCCESS)
////            {
////                throw new Win32Exception((int)errorCode);
////            }

////            return result;
////        }

////        #endregion
////    }
////}