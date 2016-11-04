using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    internal sealed class ValuePropertyDescriptor : PropertyDescriptor, ISerializable
    {
        private const string OwnerTypeKey = "ownerType";
        private const string NameKey = "name";
        private const string OrderIndexKey = "orderIndex";
        private const string PropertyValueKey = "propertyValue";
        private const string ExpandableKey = "expandable";

        private static readonly Attribute[] EmptyAttributes = new Attribute[0];

        private static readonly Attribute[] ExpandableAttributes =
        {
            new TypeConverterAttribute(typeof(ExpandableObjectConverter))
        };

        private readonly Type _ownerType;
        private readonly object _propertyValue;
        private readonly bool _expandable;

        internal ValuePropertyDescriptor(
            object owner,
            MemberKey memberKey,
            object propertyValue,
            bool expandable)
            : base(memberKey.Name, expandable ? ExpandableAttributes : EmptyAttributes)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (owner is MarshalByRefObject && !(owner is ScriptReturnValue))
            {
                throw new ArgumentException(
                    $@"The specified value must not be marshalled by reference unless it is '{
                        typeof(ScriptReturnValue).FullName}'.",
                    nameof(owner));
            }

            if (propertyValue is MarshalByRefObject)
            {
                throw new ArgumentException(
                    "The specified value must not be marshalled by reference.",
                    nameof(propertyValue));
            }

            _ownerType = owner.GetType();
            _propertyValue = propertyValue;
            OrderIndex = memberKey.OrderIndex;
            _expandable = expandable;
        }

        private ValuePropertyDescriptor(SerializationInfo info, StreamingContext context)
            : base(
                info.EnsureNotNull().GetString(NameKey),
                info.EnsureNotNull().GetBoolean(ExpandableKey) ? ExpandableAttributes : EmptyAttributes)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            _ownerType = (Type)info.GetValue(OwnerTypeKey, typeof(Type));
            _propertyValue = info.GetValue(PropertyValueKey, typeof(object));
            OrderIndex = info.GetInt32(OrderIndexKey);
            _expandable = info.GetBoolean(ExpandableKey);
        }

        public override Type ComponentType
        {
            [DebuggerNonUserCode]
            get
            {
                return _ownerType;
            }
        }

        public override bool IsReadOnly
        {
            [DebuggerStepThrough]
            get
            {
                return true;
            }
        }

        public override Type PropertyType
        {
            [DebuggerNonUserCode]
            get
            {
                return _propertyValue?.GetType();
            }
        }

        public int OrderIndex
        {
            get;
        }

        [DebuggerNonUserCode]
        public override bool CanResetValue(object component) => false;

        [DebuggerNonUserCode]
        public override object GetValue(object component) => _propertyValue;

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
        public override bool ShouldSerializeValue(object component) => false;

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(OwnerTypeKey, _ownerType);
            info.AddValue(NameKey, Name);
            info.AddValue(OrderIndexKey, OrderIndex);
            info.AddValue(PropertyValueKey, _propertyValue);
            info.AddValue(ExpandableKey, _expandable);
        }
    }
}