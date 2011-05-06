using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    public sealed class ScriptReturnValue
    {
        #region Constants

        private const BindingFlags c_memberBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        #endregion

        #region Fields

        private static readonly ScriptReturnValue s_null = new ScriptReturnValue(null);

        private readonly Dictionary<PropertyInfo, ScriptReturnValue> m_propertyValueMap;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptReturnValue"/> class.
        /// </summary>
        private ScriptReturnValue(object returnValue)
        {
            m_propertyValueMap = new Dictionary<PropertyInfo, ScriptReturnValue>();

            if (ReferenceEquals(returnValue, null))
            {
                //Type = typeof(void);
                IsSimpleType = true;
                AsString = "<null>";
                return;
            }

            Type = returnValue.GetType();
            IsSimpleType = Type.IsPrimitive || Type == typeof(string) || Type == typeof(decimal)
                || Type == typeof(DateTime);

            Func<string> toStringMethod = returnValue.ToString;
            try
            {
                AsString = toStringMethod();
            }
            catch (Exception ex)
            {
                AsString = string.Format(
                    "An exception {0} occurred on calling value's {1} method: {2}",
                    ex.GetType().FullName,
                    toStringMethod.Method.Name,
                    ex.Message);
            }

            if (!IsSimpleType)
            {
                //TODO: Populate property map
            }
        }

        #endregion

        #region Private Methods

        //

        #endregion

        #region Internal Methods

        internal static ScriptReturnValue Create(object returnValue)
        {
            return ReferenceEquals(returnValue, null) ? s_null : new ScriptReturnValue(returnValue);
        }

        #endregion

        #region Public Properties

        public Type Type
        {
            get;
            private set;
        }

        public string AsString
        {
            get;
            private set;
        }

        public bool IsSimpleType
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        //

        #endregion
    }
}