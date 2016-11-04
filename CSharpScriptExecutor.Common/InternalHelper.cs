using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CSharpScriptExecutor.Common
{
    internal static class InternalHelper
    {
        #region Constants and Fields

        private const RegexOptions DefaultRegexOptions = RegexOptions.Compiled
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.ExplicitCapture;

        private const string QuotedStringGroupName = "value";
        private const string Quote = "\"";

        private const string ParameterGroupName = "p";

        private static readonly Regex QuotedStringRegex = new Regex(
            string.Format(
                @"^ \s* {0} (?<{1}>[^{0}]*) {0} \s* $",
                Regex.Escape(Quote),
                QuotedStringGroupName),
            DefaultRegexOptions | RegexOptions.Singleline);

        private static readonly Regex CommandLineRegex = new Regex(
            string.Format(
                @"\s* (((?<{0}>[^ ""]+)? (\"" (?<{0}>[^""]*) (\"" | $))*)+) \s*",
                ParameterGroupName),
            DefaultRegexOptions | RegexOptions.Singleline);

        private static readonly string[] NoCommandLineParameters = new string[0];

        #endregion

        #region Public Methods

        public static T EnsureNotNull<T>(this T value)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value;
        }

        public static T EnsureNotNull<T>(this T? value)
            where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value.Value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue = default(TValue))
        {
            #region Argument Check

            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (ReferenceEquals(key, null))
            {
                throw new ArgumentNullException(nameof(key));
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

            if (ReferenceEquals(disposable, null))
            {
                throw new ArgumentNullException(nameof(disposable));
            }

            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
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

            if (ReferenceEquals(disposable, null))
            {
                throw new ArgumentNullException(nameof(disposable));
            }

            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
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

            var match = QuotedStringRegex.Match(result);
            if (match.Success)
            {
                result = match.Groups[QuotedStringGroupName].Value;
            }

            errorMessage = result.IndexOf(Quote, StringComparison.Ordinal) < 0
                ? null
                : $"Combination of quotes is invalid in the value:\n{value}";

            return result;
        }

        public static string ExtractFromQuotes(this string value)
        {
            string errorMessage;
            var result = TryExtractFromQuotes(value, out errorMessage);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentException(errorMessage, nameof(value));
            }

            return result;
        }

        public static bool BytesEqual(this byte[] array, byte[] otherArray)
        {
            if (ReferenceEquals(array, otherArray))
            {
                return true;
            }

            if (ReferenceEquals(array, null) || ReferenceEquals(otherArray, null))
            {
                return false;
            }

            if (array.Length != otherArray.Length)
            {
                return false;
            }

            for (var index = 0; index < array.Length; index++)
            {
                if (array[index] != otherArray[index])
                {
                    return false;
                }
            }

            return true;
        }

        public static TAttribute GetSoleAttribute<TAttribute>(
            this ICustomAttributeProvider customAttributeProvider)
            where TAttribute : Attribute
        {
            #region Argument Check

            if (customAttributeProvider == null)
            {
                throw new ArgumentNullException(nameof(customAttributeProvider));
            }

            #endregion

            var attributes = (TAttribute[])customAttributeProvider.GetCustomAttributes(
                typeof(TAttribute),
                true);

            if ((attributes == null) || (attributes.Length != 1))
            {
                throw new InvalidProgramException(
                    $@"Invalid definition of the attribute '{typeof(TAttribute).FullName}'.");
            }

            return attributes[0];
        }

        public static string[] ParseCommandLineParameters(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine))
            {
                return NoCommandLineParameters;
            }

            var matches = CommandLineRegex.Matches(commandLine);

            var result = matches
                .Cast<Match>()
                .Where(match => match.Success)
                .Select(match => match.Groups[ParameterGroupName])
                .Where(group => group != null && group.Success)
                .Select(
                    group => string.Join(
                        string.Empty,
                        group
                            .Captures
                            .Cast<Capture>()
                            .OrderBy(capture => capture.Index)
                            .Select(capture => capture.Value)))
                .ToArray();
            return result;
        }

        #endregion
    }
}