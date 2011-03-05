namespace AjScript
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IContext
    {
        void SetValue(int nvariable, object value);

        object GetValue(int nvariable);
    }
}
