using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    [ImmutableObject(true)]
    public sealed class ScriptReturnValue : ICustomTypeDescriptor
    {
        #region Nested Types

        #region ReferenceWrapper Structure

        [DebuggerDisplay("{GetType().Name,nq}. Value = {Value}")]
        private struct ReferenceWrapper : IEquatable<ReferenceWrapper>
        {
            #region Fields

            private static readonly ReferenceWrapper s_null = new ReferenceWrapper();

            private readonly object m_value;

            #endregion

            #region Constructors

            internal ReferenceWrapper(object value)
            {
                m_value = value;
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

            #endregion

            #region Public Methods

            public override bool Equals(object obj)
            {
                return (obj is ReferenceWrapper) && this.Equals((ReferenceWrapper)obj);
            }

            public override int GetHashCode()
            {
                return RuntimeHelpers.GetHashCode(m_value);
            }

            public override string ToString()
            {
                return object.ReferenceEquals(m_value, null) ? string.Empty : m_value.ToString();
            }

            #endregion

            #region IEquatable<ReferenceWrapper> Members

            public bool Equals(ReferenceWrapper other)
            {
                return object.ReferenceEquals(other.m_value, m_value);
            }

            #endregion
        }

        #endregion

        #region MemberCollection Class

        [Serializable]
        [ImmutableObject(true)]
        private sealed class MemberCollection : ICustomTypeDescriptor
        {
            #region Nested Types

            #region SubvaluePropertyDescriptor Class

            private sealed class SubvaluePropertyDescriptor : PropertyDescriptor
            {
                #region Fields

                private static readonly Attribute[] s_attributes =
                    new Attribute[]
                    {
                        new TypeConverterAttribute(typeof(ExpandableObjectConverter))
                    };

                private readonly MemberCollection m_owner;
                private readonly ScriptReturnValue m_propertyValue;

                #endregion

                #region Constructors

                internal SubvaluePropertyDescriptor(
                    MemberCollection owner,
                    string name,
                    ScriptReturnValue propertyValue)
                    : base(name, s_attributes)
                {
                    #region Argument Check

                    if (owner == null)
                    {
                        throw new ArgumentNullException("owner");
                    }
                    if (propertyValue == null)
                    {
                        throw new ArgumentNullException("propertyValue");
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
                    get { return m_propertyValue.GetType(); }
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

            #endregion

            #region Fields

            private readonly Dictionary<string, ScriptReturnValue> m_subvalueMap;
            private readonly string m_displayName;

            #endregion

            #region Constructors

            internal MemberCollection(Dictionary<string, ScriptReturnValue> subvalueMap, string displayName)
            {
                #region Argument Check

                if (subvalueMap == null)
                {
                    throw new ArgumentNullException("subvalueMap");
                }
                if (string.IsNullOrEmpty(displayName))
                {
                    throw new ArgumentException("The value can be neither empty string nor null.", "displayName");
                }

                #endregion

                m_subvalueMap = subvalueMap;
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
                get { return m_subvalueMap.Count != 0; }
            }

            #endregion

            #region Public Methods

            public override string ToString()
            {
                return string.Format("Count = {0}", m_subvalueMap.Count);
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
                var proxyResult = new List<PropertyDescriptor>(m_subvalueMap.Count);
                foreach (var pair in m_subvalueMap)
                {
                    var propertyDescriptor = new SubvaluePropertyDescriptor(this, pair.Key, pair.Value);
                    proxyResult.Add(propertyDescriptor);
                }

                return new PropertyDescriptorCollection(proxyResult.ToArray(), true);
            }

            object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
            {
                return this;
            }

            #endregion
        }

        #endregion

        #region ReturnValuePropertyDescriptor Class

        private sealed class ReturnValuePropertyDescriptor : PropertyDescriptor
        {
            #region Fields

            private static readonly Attribute[] s_attributes =
                new Attribute[]
                {
                    new TypeConverterAttribute(typeof(ExpandableObjectConverter))
                };

            private readonly ScriptReturnValue m_owner;
            private readonly MemberCollection m_propertyValue;

            #endregion

            #region Constructors

            internal ReturnValuePropertyDescriptor(
                ScriptReturnValue owner,
                string name,
                MemberCollection propertyValue)
                : base(name, s_attributes)
            {
                #region Argument Check

                if (owner == null)
                {
                    throw new ArgumentNullException("owner");
                }
                if (propertyValue == null)
                {
                    throw new ArgumentNullException("propertyValue");
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
                get { return m_propertyValue.GetType(); }
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

        #endregion

        #region Constants

        private const BindingFlags c_memberBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        #endregion

        #region Fields

        private static readonly ScriptReturnValue s_null = new ScriptReturnValue(null);

        [ThreadStatic]
        private static Dictionary<ReferenceWrapper, ScriptReturnValue> s_objectsBeingProcessed;

        private readonly Dictionary<string, ScriptReturnValue> m_fieldValueMap =
            new Dictionary<string, ScriptReturnValue>();
        private readonly Dictionary<string, ScriptReturnValue> m_propertyValueMap =
            new Dictionary<string, ScriptReturnValue>();
        private readonly IList<MemberCollection> m_memberCollections;

        [NonSerialized]
        private Type m_type;
        [NonSerialized]
        private object m_value;

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
                m_memberCollections = new List<MemberCollection>().AsReadOnly();
                return;
            }

            m_value = value;
            m_type = value.GetType();

            this.TypeFullName = m_type.FullName;
            this.TypeAssemblyName = m_type.Assembly.GetName();
            this.TypeQualifiedName = m_type.AssemblyQualifiedName;
            this.IsSimpleType = m_type.IsPrimitive || m_type.IsEnum || m_type == typeof(char)
                || m_type == typeof(string) || m_type == typeof(decimal);

            m_memberCollections =
                new List<MemberCollection>
                {
                    new MemberCollection(m_fieldValueMap, "[Fields]"),
                    new MemberCollection(m_propertyValueMap, "[Properties]")
                }
                .AsReadOnly();

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
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            if (m_type == null || m_value == null)
            {
                throw new InvalidOperationException("Initialization is called too late or twice.");
            }

            if (!this.IsSimpleType || !m_type.IsSerializable)
            {
                var fieldInfos = m_type.GetFields(c_memberBindingFlags);
                foreach (var fieldInfo in fieldInfos)
                {
                    var fieldValue = fieldInfo.GetValue(m_value);
                    var wrappedFieldValue = ScriptReturnValue.Create(fieldValue);
                    m_fieldValueMap.Add(fieldInfo.Name, wrappedFieldValue);
                }

                var propertyInfos = m_type
                    .GetProperties(c_memberBindingFlags)
                    .Where(item => item.CanRead && !item.GetIndexParameters().Any())
                    .ToList();
                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = propertyInfo.GetValue(m_value, null);
                    var wrappedPropertyValue = ScriptReturnValue.Create(propertyValue);
                    m_propertyValueMap.Add(propertyInfo.Name, wrappedPropertyValue);
                }
            }

            m_type = null;
            m_value = null;
        }

        #endregion

        #region Internal Methods

        internal static ScriptReturnValue Create(object returnValue)
        {
            if (ReferenceEquals(returnValue, null))
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

                var isReferenceType = !returnValue.GetType().IsValueType;
                if (isReferenceType)
                {
                    var reference = new ReferenceWrapper(returnValue);
                    ScriptReturnValue preResult;
                    if (s_objectsBeingProcessed.TryGetValue(reference, out preResult))
                    {
                        return preResult;
                    }
                }

                var result = new ScriptReturnValue(returnValue);
                if (isReferenceType)
                {
                    s_objectsBeingProcessed.Add(new ReferenceWrapper(returnValue), result);
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

        public List<string> GetPropertyNames()
        {
            return m_propertyValueMap.Keys.ToList();
        }

        public ScriptReturnValue GetPropertyValue(string name)
        {
            #region Argument Check

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The value can be neither empty string nor null.", "name");
            }

            #endregion

            return m_propertyValueMap[name];
        }

        public List<string> GetFieldNames()
        {
            return m_fieldValueMap.Keys.ToList();
        }

        public ScriptReturnValue GetFieldValue(string name)
        {
            #region Argument Check

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The value can be neither empty string nor null.", "name");
            }

            #endregion

            return m_fieldValueMap[name];
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
            var proxyResult = new List<PropertyDescriptor>(
                TypeDescriptor.GetProperties(this, true).Cast<PropertyDescriptor>());
            foreach (var memberCollection in m_memberCollections)
            {
                if (memberCollection.HasAnyValue)
                {
                    var propertyDescriptor = new ReturnValuePropertyDescriptor(
                        this,
                        memberCollection.DisplayName,
                        memberCollection);
                    proxyResult.Add(propertyDescriptor);
                }
            }

            return new PropertyDescriptorCollection(proxyResult.ToArray(), true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }
}