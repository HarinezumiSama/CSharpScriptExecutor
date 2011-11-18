using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    internal static class InternalHelper
    {
        #region Public Methods

        public static T EnsureNotNull<T>(this T value)
        {
            #region Argument Check

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            #endregion

            return value;
        }

        public static TResult UseDisposable<TDisposable, TResult>(
            this TDisposable disposable,
            Func<TDisposable, TResult> execute)
            where TDisposable : IDisposable
        {
            #region Argument Check

            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            #endregion

            using (disposable)
            {
                return execute(disposable);
            }
        }

        public static void UseDisposable<TDisposable>(this TDisposable disposable, Action<TDisposable> execute)
            where TDisposable : IDisposable
        {
            #region Argument Check

            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            #endregion

            using (disposable)
            {
                execute(disposable);
            }
        }

        #endregion
    }
}