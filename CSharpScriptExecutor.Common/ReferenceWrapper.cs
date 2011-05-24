using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSharpScriptExecutor.Common
{
    internal struct ReferenceWrapper : IEquatable<ReferenceWrapper>
    {
        #region Fields

        private static readonly ReferenceWrapper s_null = new ReferenceWrapper();

        private readonly object m_value;
        private readonly IntPtr m_address;

        #endregion

        #region Constructors

        internal ReferenceWrapper(object value)
        {
            if (value is Pointer)
            {
                m_value = null;
                unsafe
                {
                    m_address = new IntPtr(Pointer.Unbox(value));
                }
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
}