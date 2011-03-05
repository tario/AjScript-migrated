namespace AjScript.Expressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using AjScript.Commands;
    using AjScript.Language;
    using AjScript.Expressions;

    public class InvokeExpression : IExpression
    {
        private int nvariable;
        private ICollection<IExpression> arguments;

        public InvokeExpression(int nvariable, ICollection<IExpression> arguments)
        {
            this.nvariable = nvariable;
            this.arguments = arguments;
        }

        public int NVariable { get { return this.nvariable; } }

        public ICollection<IExpression> Arguments { get { return this.arguments; } }

        public object Evaluate(IContext context)
        {
            ICallable callable = (ICallable)context.GetValue(this.nvariable);

            List<object> parameters = new List<object>();

            foreach (IExpression expression in this.arguments)
            {
                object parameter = expression.Evaluate(context);

                parameters.Add(parameter);
            }

            return callable.Invoke(parameters.ToArray());
        }
    }
}
