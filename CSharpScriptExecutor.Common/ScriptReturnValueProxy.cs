using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    [ImmutableObject(true)]
    public sealed class ScriptReturnValueProxy : IScriptReturnValue
    {
        #region Fields

        private readonly ScriptReturnValue m_scriptReturnValue;
        private readonly string m_className;
        private readonly string m_componentName;
        private PropertyDescriptorCollection m_properties;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScriptReturnValueProxy"/> class.
        /// </summary>
        internal ScriptReturnValueProxy(ScriptReturnValue scriptReturnValue)
        {
            #region Argument Check

            if (scriptReturnValue == null)
            {
                throw new ArgumentNullException("scriptReturnValue");
            }

            #endregion

            m_scriptReturnValue = scriptReturnValue;
            m_className = TypeDescriptor.GetClassName(scriptReturnValue, true);
            m_componentName = TypeDescriptor.GetComponentName(scriptReturnValue, true);

            this.AsString = scriptReturnValue.AsString;
            this.IsNull = scriptReturnValue.IsNull;
            this.Type = scriptReturnValue.Type;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return this.AsString;
        }

        #endregion

        #region IScriptReturnValue Members

        public string AsString
        {
            get;
            private set;
        }

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

        #endregion

        #region ICustomTypeDescriptor Members

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return m_className;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return m_componentName;
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
            if (m_properties == null)
            {
                var proxyResult = m_scriptReturnValue.GetPropertiesInternal();
                m_properties = new PropertyDescriptorCollection(proxyResult, true);
            }
            return m_properties;
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }
}