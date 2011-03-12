namespace AjScript
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AjScript.Language;

    public interface IContext
    {
        ReturnValue ReturnValue { get; set;  }

        void SetValue(int nvariable, object value);

        void SetValue(string name, object value);

        object GetValue(int nvariable);

        object GetValue(string name);

        int DefineVariable(string name);

        int GetVariableOffset(string name);
    }
}

