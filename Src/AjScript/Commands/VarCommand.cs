namespace AjScript.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using AjScript.Expressions;
    using AjScript.Language;

    public class VarCommand : ICommand
    {
        private string name;
        private IExpression expression;

        public VarCommand(string name, IExpression expression)
        {
            this.name = name;
            this.expression = expression;
        }

        public string Name { get { return this.name; } }

        public IExpression Expression { get { return this.expression; } }

        public void Execute(IContext context)
        {
            context.DefineVariable(this.name);

            if (this.expression == null)
                return;

            object value = this.expression.Evaluate(context);

            context.SetValue(this.name, value);
        }
    }
}
