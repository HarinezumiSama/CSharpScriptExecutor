using System;
using System.ComponentModel;

namespace CSharpScriptExecutor.Common
{
    public interface IScriptReturnValue : ICustomTypeDescriptor
    {
        #region Properties

        string AsString
        {
            get;
        }

        bool IsNull
        {
            get;
        }

        TypeWrapper Type
        {
            get;
        }

        #endregion
    }
}