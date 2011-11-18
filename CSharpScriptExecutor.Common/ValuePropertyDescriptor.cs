using System;
using System.Collections;
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
        #region Constants

        private const string c_ownerTypeKey = "ownerType";
        private const string c_nameKey = "name";
        private const string c_orderIndexKey = "orderIndex";
        private const string c_propertyValueKey = "propertyValue";
        private const string c_expandableKey = "expandable";

        #endregion

        #region Fields

        private static readonly Attribute[] s_emptyAttributes = new Attribute[0];

        private static readonly Attribute[] s_expandableAttributes =
            new Attribute[] { new TypeConverterAttribute(typeof(ExpandableObjectConverter)) };

        private readonly Type m_ownerType;
        private readonly object m_propertyValue;
        private readonly bool m_expandable;

        #endregion

        #region Constructors

        internal ValuePropertyDescriptor(
            object owner,
            MemberKey memberKey,
            object propertyValue,
            bool expandable)
            : base(memberKey.Name, expandable ? s_expandableAttributes : s_emptyAttributes)
        {
            #region Argument Check

            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (owner is MarshalByRefObject && !(owner is ScriptReturnValue))
            {
                throw new ArgumentException(
                    string.Format(
                        "The specified value must not be marshalled by reference unless it is '{0}'.",
                        typeof(ScriptReturnValue).FullName),
                    "owner");
            }
            if (propertyValue is MarshalByRefObject)
            {
                throw new ArgumentException(
                    "The specified value must not be marshalled by reference.",
                    "propertyValue");
            }

            #endregion

            m_ownerType = owner.GetType();
            m_propertyValue = propertyValue;
            this.OrderIndex = memberKey.OrderIndex;
            m_expandable = expandable;
        }

        private ValuePropertyDescriptor(SerializationInfo info, StreamingContext context)
            : base(
                info.EnsureNotNull().GetString(c_nameKey),
                info.EnsureNotNull().GetBoolean(c_expandableKey) ? s_expandableAttributes : s_emptyAttributes)
        {
            #region Argument Check

            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            #endregion

            m_ownerType = (Type)info.GetValue(c_ownerTypeKey, typeof(Type));
            m_propertyValue = info.GetValue(c_propertyValueKey, typeof(object));
            this.OrderIndex = info.GetInt32(c_orderIndexKey);
            m_expandable = info.GetBoolean(c_expandableKey);
        }

        #endregion

        #region Public Properties

        public override Type ComponentType
        {
            [DebuggerNonUserCode]
            get { return m_ownerType; }
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

        public int OrderIndex
        {
            get;
            private set;
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
            return m_propertyValue;
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

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            #region Argument Check

            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            #endregion

            info.AddValue(c_ownerTypeKey, m_ownerType);
            info.AddValue(c_nameKey, this.Name);
            info.AddValue(c_orderIndexKey, this.OrderIndex);
            info.AddValue(c_propertyValueKey, m_propertyValue);
            info.AddValue(c_expandableKey, m_expandable);
        }

        #endregion
    }
}