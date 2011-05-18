using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

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

            internal unsafe ReferenceWrapper(object value)
            {
                if (value is Pointer)
                {
                    m_value = null;
                    m_address = new IntPtr(Pointer.Unbox(value));
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
                return this.Name;
            }

            #endregion
        }

        #endregion

        #region ValuePropertyAccessException Class

        [Serializable]
        public sealed class ValuePropertyAccessException : Exception
        {
            #region Constructors and Destructors

            public ValuePropertyAccessException(string message, Exception innerException)
                : base(message, innerException)
            {
                // Nothing to do
            }

            private ValuePropertyAccessException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                // Nothing to do
            }

            #endregion
        }

        #endregion

        #endregion

        #region Constants

        private const BindingFlags c_memberBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        #endregion

        #region Fields

        private static readonly ScriptReturnValue s_null = new ScriptReturnValue(null);
        [ThreadStatic]
        private static Dictionary<ReferenceWrapper, ScriptReturnValue> s_objectsBeingProcessed;

        private readonly List<MemberCollection> m_memberCollections = new List<MemberCollection>();
        [NonSerialized]
        private Type m_type;
        [NonSerialized]
        private object m_value;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptReturnValue"/> class.
        /// </summary>
        private unsafe ScriptReturnValue(object value)
        {
            if (ReferenceEquals(value, null))
            {
                IsNull = true;
                IsSimpleType = true;
                AsString = "<null>";
                return;
            }

            m_value = value;
            m_type = value.GetType();

            this.Type = new TypeWrapper(m_type);
            this.IsSimpleType = m_type.IsPrimitive
                || m_type.IsEnum
                || m_type.IsPointer
                || m_type == typeof(char)
                || m_type == typeof(string)
                || m_type == typeof(decimal)
                || m_type == typeof(Pointer);

            var toStringableValue = m_type == typeof(Pointer) ? new IntPtr(Pointer.Unbox(value)) : value;
            Func<string> toStringMethod = toStringableValue.ToString;
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
        }

        #endregion

        #region Private Methods

        [DebuggerStepThrough]
        private object GetPropertyValueInternal(PropertyInfo propertyInfo)
        {
            object result;
            try
            {
                result = propertyInfo.GetValue(m_value, null);
            }
            catch (Exception ex)
            {
                var baseException = ex.GetBaseException();

                result = new ValuePropertyAccessException(
                    string.Format(
                        "Cannot read the property '{0}' of the type '{1}': [{2}] {3}",
                        propertyInfo.Name,
                        m_type.FullName,
                        baseException.GetType().FullName,
                        baseException.Message),
                    ex);
            }
            return result;
        }

        private void Initialize()
        {
            if (m_type == null || m_value == null)
            {
                throw new InvalidOperationException("Initialization is called too late or twice.");
            }

            if (!this.IsSimpleType)
            {
                var fieldValueMap = new Dictionary<string, ScriptReturnValue>();
                m_memberCollections.Add(new MemberCollection(fieldValueMap, "Fields"));
                var fieldInfos = m_type.GetFields(c_memberBindingFlags);
                foreach (var fieldInfo in fieldInfos)
                {
                    var fieldValue = fieldInfo.GetValue(m_value);
                    var wrappedFieldValue = Create(fieldValue);
                    fieldValueMap.Add(fieldInfo.Name, wrappedFieldValue);
                }

                var propertyValueMap = new Dictionary<string, ScriptReturnValue>();
                var propertyInfos = m_type
                    .GetProperties(c_memberBindingFlags)
                    .Where(item => item.CanRead && !item.GetIndexParameters().Any())
                    .ToList();
                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = GetPropertyValueInternal(propertyInfo);
                    var wrappedPropertyValue = Create(propertyValue);
                    propertyValueMap.Add(propertyInfo.Name, wrappedPropertyValue);
                }
                m_memberCollections.Add(new MemberCollection(propertyValueMap, "Properties"));
            }

            var enumerable = m_value as IEnumerable;
            if (enumerable != null)
            {
                var elementMap = enumerable
                    .Cast<object>()
                    .Select(
                        (item, index) => new { Key = string.Format("[{0}]", index), Value = Create(item) })
                    .ToDictionary(item => item.Key, item => item.Value);
                m_memberCollections.Add(new MemberCollection(elementMap, "Elements"));
            }

            m_type = null;
            m_value = null;
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
            try
            {
                if (s_objectsBeingProcessed == null)
                {
                    objectsBeingProcessedCreated = true;
                    s_objectsBeingProcessed = new Dictionary<ReferenceWrapper, ScriptReturnValue>();
                }

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
                result.Initialize();

                return result;
            }
            finally
            {
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

        public bool IsSimpleType
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
                .Where(item => item.HasAnyValue)
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