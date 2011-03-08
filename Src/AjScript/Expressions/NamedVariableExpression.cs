namespace AjScript.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class NamedVariableExpression : IExpression
    {
        private string name;

        public NamedVariableExpression(string name)
        {
            this.name = name;
        }

        public string Name { get { return this.name; } }

        public object Evaluate(IContext context)
        {
            return context.GetValue(this.name);
        }
    }
}
