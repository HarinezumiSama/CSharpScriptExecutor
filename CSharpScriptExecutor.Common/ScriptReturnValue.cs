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
    [ImmutableObject(true)]
    [Serializable]
    public sealed class ScriptReturnValue : MarshalByRefObject, IScriptReturnValue
    {
        #region Nested Types

        #region MemberCollection Class

        [Serializable]
        [ImmutableObject(true)]
        private sealed class MemberCollection : ICustomTypeDescriptor
        {
            #region Fields

            private readonly Dictionary<MemberKey, IScriptReturnValue> m_valueMap;
            private readonly string m_displayName;

            #endregion

            #region Constructors

            internal MemberCollection(Dictionary<MemberKey, IScriptReturnValue> valueMap, string displayName)
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
                            pair => new ValuePropertyDescriptor(
                                this,
                                pair.Key,
                                pair.Value,
                                true))
                        .OrderBy(item => item.OrderIndex)
                        .ThenBy(item => item.DisplayName)
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

        #endregion

        #region Constants

        private const BindingFlags c_memberBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const int c_maxRecursionCount = 128;

        #endregion

        #region Fields

        #region Static Fields

        private static readonly ScriptReturnValueProxy s_null =
            new ScriptReturnValueProxy(new ScriptReturnValue(null));
        private static readonly string s_toStringMethodName = new Func<string>(new object().ToString).Method.Name;
        private static readonly string s_pointerStringFormat = GetPointerStringFormat();

        private static readonly MemberKey s_isNullMemberKey = new MemberKey("(IsNull)");
        private static readonly MemberKey s_typeMemberKey = new MemberKey("(Type)");
        private static readonly MemberKey s_asStringMemberKey = new MemberKey("(AsString)");

        [ThreadStatic]
        private static Dictionary<ReferenceWrapper, ScriptReturnValueProxy> s_objectsBeingProcessed;
        [ThreadStatic]
        private static int? s_recursionCount;

        [ThreadStatic]
        private static ulong s_instanceCount;

        #endregion

        #region Instance Fields

        private readonly List<MemberCollection> m_memberCollections = new List<MemberCollection>();
        private readonly bool m_isSimpleType;
        private bool m_isInitialized;
        private Type m_systemType;
        private object m_value;

        #endregion

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
            if (s_instanceCount % 1000 == 0)
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
            m_isSimpleType = m_systemType.IsPrimitive
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

        [DebuggerNonUserCode]
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

        [DebuggerNonUserCode]
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
                result = new ValueAccessException("Cannot read the property.", ex.GetBaseException());
            }

            result = AutoWrapIfSpecialType(result);
            return result;
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

            if (!m_isSimpleType)
            {
                if (allowLazy)
                {
                    return;
                }

                var fieldValueMap = new Dictionary<MemberKey, IScriptReturnValue>();
                var fieldInfos = m_systemType.GetFields(c_memberBindingFlags);
                foreach (var fieldInfo in fieldInfos)
                {
                    var fieldValue = GetFieldValueInternal(fieldInfo);
                    var wrappedFieldValue = Create(fieldValue);
                    fieldValueMap.Add(new MemberKey(fieldInfo.Name), wrappedFieldValue);
                }
                m_memberCollections.Add(new MemberCollection(fieldValueMap, "Fields"));

                var propertyValueMap = new Dictionary<MemberKey, IScriptReturnValue>();
                var propertyInfos = m_systemType
                    .GetProperties(c_memberBindingFlags)
                    .Where(item => item.CanRead && !item.GetIndexParameters().Any())
                    .ToArray();
                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = GetPropertyValueInternal(propertyInfo);
                    var wrappedPropertyValue = Create(propertyValue);
                    propertyValueMap.Add(new MemberKey(propertyInfo.Name), wrappedPropertyValue);
                }
                m_memberCollections.Add(new MemberCollection(propertyValueMap, "Properties"));
            }

            if (!(m_value is string))
            {
                var enumerable = m_value as IEnumerable;
                if (enumerable != null)
                {
                    if (allowLazy)
                    {
                        return;
                    }

                    var elementMap = enumerable
                        .Cast<object>()
                        .Select(
                            (item, index) =>
                                new
                                {
                                    Index = index,
                                    Name = string.Format("[{0}]", index),
                                    Value = Create(item)
                                })
                        .ToDictionary(item => new MemberKey(item.Name, item.Index), item => item.Value);
                    m_memberCollections.Add(new MemberCollection(elementMap, "Elements"));
                }
            }

            m_systemType = null;
            m_value = null;
            m_isInitialized = true;
        }

        #endregion

        #region Internal Properties

        internal static int MaxRecursionCount
        {
            [DebuggerStepThrough]
            get { return c_maxRecursionCount; }
        }

        #endregion

        #region Internal Methods

        internal static IScriptReturnValue Create(object value)
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
                    s_objectsBeingProcessed = new Dictionary<ReferenceWrapper, ScriptReturnValueProxy>();
                }
                if (!s_recursionCount.HasValue)
                {
                    recursionCountInitialized = true;
                    s_recursionCount = 0;
                }

                ScriptReturnValueProxy result;

                var isReferenceType = !value.GetType().IsValueType;
                if (isReferenceType)
                {
                    var reference = new ReferenceWrapper(value);
                    if (s_objectsBeingProcessed.TryGetValue(reference, out result))
                    {
                        return result;
                    }
                }

                var internalResult = new ScriptReturnValue(value);
                result = new ScriptReturnValueProxy(internalResult);
                if (isReferenceType)
                {
                    s_objectsBeingProcessed.Add(new ReferenceWrapper(value), result);
                }

                // Initialization must be performed only after reference is added to processed objects map
                s_recursionCount = s_recursionCount.Value + 1;
                try
                {
                    internalResult.Initialize(true);
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

        internal PropertyDescriptor[] GetPropertiesInternal()
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
                predefinedProperties.Add(new ValuePropertyDescriptor(this, s_isNullMemberKey, this.IsNull, false));
            }
            else
            {
                predefinedProperties.Add(new ValuePropertyDescriptor(this, s_typeMemberKey, this.Type, true));
                predefinedProperties.Add(new ValuePropertyDescriptor(this, s_asStringMemberKey, this.AsString, false));
            }

            var runTimeProperties = m_memberCollections
                .Select(item => new ValuePropertyDescriptor(this, new MemberKey(item.DisplayName), item, true));

            return predefinedProperties.Concat(runTimeProperties).ToArray();
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return this.AsString;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region IScriptReturnValue Members

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

        #region ICustomTypeDescriptor Members

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            // This method must not be called
            throw new NotSupportedException();
        }

        #endregion
    }
}