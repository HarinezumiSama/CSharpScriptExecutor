using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CSharpScriptExecutor.Common
{
    [ImmutableObject(true)]
    [Serializable]
    public sealed class ScriptReturnValue : MarshalByRefObject, IScriptReturnValue
    {
        #region MemberCollection Class

        [Serializable]
        [ImmutableObject(true)]
        private sealed class MemberCollection : ICustomTypeDescriptor
        {
            #region Fields

            private readonly Dictionary<MemberKey, IScriptReturnValue> _valueMap;
            private readonly IScriptReturnValue _error;

            #endregion

            #region Constructors

            internal MemberCollection(string displayName, Dictionary<MemberKey, IScriptReturnValue> valueMap)
            {
                #region Argument Check

                if (string.IsNullOrEmpty(displayName))
                {
                    throw new ArgumentException("The value can be neither empty string nor null.", nameof(displayName));
                }
                if (valueMap == null)
                {
                    throw new ArgumentNullException(nameof(valueMap));
                }

                #endregion

                DisplayName = displayName;
                _valueMap = valueMap;
            }

            internal MemberCollection(string displayName, Exception error)
            {
                #region Argument Check

                if (string.IsNullOrEmpty(displayName))
                {
                    throw new ArgumentException("The value can be neither empty string nor null.", nameof(displayName));
                }
                if (error == null)
                {
                    throw new ArgumentNullException(nameof(error));
                }

                #endregion

                DisplayName = displayName;
                _error = Create(error);
            }

            #endregion

            #region Public Properties

            public string DisplayName
            {
                [DebuggerStepThrough]
                get;
            }

            #endregion

            #region Public Methods

            public override string ToString()
            {
                return _error == null
                    ? (_valueMap == null ? "?" : $@"Count = {_valueMap.Count}")
                    : _error.AsString;
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
                var properties = _valueMap == null || _error != null
                    ? new PropertyDescriptor[] { new ValuePropertyDescriptor(this, ErrorMemberKey, _error, true) }
                    : _valueMap
                        .Select(pair => new ValuePropertyDescriptor(this, pair.Key, pair.Value, true))
                        .OrderBy(item => item.OrderIndex)
                        .ThenBy(item => item.DisplayName)
                        .ThenBy(item => item.Name)
                        .ToArray();

                return new PropertyDescriptorCollection(properties, true);
            }

            object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) => this;

            #endregion
        }

        #endregion

        #region IHexInfo Interface

        private interface IHexInfo
        {
            #region Public

            Type Type
            {
                get;
            }

            Func<object, object> Preconvert
            {
                get;
            }

            string Format
            {
                get;
            }

            #endregion
        }

        #endregion

        #region HexInfo Class

        private sealed class HexInfo<T> : IHexInfo
            where T : struct
        {
            #region Constructors

            internal HexInfo(Func<object, object> preconvert = null, int? size = null)
            {
                #region Argument Check

                if (size.HasValue && size.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be positive, if specified.");
                }

                #endregion

                Type = typeof(T);
                Preconvert = preconvert ?? (x => (T)x);
                var actualSize = size ?? Marshal.SizeOf(Type);
                Format = string.Format("0x{{0:X{0}}}", actualSize * 2);
            }

            internal HexInfo()
                : this(null)
            {
                // Nothing to do
            }

            #endregion

            #region IHexInfo Members

            public Type Type
            {
                get;
            }

            public Func<object, object> Preconvert
            {
                get;
            }

            public string Format
            {
                get;
            }

            #endregion
        }

        #endregion

        #region Constants

        private const BindingFlags MemberBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static readonly int MaxRecursionCount = 128;

        private const string FieldsPropertyName = "Fields";
        private const string PropertiesPropertyName = "Properties";
        private const string ElementsPropertyName = "Elements";

        #endregion

        #region Fields

        private static readonly ScriptReturnValueProxy Null =
            new ScriptReturnValueProxy(new ScriptReturnValue(null));
        private static readonly string ToStringMethodName = new Func<string>(new object().ToString).Method.Name;
        private static readonly string PointerStringFormat = GetPointerStringFormat();

        private static readonly Dictionary<Type, IHexInfo> HexInfoMap =
            new IHexInfo[]
            {
                new HexInfo<char>(x => (ushort)(char)x, sizeof(char)),
                new HexInfo<byte>(),
                new HexInfo<sbyte>(),
                new HexInfo<short>(),
                new HexInfo<ushort>(),
                new HexInfo<int>(),
                new HexInfo<uint>(),
                new HexInfo<long>(),
                new HexInfo<ulong>(),
                new HexInfo<IntPtr>(x => ((IntPtr)x).ToInt64(), IntPtr.Size),
                new HexInfo<UIntPtr>(x => ((UIntPtr)x).ToUInt64(), UIntPtr.Size)
            }
            .ToDictionary(item => item.Type);

        private static readonly MemberKey IsNullMemberKey = new MemberKey("(IsNull)");
        private static readonly MemberKey TypeMemberKey = new MemberKey("(Type)");
        private static readonly MemberKey AsStringMemberKey = new MemberKey("(AsString)");
        private static readonly MemberKey AsHexadecimalMemberKey = new MemberKey("(AsHexadecimal)");
        private static readonly MemberKey ErrorMemberKey = new MemberKey("[Error]");

        [ThreadStatic]
        private static Dictionary<ReferenceWrapper, ScriptReturnValueProxy> _objectsBeingProcessed;

        [ThreadStatic]
        private static int? _recursionCount;

        [ThreadStatic]
        private static ulong _instanceCount;

        private readonly List<MemberCollection> _memberCollections = new List<MemberCollection>();
        private readonly bool _isSimpleType;
        private readonly string _hexValue;
        private bool _isInitialized;
        private Type _systemType;
        private object _value;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptReturnValue"/> class.
        /// </summary>
        private ScriptReturnValue(object value)
        {
            checked
            {
                _instanceCount++;
            }
            if (_instanceCount % 1000 == 0)
            {
                Debug.WriteLine("{0} instances: {1}+", GetType().Name, _instanceCount);
            }

            if (ReferenceEquals(value, null))
            {
                IsNull = true;
                _isSimpleType = true;
                AsString = "<null>";
                _isInitialized = true;
                return;
            }

            _value = value;
            _systemType = value.GetType();

            Type = new TypeWrapper(_systemType);
            _isSimpleType = _systemType.IsPrimitive
                || _systemType.IsEnum
                || _systemType.IsPointer
                || _systemType == typeof(char)
                || _systemType == typeof(string)
                || _systemType == typeof(decimal)
                || _systemType == typeof(Pointer)
                || typeof(Delegate).IsAssignableFrom(_systemType);

            _hexValue = GetHexValue();

            try
            {
                if (_systemType == typeof(Pointer))
                {
                    unsafe
                    {
                        AsString = string.Format(PointerStringFormat, (long)Pointer.Unbox(value));
                    }
                }
                else
                {
                    AsString = value.ToString();
                }
            }
            catch (Exception ex)
            {
                AsString = string.Format(
                    "An exception {0} occurred on calling value's {1} method: {2}",
                    ex.GetType().FullName,
                    ToStringMethodName,
                    ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private string GetHexValue()
        {
            var actualType = _systemType.IsEnum ? Enum.GetUnderlyingType(_systemType) : _systemType;

            IHexInfo hexInfo;
            if (!HexInfoMap.TryGetValue(actualType, out hexInfo))
            {
                return null;
            }

            string result;
            try
            {
                var convertedValue = hexInfo.Preconvert(_value);
                result = string.Format(hexInfo.Format, convertedValue);
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

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

            if (_recursionCount.GetValueOrDefault() > MaxRecursionCount)
            {
                result = ValueAccessException.FieldRecursionLimitExceeded;
                return result;
            }

            result = fieldInfo.GetValue(_value);
            result = AutoWrapIfSpecialType(result);
            return result;
        }

        [DebuggerNonUserCode]
        private object GetPropertyValueInternal(PropertyInfo propertyInfo)
        {
            object result;

            if (_recursionCount.GetValueOrDefault() > MaxRecursionCount)
            {
                result = ValueAccessException.PropertyRecursionLimitExceeded;
                return result;
            }

            try
            {
                result = propertyInfo.GetValue(_value, null);
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
            if (_isInitialized)
            {
                return;
            }

            if (_systemType == null || _value == null)
            {
                throw new InvalidOperationException("Initialization is called too late.");
            }

            if (!_isSimpleType)
            {
                if (allowLazy)
                {
                    return;
                }

                var fieldValueMap = new Dictionary<MemberKey, IScriptReturnValue>();
                var fieldInfos = _systemType.GetFields(MemberBindingFlags);
                foreach (var fieldInfo in fieldInfos)
                {
                    var fieldValue = GetFieldValueInternal(fieldInfo);
                    var wrappedFieldValue = Create(fieldValue);
                    fieldValueMap.Add(new MemberKey(fieldInfo.Name), wrappedFieldValue);
                }

                _memberCollections.Add(new MemberCollection(FieldsPropertyName, fieldValueMap));

                var propertyValueMap = new Dictionary<MemberKey, IScriptReturnValue>();
                var propertyInfos = _systemType
                    .GetProperties(MemberBindingFlags)
                    .Where(item => item.CanRead && !item.GetIndexParameters().Any())
                    .ToArray();
                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = GetPropertyValueInternal(propertyInfo);
                    var wrappedPropertyValue = Create(propertyValue);
                    propertyValueMap.Add(new MemberKey(propertyInfo.Name), wrappedPropertyValue);
                }

                _memberCollections.Add(new MemberCollection(PropertiesPropertyName, propertyValueMap));
            }

            if (!(_value is string))
            {
                var enumerable = _value as IEnumerable;
                if (enumerable != null)
                {
                    if (allowLazy)
                    {
                        return;
                    }

                    MemberCollection elementMemberCollection;
                    try
                    {
                        var initialCount = 0;
                        var collection = enumerable as ICollection;
                        if (collection != null)
                        {
                            try
                            {
                                initialCount = collection.Count;
                            }
                            catch (Exception)
                            {
                                // Nothing to do
                            }
                        }

                        var enumerator = enumerable.GetEnumerator();
                        var items = new List<object>(initialCount);

                        while (enumerator.MoveNext())
                        {
                            object item;
                            try
                            {
                                item = enumerator.Current;
                            }
                            catch (Exception ex)
                            {
                                item = new ValueAccessException(
                                    "Cannot obtain collection's element.",
                                    ex.GetBaseException());
                            }

                            items.Add(item);
                        }

                        var elementMap = items
                            .Select(
                                (item, index) =>
                                new
                                {
                                    Index = index,
                                    Name = string.Format("[{0}]", index),
                                    Value = Create(item)
                                })
                            .ToDictionary(item => new MemberKey(item.Name, item.Index), item => item.Value);
                        elementMemberCollection = new MemberCollection(ElementsPropertyName, elementMap);
                    }
                    catch (Exception ex)
                    {
                        var error = new ValueAccessException(
                            "Cannot enumerate the collection.",
                            ex.GetBaseException());
                        elementMemberCollection = new MemberCollection(ElementsPropertyName, error);
                    }

                    _memberCollections.Add(elementMemberCollection);
                }
            }

            _systemType = null;
            _value = null;
            _isInitialized = true;
        }

        #endregion

        #region Internal Methods

        internal static IScriptReturnValue Create(object value)
        {
            if (ReferenceEquals(value, null))
            {
                return Null;
            }

            var objectsBeingProcessedCreated = false;
            var recursionCountInitialized = false;
            try
            {
                if (_objectsBeingProcessed == null)
                {
                    objectsBeingProcessedCreated = true;
                    _objectsBeingProcessed = new Dictionary<ReferenceWrapper, ScriptReturnValueProxy>();
                }
                if (!_recursionCount.HasValue)
                {
                    recursionCountInitialized = true;
                    _recursionCount = 0;
                }

                ScriptReturnValueProxy result;

                var isReferenceType = !value.GetType().IsValueType;
                if (isReferenceType)
                {
                    var reference = new ReferenceWrapper(value);
                    if (_objectsBeingProcessed.TryGetValue(reference, out result))
                    {
                        return result;
                    }
                }

                var internalResult = new ScriptReturnValue(value);
                result = new ScriptReturnValueProxy(internalResult);
                if (isReferenceType)
                {
                    _objectsBeingProcessed.Add(new ReferenceWrapper(value), result);
                }

                // Initialization must be performed only after reference is added to processed objects map
                _recursionCount = _recursionCount.Value + 1;
                try
                {
                    internalResult.Initialize(true);
                }
                finally
                {
                    _recursionCount = _recursionCount.Value - 1;
                }

                return result;
            }
            finally
            {
                if (recursionCountInitialized)
                {
                    _recursionCount = null;
                }
                if (objectsBeingProcessedCreated)
                {
                    _objectsBeingProcessed = null;
                }
            }
        }

        internal PropertyDescriptor[] GetPropertiesInternal()
        {
            Initialize(false);
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    string.Format("{0}: initialization has failed.", GetType().FullName));
            }

            var predefinedProperties = new List<PropertyDescriptor>();
            if (IsNull)
            {
                predefinedProperties.Add(new ValuePropertyDescriptor(this, IsNullMemberKey, IsNull, false));
            }
            else
            {
                predefinedProperties.Add(new ValuePropertyDescriptor(this, TypeMemberKey, Type, true));
                predefinedProperties.Add(new ValuePropertyDescriptor(this, AsStringMemberKey, AsString, false));
                if (!string.IsNullOrEmpty(_hexValue))
                {
                    predefinedProperties.Add(
                        new ValuePropertyDescriptor(this, AsHexadecimalMemberKey, _hexValue, false));
                }
            }

            var runTimeProperties = _memberCollections
                .Select(item => new ValuePropertyDescriptor(this, new MemberKey(item.DisplayName), item, true));

            return predefinedProperties.Concat(runTimeProperties).ToArray();
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return AsString;
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
        }

        public TypeWrapper Type
        {
            get;
        }

        public string AsString
        {
            get;
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