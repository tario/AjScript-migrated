namespace AjScript.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class LocalVariableExpression : IExpression
    {
        private int nvariable;

        public LocalVariableExpression(int nvariable)
        {
            this.nvariable = nvariable;
        }

        public int NVariable { get { return this.nvariable; } }

        public object Evaluate(IContext context)
        {
            return context.GetValue(this.nvariable);
        }
    }
}
