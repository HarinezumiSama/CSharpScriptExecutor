﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    public sealed class ValueAccessException : Exception
    {
        private static readonly string FieldRecursionLimitExceededMessage =
            $"Skipped reading the field: recursion limit {ScriptReturnValue.MaxRecursionCount} is reached.";

        private static readonly string PropertyRecursionLimitExceededMessage =
            $"Skipped reading the property: recursion limit {ScriptReturnValue.MaxRecursionCount} is reached.";

        internal static readonly ValueAccessException FieldRecursionLimitExceeded =
            new ValueAccessException(FieldRecursionLimitExceededMessage, null);

        internal static readonly ValueAccessException PropertyRecursionLimitExceeded =
            new ValueAccessException(PropertyRecursionLimitExceededMessage, null);

        private readonly string _asString;

        internal ValueAccessException(string message, Exception innerException)
            : base(message)
        {
            var asStringBuilder = new StringBuilder(message);

            if (innerException != null)
            {
                InnerExceptionType = new TypeWrapper(innerException.GetType());
                InnerExceptionMessage = innerException.Message;
                if (!string.IsNullOrWhiteSpace(InnerExceptionMessage))
                {
                    asStringBuilder.AppendFormat(" [{0}] {1}", InnerExceptionType, InnerExceptionMessage);
                }
            }

            _asString = asStringBuilder.ToString();
        }

        private ValueAccessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Nothing to do
        }

        public TypeWrapper InnerExceptionType
        {
            get;
        }

        public string InnerExceptionMessage
        {
            get;
        }

        public override string ToString() => _asString;
    }
}