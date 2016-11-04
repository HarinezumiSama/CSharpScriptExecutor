using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSharpScriptExecutor.Common
{
    internal struct ReferenceWrapper : IEquatable<ReferenceWrapper>
    {
        public static readonly ReferenceWrapper Null = new ReferenceWrapper();

        internal ReferenceWrapper(object value)
        {
            if (value is Pointer)
            {
                Value = null;
                unsafe
                {
                    Address = new IntPtr(Pointer.Unbox(value));
                }
            }
            else
            {
                Value = value;
                Address = IntPtr.Zero;
            }
        }

        public object Value
        {
            [DebuggerStepThrough]
            get;
        }

        public IntPtr Address
        {
            [DebuggerStepThrough]
            get;
        }

        public override bool Equals(object obj) => obj is ReferenceWrapper && Equals((ReferenceWrapper)obj);

        public override int GetHashCode() => RuntimeHelpers.GetHashCode(Value) ^ Address.GetHashCode();

        public override string ToString() => $@"{{{GetType().Name}. Value = {{{Value}}}, Address = {Address}}}";

        public bool Equals(ReferenceWrapper other) => ReferenceEquals(other.Value, Value) && other.Address == Address;
    }
}