﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharpScriptExecutor.Common
{
    [Serializable]
    internal struct MemberKey
    {
        #region Constructors

        internal MemberKey(string name, int orderIndex = 0)
            : this()
        {
            #region Fields

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            #endregion

            this.Name = name;
            this.OrderIndex = orderIndex;
        }

        #endregion

        #region Public Properties

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

        #endregion
    }
}