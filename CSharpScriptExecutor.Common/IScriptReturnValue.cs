using System;
using System.ComponentModel;

namespace CSharpScriptExecutor.Common
{
    public interface IScriptReturnValue : ICustomTypeDescriptor
    {
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
    }
}