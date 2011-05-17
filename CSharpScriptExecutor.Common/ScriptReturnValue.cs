using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CSharpScriptExecutor.Common
{
    // TODO: Use TypeDescriptionProvider to show all properties and fields of the underlying value in a property grid

    [Serializable]
    [ImmutableObject(true)]
    public sealed class ScriptReturnValue
    {
        #region Constants

        private const BindingFlags c_memberBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        #endregion

        #region Fields

        private static readonly ScriptReturnValue s_null = new ScriptReturnValue(null);

        private readonly Dictionary<string, ScriptReturnValue> m_propertyValueMap =
            new Dictionary<string, ScriptReturnValue>();
        private readonly Dictionary<string, ScriptReturnValue> m_fieldValueMap =
            new Dictionary<string, ScriptReturnValue>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptReturnValue"/> class.
        /// </summary>
        private ScriptReturnValue(object value)
        {
            if (ReferenceEquals(value, null))
            {
                IsNull = true;
                IsSimpleType = true;
                AsString = "<null>";
                return;
            }

            var type = value.GetType();

            this.TypeFullName = type.FullName;
            this.TypeAssemblyName = type.Assembly.GetName();
            this.TypeQualifiedName = type.AssemblyQualifiedName;
            this.IsSimpleType = type.IsPrimitive || type.IsEnum || type == typeof(char) || type == typeof(string)
                || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(DateTimeOffset);

            Func<string> toStringMethod = value.ToString;
            try
            {
                this.AsString = toStringMethod();
            }
            catch (Exception ex)
            {
                this.AsString = string.Format(
                    "An exception {0} occurred on calling value's {1} method: {2}",
                    ex.GetType().FullName,
                    toStringMethod.Method.Name,
                    ex.Message);
            }

            if (!this.IsSimpleType)
            {
                // TODO: Special handling for IDictionary, IEnumerable etc.

                var propertyInfos = type
                    .GetProperties(c_memberBindingFlags)
                    .Where(item => item.CanRead && !item.GetIndexParameters().Any())
                    .ToList();
                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = propertyInfo.GetValue(value, null);
                    var wrappedPropertyValue = new ScriptReturnValue(propertyValue);
                    m_propertyValueMap.Add(propertyInfo.Name, wrappedPropertyValue);
                }

                var fieldInfos = type.GetFields(c_memberBindingFlags);
                foreach (var fieldInfo in fieldInfos)
                {
                    var fieldValue = fieldInfo.GetValue(value);
                    var wrappedFieldValue = new ScriptReturnValue(fieldValue);
                    m_propertyValueMap.Add(fieldInfo.Name, wrappedFieldValue);
                }
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

        public bool IsNull
        {
            get;
            private set;
        }

        public string TypeFullName
        {
            get;
            private set;
        }

        [Browsable(false)]
        public AssemblyName TypeAssemblyName
        {
            get;
            private set;
        }

        [Browsable(false)]
        public string TypeQualifiedName
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