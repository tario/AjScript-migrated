namespace AjScript.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AjScript.Expressions;

    public class SetLocalVariableCommand : ICommand
    {
        private int nvariable;
        private IExpression expression;

        public SetLocalVariableCommand(int nvariable, IExpression expression)
        {
            this.expression = expression;
            this.nvariable = nvariable;
        }

        public int NVariable { get { return this.nvariable; } }

        public IExpression Expression { get { return this.expression; } }

        public void Execute(IContext context)
        {
            context.SetValue(this.nvariable, this.expression.Evaluate(context));
        }
    }
}
