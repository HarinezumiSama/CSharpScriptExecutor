using System;
using System.Collections;
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
        #region Fields

        private readonly string m_asString;

        #endregion

        #region Constructors

        internal TypeWrapper(Type type)
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
}