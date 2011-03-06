namespace AjScript.Commands
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Text;

    using AjScript.Expressions;

    public class ForEachCommand : ICommand
    {
        private int nvariable;
        private IExpression expression;
        private ICommand command;

        public ForEachCommand(int nvariable, IExpression expression, ICommand command)
        {
            this.nvariable = nvariable;
            this.expression = expression;
            this.command = command;
        }

        public int NVariable { get { return this.nvariable; } }

        public IExpression Expression { get { return this.expression; } }

        public ICommand Command { get { return this.command; } }

        public void Execute(IContext context)
        {
            foreach (object result in (IEnumerable) this.expression.Evaluate(context))
            {
                context.SetValue(this.nvariable, result);
                this.command.Execute(context);
            }
        }
    }
}
