using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [ImmutableObject(true)]
    public sealed class TypeWrapper
    {
        private readonly string _asString;

        internal TypeWrapper(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Name = type.Name;
            FullName = type.FullName;
            AssemblyName = type.Assembly.GetName().Name;
            AssemblyFullName = type.Assembly.GetName().FullName;
            AssemblyQualifiedName = type.AssemblyQualifiedName;
            _asString = type.ToString();
        }

        public string Name
        {
            get;
        }

        public string FullName
        {
            get;
        }

        public string AssemblyName
        {
            get;
        }

        public string AssemblyFullName
        {
            get;
        }

        public string AssemblyQualifiedName
        {
            get;
        }

        public override string ToString() => _asString;
    }
}