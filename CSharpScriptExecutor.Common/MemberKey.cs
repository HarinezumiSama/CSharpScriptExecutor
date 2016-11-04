using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    internal struct MemberKey
    {
        internal MemberKey(string name, int orderIndex = 0)
            : this()
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            OrderIndex = orderIndex;
        }

        public string Name
        {
            get;
            private set;
        }

        public int OrderIndex
        {
            get;
            private set;
        }
    }
}