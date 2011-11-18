using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpScriptExecutor.Common
{
    internal static class InternalHelper
    {
        #region Constants

        private const string c_quotedStringGroupName = "value";
        private const string c_quote = "\"";

        #endregion

        #region Fields

        private static readonly Regex s_quotedStringRegex = new Regex(
            string.Format(
                @"^ \s* {0} (?<{1}>[^{0}]*) {0} \s* $",
                Regex.Escape(c_quote),
                c_quotedStringGroupName),
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

        #endregion

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

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue = default(TValue))
        {
            #region Argument Check

            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            #endregion

            TValue result;
            if (!dictionary.TryGetValue(key, out result))
            {
                result = defaultValue;
            }
            return result;
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

        public static string TryExtractFromQuotes(this string value, out string errorMessage)
        {
            if (string.IsNullOrEmpty(value))
            {
                errorMessage = null;
                return value;
            }

            var result = value;

            var match = s_quotedStringRegex.Match(result);
            if (match != null && match.Success)
            {
                result = match.Groups[c_quotedStringGroupName].Value;
            }

            errorMessage = result.IndexOf(c_quote) < 0
                ? null
                : string.Format("Combination of quotes is invalid in the value:\n{0}", value);
            return result;
        }

        public static string ExtractFromQuotes(this string value)
        {
            string errorMessage;
            var result = TryExtractFromQuotes(value, out errorMessage);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentException(errorMessage, "value");
            }
            return result;
        }

        public static bool BytesEqual(this byte[] array, byte[] otherArray)
        {
            if (object.ReferenceEquals(array, otherArray))
            {
                return true;
            }
            if (object.ReferenceEquals(array, null) || object.ReferenceEquals(otherArray, null))
            {
                return false;
            }
            if (array.Length != otherArray.Length)
            {
                return false;
            }

            for (int index = 0; index < array.Length; index++)
            {
                if (array[index] != otherArray[index])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}