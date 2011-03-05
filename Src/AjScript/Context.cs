namespace AjScript
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Context : IContext
    {
        private object[] values;

        public Context(int nvariables)
        {
            this.values = new object[nvariables];
        }

        public Context(IContext parent, int nvariables)
        {
            throw new NotImplementedException();
        }

        public void SetValue(int nvariable, object value)
        {
            this.values[nvariable] = value;
        }

        public object GetValue(int nvariable)
        {
            return this.values[nvariable];
        }
    }
}
