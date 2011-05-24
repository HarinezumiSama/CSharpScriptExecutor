using System;
using System.Collections;
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
        #region Fields

        private static readonly string s_fieldRecursionLimitExceededMessage = string.Format(
            "Skipped reading the field: recursion limit {0} is reached.",
            ScriptReturnValue.MaxRecursionCount);
        private static readonly string s_propertyRecursionLimitExceededMessage = string.Format(
            "Skipped reading the property: recursion limit {0} is reached.",
            ScriptReturnValue.MaxRecursionCount);

        internal static readonly ValueAccessException FieldRecursionLimitExceeded =
            new ValueAccessException(s_fieldRecursionLimitExceededMessage, null);
        internal static readonly ValueAccessException PropertyRecursionLimitExceeded =
            new ValueAccessException(s_propertyRecursionLimitExceededMessage, null);

        private readonly string m_asString;

        #endregion

        #region Constructors and Destructors

        internal ValueAccessException(string message, Exception innerException)
            : base(message)
        {
            StringBuilder asStringBuilder = new StringBuilder(message);

            if (innerException != null)
            {
                InnerExceptionType = new TypeWrapper(innerException.GetType());
                InnerExceptionMessage = innerException.Message;
                if (!string.IsNullOrWhiteSpace(InnerExceptionMessage))
                {
                    asStringBuilder.AppendFormat(" [{0}] {1}", InnerExceptionType, InnerExceptionMessage);
                }
            }

            m_asString = asStringBuilder.ToString();
        }

        private ValueAccessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Nothing to do
        }

        #endregion

        #region Public Properties

        public TypeWrapper InnerExceptionType
        {
            get;
            private set;
        }

        public string InnerExceptionMessage
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return m_asString;
        }

        #endregion
    }
}