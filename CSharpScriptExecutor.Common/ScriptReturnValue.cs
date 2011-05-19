using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    [ImmutableObject(true)]
    public sealed class ScriptReturnValue : ICustomTypeDescriptor
    {
        #region Nested Types

        #region ReferenceWrapper Structure

        private struct ReferenceWrapper : IEquatable<ReferenceWrapper>
        {
            #region Fields

            private static readonly ReferenceWrapper s_null = new ReferenceWrapper();

            private readonly object m_value;
            private readonly IntPtr m_address;

            #endregion

            #region Constructors

            internal ReferenceWrapper(object value)
            {
                if (value is Pointer)
                {
                    m_value = null;
                    unsafe
                    {
                        m_address = new IntPtr(Pointer.Unbox(value));
                    }
                }
                else
                {
                    m_value = value;
                    m_address = IntPtr.Zero;
                }
            }

            #endregion

            #region Public Properties

            public static ReferenceWrapper Null
            {
                [DebuggerStepThrough]
                get { return s_null; }
            }

            public object Value
            {
                [DebuggerStepThrough]
                get { return m_value; }
            }

            public IntPtr Address
            {
                [DebuggerStepThrough]
                get { return m_address; }
            }

            #endregion

            #region Public Methods

            public override bool Equals(object obj)
            {
                return (obj is ReferenceWrapper) && this.Equals((ReferenceWrapper)obj);
            }

            public override int GetHashCode()
            {
                return RuntimeHelpers.GetHashCode(m_value) ^ m_address.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("{{{0}. Value = {{{1}}}, Address = {2}}}", GetType().Name, m_value, m_address);
            }

            #endregion

            #region IEquatable<ReferenceWrapper> Members

            public bool Equals(ReferenceWrapper other)
            {
                return ReferenceEquals(other.m_value, m_value) && other.m_address == m_address;
            }

            #endregion
        }

        #endregion

        #region ValuePropertyDescriptor Class

        private sealed class ValuePropertyDescriptor : PropertyDescriptor
        {
            #region Fields

            private static readonly Attribute[] s_emptyAttributes = new Attribute[0];

            private static readonly Attribute[] s_expandableAttributes =
                new Attribute[] { new TypeConverterAttribute(typeof(ExpandableObjectConverter)) };

            private readonly object m_owner;
            private readonly object m_propertyValue;

            #endregion

            #region Constructors

            internal ValuePropertyDescriptor(
                object owner,
                string name,
                object propertyValue,
                bool expandable)
                : base(name, expandable ? s_expandableAttributes : s_emptyAttributes)
            {
                #region Argument Check

                if (owner == null)
                {
                    throw new ArgumentNullException("owner");
                }

                #endregion

                m_owner = owner;
                m_propertyValue = propertyValue;
            }

            #endregion

            #region Public Properties

            public override Type ComponentType
            {
                [DebuggerNonUserCode]
                get { return m_owner.GetType(); }
            }

            public override bool IsReadOnly
            {
                [DebuggerStepThrough]
                get { return true; }
            }

            public override Type PropertyType
            {
                [DebuggerNonUserCode]
                get { return m_propertyValue == null ? null : m_propertyValue.GetType(); }
            }

            #endregion

            #region Public Methods

            [DebuggerNonUserCode]
            public override bool CanResetValue(object component)
            {
                return false;
            }

            [DebuggerNonUserCode]
            public override object GetValue(object component)
            {
                if (component == m_owner)
                {
                    return m_propertyValue;
                }
                return null;
            }

            [DebuggerNonUserCode]
            public override void ResetValue(object component)
            {
                throw new InvalidOperationException("The property is non-resettable.");
            }

            [DebuggerNonUserCode]
            public override void SetValue(object component, object value)
            {
                throw new InvalidOperationException("The property is read-only.");
            }

            [DebuggerNonUserCode]
            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            #endregion
        }

        #endregion

        #region MemberCollection Class

        [Serializable]
        [ImmutableObject(true)]
        private sealed class MemberCollection : ICustomTypeDescriptor
        {
            #region Fields

            private readonly Dictionary<string, ScriptReturnValue> m_valueMap;
            private readonly string m_displayName;

            #endregion

            #region Constructors

            internal MemberCollection(Dictionary<string, ScriptReturnValue> valueMap, string displayName)
            {
                #region Argument Check

                if (valueMap == null)
                {
                    throw new ArgumentNullException("valueMap");
                }
                if (string.IsNullOrEmpty(displayName))
                {
                    throw new ArgumentException("The value can be neither empty string nor null.", "displayName");
                }

                #endregion

                m_valueMap = valueMap;
                m_displayName = displayName;
            }

            #endregion

            #region Public Properties

            public string DisplayName
            {
                [DebuggerStepThrough]
                get { return m_displayName; }
            }

            public bool HasAnyValue
            {
                [DebuggerNonUserCode]
                get { return m_valueMap.Count != 0; }
            }

            #endregion

            #region Public Methods

            public override string ToString()
            {
                return string.Format("Count = {0}", m_valueMap.Count);
            }

            #endregion

            #region ICustomTypeDescriptor Members

            AttributeCollection ICustomTypeDescriptor.GetAttributes()
            {
                return TypeDescriptor.GetAttributes(this, true);
            }

            string ICustomTypeDescriptor.GetClassName()
            {
                return TypeDescriptor.GetClassName(this, true);
            }

            string ICustomTypeDescriptor.GetComponentName()
            {
                return TypeDescriptor.GetComponentName(this, true);
            }

            TypeConverter ICustomTypeDescriptor.GetConverter()
            {
                return TypeDescriptor.GetConverter(this, true);
            }

            EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
            {
                return TypeDescriptor.GetDefaultEvent(this, true);
            }

            PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
            {
                return TypeDescriptor.GetDefaultProperty(this, true);
            }

            object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
            {
                return TypeDescriptor.GetEditor(this, editorBaseType, true);
            }

            EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
            {
                return TypeDescriptor.GetEvents(this, attributes, true);
            }

            EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
            {
                return TypeDescriptor.GetEvents(this, true);
            }

            PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
            {
                return ((ICustomTypeDescriptor)this).GetProperties();
            }

            PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
            {
                return new PropertyDescriptorCollection(
                    m_valueMap
                        .Select(
                            pair => (PropertyDescriptor)new ValuePropertyDescriptor(
                                this,
                                pair.Key,
                                pair.Value,
                                true))
                        .OrderBy(item => item.DisplayName)
                        .ThenBy(item => item.Name)
                        .ToArray(),
                    true);
            }

            object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
            {
                return this;
            }

            #endregion
        }

        #endregion

        #region TypeWrapper Class

        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [ImmutableObject(true)]
        public sealed class TypeWrapper
        {
            #region Fields

            private readonly string m_asString;

            #endregion

            #region Constructors

            public TypeWrapper(Type type)
            {
                #region Argument Check

                if (type == null)
                {
                    throw new ArgumentNullException("type");
                }

                #endregion

                this.Name = type.Name;
                this.FullName = type.FullName;
                this.AssemblyName = type.Assembly.GetName().Name;
                this.AssemblyFullName = type.Assembly.GetName().FullName;
                this.AssemblyQualifiedName = type.AssemblyQualifiedName;
                m_asString = type.ToString();
            }

            #endregion

            #region Public Properties

            public string Name
            {
                get;
                private set;
            }

            public string FullName
            {
                get;
                private set;
            }

            public string AssemblyName
            {
                get;
                private set;
            }

            public string AssemblyFullName
            {
                get;
                private set;
            }

            public string AssemblyQualifiedName
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

        #endregion

        #region ValueAccessException Class

        [Serializable]
        internal sealed class ValueAccessException : Exception
        {
            #region Fields

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

        #endregion

        #endregion

        #region Constants

        private const BindingFlags c_memberBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const int c_maxRecursionCount = 128;

        #endregion

        #region Fields

        private static readonly ScriptReturnValue s_null = new ScriptReturnValue(null);
        private static readonly string s_toStringMethodName = new Func<string>(new object().ToString).Method.Name;
        private static readonly string s_pointerStringFormat = GetPointerStringFormat();
        private static readonly HashSet<Assembly> s_safeAssemblies = GetSafeAssemblies();

        private static readonly string s_fieldRecursionLimitExceededMessage = string.Format(
            "Skipped reading the field: recursion limit {0} is reached.",
            c_maxRecursionCount);
        private static readonly string s_propertyRecursionLimitExceededMessage = string.Format(
            "Skipped reading the property: recursion limit {0} is reached.",
            c_maxRecursionCount);

        [ThreadStatic]
        private static Dictionary<ReferenceWrapper, ScriptReturnValue> s_objectsBeingProcessed;
        [ThreadStatic]
        private static int? s_recursionCount;

        [ThreadStatic]
        private static ulong s_instanceCount;

        private readonly List<MemberCollection> m_memberCollections = new List<MemberCollection>();
        private readonly bool m_isSimpleType;
        private bool m_isInitialized;
        private Type m_systemType;
        private object m_value;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptReturnValue"/> class.
        /// </summary>
        private ScriptReturnValue(object value)
        {
            checked
            {
                s_instanceCount++;
            }
            if (s_instanceCount % 10 == 0)
            {
                Debug.WriteLine("{0} instances: {1}+", GetType().Name, s_instanceCount);
            }

            if (ReferenceEquals(value, null))
            {
                IsNull = true;
                m_isSimpleType = true;
                AsString = "<null>";
                m_isInitialized = true;
                return;
            }

            m_value = value;
            m_systemType = value.GetType();

            this.Type = new TypeWrapper(m_systemType);
            this.m_isSimpleType = m_systemType.IsPrimitive
                || m_systemType.IsEnum
                || m_systemType.IsPointer
                || m_systemType == typeof(char)
                || m_systemType == typeof(string)
                || m_systemType == typeof(decimal)
                || m_systemType == typeof(Pointer)
                || typeof(Delegate).IsAssignableFrom(m_systemType);

            try
            {
                if (m_systemType == typeof(Pointer))
                {
                    unsafe
                    {
                        this.AsString = string.Format(s_pointerStringFormat, (long)Pointer.Unbox(value));
                    }
                }
                else
                {
                    this.AsString = value.ToString();
                }
            }
            catch (Exception ex)
            {
                this.AsString = string.Format(
                    "An exception {0} occurred on calling value's {1} method: {2}",
                    ex.GetType().FullName,
                    s_toStringMethodName,
                    ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private static unsafe string GetPointerStringFormat()
        {
            return string.Format("0x{{0:X{0}}}", sizeof(IntPtr) * 2);
        }

        private static HashSet<Assembly> GetSafeAssemblies()
        {
            return new HashSet<Assembly>
            {
                // mscorlib
                typeof(void).Assembly,

                // System
                typeof(object).Assembly,

                // System.Core
                typeof(global::System.Linq.Enumerable).Assembly,

                // System.Data
                typeof(global::System.Data.DataTable).Assembly,

                // This assembly
                typeof(global::CSharpScriptExecutor.Common.ScriptReturnValue).Assembly
            };
        }

        [DebuggerStepThrough]
        private static object AutoWrapIfSpecialType(object value)
        {
            var type = value as Type;
            if (type != null)
            {
                return new TypeWrapper(type);
            }

            return value;
        }

        [DebuggerStepThrough]
        private object GetFieldValueInternal(FieldInfo fieldInfo)
        {
            object result;

            if (s_recursionCount.GetValueOrDefault() > c_maxRecursionCount)
            {
                result = ValueAccessException.FieldRecursionLimitExceeded;
                return result;
            }

            result = fieldInfo.GetValue(m_value);
            result = AutoWrapIfSpecialType(result);
            return result;
        }

        [DebuggerStepThrough]
        private object GetPropertyValueInternal(PropertyInfo propertyInfo)
        {
            object result;

            if (s_recursionCount.GetValueOrDefault() > c_maxRecursionCount)
            {
                result = ValueAccessException.PropertyRecursionLimitExceeded;
                return result;
            }

            try
            {
                result = propertyInfo.GetValue(m_value, null);
            }
            catch (Exception ex)
            {
                var baseException = ex.GetBaseException();

                result = new ValueAccessException("Cannot read the property.", ex);
            }

            result = AutoWrapIfSpecialType(result);
            return result;
        }

        private static bool IsLazy(Type type)
        {
            return type != null && type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Lazy<>));
        }

        private bool CanInitializationBeSkipped(bool allowLazy)
        {
            return allowLazy
                && !IsLazy(m_systemType)
                && m_systemType.IsSerializable
                && s_safeAssemblies.Contains(m_systemType.Assembly);
        }

        private void Initialize(bool allowLazy)
        {
            if (m_isInitialized)
            {
                return;
            }

            if (m_systemType == null || m_value == null)
            {
                throw new InvalidOperationException("Initialization is called too late.");
            }

            if (!this.m_isSimpleType)
            {
                if (CanInitializationBeSkipped(allowLazy))
                {
                    return;
                }

                var fieldValueMap = new Dictionary<string, ScriptReturnValue>();
                var fieldInfos = m_systemType.GetFields(c_memberBindingFlags);
                foreach (var fieldInfo in fieldInfos)
                {
                    var fieldValue = GetFieldValueInternal(fieldInfo);
                    var wrappedFieldValue = Create(fieldValue);
                    fieldValueMap.Add(fieldInfo.Name, wrappedFieldValue);
                }
                m_memberCollections.Add(new MemberCollection(fieldValueMap, "Fields"));

                var propertyValueMap = new Dictionary<string, ScriptReturnValue>();
                var propertyInfos = m_systemType
                    .GetProperties(c_memberBindingFlags)
                    .Where(item => item.CanRead && !item.GetIndexParameters().Any())
                    .ToArray();
                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = GetPropertyValueInternal(propertyInfo);
                    var wrappedPropertyValue = Create(propertyValue);
                    propertyValueMap.Add(propertyInfo.Name, wrappedPropertyValue);
                }
                m_memberCollections.Add(new MemberCollection(propertyValueMap, "Properties"));
            }

            if (!(m_value is string))
            {
                var enumerable = m_value as IEnumerable;
                if (enumerable != null)
                {
                    if (CanInitializationBeSkipped(allowLazy))
                    {
                        return;
                    }

                    var elementMap = enumerable
                        .Cast<object>()
                        .Select(
                            (item, index) =>
                                new { Key = string.Format("[{0}]", index), Value = Create(item) })
                        .ToDictionary(item => item.Key, item => item.Value);
                    m_memberCollections.Add(new MemberCollection(elementMap, "Elements"));
                }
            }

            m_systemType = null;
            m_value = null;
            m_isInitialized = true;
        }

        #endregion

        #region Internal Methods

        internal static ScriptReturnValue Create(object value)
        {
            if (ReferenceEquals(value, null))
            {
                return s_null;
            }

            var objectsBeingProcessedCreated = false;
            var recursionCountInitialized = false;
            try
            {
                if (s_objectsBeingProcessed == null)
                {
                    objectsBeingProcessedCreated = true;
                    s_objectsBeingProcessed = new Dictionary<ReferenceWrapper, ScriptReturnValue>();
                }
                if (!s_recursionCount.HasValue)
                {
                    recursionCountInitialized = true;
                    s_recursionCount = 0;
                }

                // TODO: Look into why SyncRoot property of int[] instance (array) causes eternal recursion

                var isReferenceType = !value.GetType().IsValueType;
                if (isReferenceType)
                {
                    var reference = new ReferenceWrapper(value);
                    ScriptReturnValue preResult;
                    if (s_objectsBeingProcessed.TryGetValue(reference, out preResult))
                    {
                        return preResult;
                    }
                }

                var result = new ScriptReturnValue(value);
                if (isReferenceType)
                {
                    s_objectsBeingProcessed.Add(new ReferenceWrapper(value), result);
                }

                // Initialization must be performed only after reference is added to processed objects map
                s_recursionCount = s_recursionCount.Value + 1;
                try
                {
                    result.Initialize(true);
                }
                finally
                {
                    s_recursionCount = s_recursionCount.Value - 1;
                }

                return result;
            }
            finally
            {
                if (recursionCountInitialized)
                {
                    s_recursionCount = null;
                }
                if (objectsBeingProcessedCreated)
                {
                    s_objectsBeingProcessed = null;
                }
            }
        }

        #endregion

        #region Public Properties

        public bool IsNull
        {
            get;
            private set;
        }

        public TypeWrapper Type
        {
            get;
            private set;
        }

        public string AsString
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return this.AsString;
        }

        #endregion

        #region ICustomTypeDescriptor Members

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return ((ICustomTypeDescriptor)this).GetProperties();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            this.Initialize(false);
            if (!m_isInitialized)
            {
                throw new InvalidOperationException(
                    string.Format("{0}: initialization has failed.", GetType().FullName));
            }

            var predefinedProperties = new List<PropertyDescriptor>();
            if (this.IsNull)
            {
                predefinedProperties.Add(new ValuePropertyDescriptor(this, "(IsNull)", this.IsNull, false));
            }
            else
            {
                predefinedProperties.Add(new ValuePropertyDescriptor(this, "(Type)", this.Type, false));
                predefinedProperties.Add(new ValuePropertyDescriptor(this, "(AsString)", this.AsString, false));
            }

            var runTimeProperties = m_memberCollections
                .Select(item => new ValuePropertyDescriptor(this, item.DisplayName, item, true));

            return new PropertyDescriptorCollection(predefinedProperties.Concat(runTimeProperties).ToArray(), true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }
}