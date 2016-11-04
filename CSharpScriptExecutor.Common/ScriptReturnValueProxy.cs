using System;
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

        private readonly ScriptReturnValue _scriptReturnValue;
        private readonly string _className;
        private readonly string _componentName;
        private PropertyDescriptorCollection _properties;

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
                throw new ArgumentNullException(nameof(scriptReturnValue));
            }

            #endregion

            _scriptReturnValue = scriptReturnValue;
            _className = TypeDescriptor.GetClassName(scriptReturnValue, true);
            _componentName = TypeDescriptor.GetComponentName(scriptReturnValue, true);

            AsString = scriptReturnValue.AsString;
            IsNull = scriptReturnValue.IsNull;
            Type = scriptReturnValue.Type;
        }

        #endregion

        #region Public Methods

        public override string ToString() => AsString;

        #endregion

        #region IScriptReturnValue Members

        public string AsString
        {
            get;
        }

        public bool IsNull
        {
            get;
        }

        public TypeWrapper Type
        {
            get;
        }

        #endregion

        #region ICustomTypeDescriptor Members

        AttributeCollection ICustomTypeDescriptor.GetAttributes() => TypeDescriptor.GetAttributes(this, true);

        string ICustomTypeDescriptor.GetClassName() => _className;

        string ICustomTypeDescriptor.GetComponentName() => _componentName;

        TypeConverter ICustomTypeDescriptor.GetConverter() => TypeDescriptor.GetConverter(this, true);

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(this, true);

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(this, true);

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(this, editorBaseType, true);

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(this, attributes, true);

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() => TypeDescriptor.GetEvents(this, true);

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) => ((ICustomTypeDescriptor)this).GetProperties();

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            if (_properties == null)
            {
                var proxyResult = _scriptReturnValue.GetPropertiesInternal();
                _properties = new PropertyDescriptorCollection(proxyResult, true);
            }

            return _properties;
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) => this;

        #endregion
    }
}