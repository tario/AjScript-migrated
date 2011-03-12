namespace AjScript
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AjScript.Language;

    public class Context : IContext
    {
        private IList<object> values;
        private Dictionary<string, int> positions;
        private IContext parent;

        public Context(int nvariables)
            : this(null, nvariables)
        {
        }

        public Context(IContext parent, int nvariables)
        {
            this.parent = parent;
            this.values = new List<object>(nvariables);

            for (int k = 0; k < nvariables; k++)
                this.values.Add(Undefined.Instance);
        }

        public ReturnValue ReturnValue { get; set; }

        public void SetValue(int nvariable, object value)
        {
            this.values[nvariable] = value;
        }

        public object GetValue(int nvariable)
        {
            return this.values[nvariable];
        }

        public object GetValue(string name)
        {
            if (this.positions == null || !this.positions.ContainsKey(name))
                return Undefined.Instance;

            return this.values[this.positions[name]];
        }

        public void SetValue(string name, object value)
        {
            this.values[this.positions[name]] = value;
        }

        public int DefineVariable(string name)
        {
            if (this.positions == null)
                this.positions = new Dictionary<string, int>();

            this.positions[name] = this.values.Count;
            this.values.Add(Undefined.Instance);
            return this.positions[name];
        }

        public int GetVariableOffset(string name)
        {
            if (this.positions == null || !this.positions.ContainsKey(name))
                return -1;

            return this.positions[name];
        }
    }
}
