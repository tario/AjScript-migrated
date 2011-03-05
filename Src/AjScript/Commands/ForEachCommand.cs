namespace AjScript.Commands
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Text;

    using AjScript.Expressions;

    public class ForEachCommand : ICommand
    {
        private IExpression expression;
        private ICommand command;
        private bool localvar;

        public ForEachCommand(IExpression expression, ICommand command)
            : this(expression, command, false)
        {
        }

        public ForEachCommand(IExpression expression, ICommand command, bool localvar)
        {
            this.expression = expression;
            this.command = command;
            this.localvar = localvar;
        }

        public IExpression Expression { get { return this.expression; } }

        public ICommand Command { get { return this.command; } }

        public bool LocalVariable { get { return this.localvar; } }

        public void Execute(IContext context)
        {
            IContext newContext = context;

            if (this.localvar)
            {
                newContext = new Context(context, 1);
                newContext.SetValue(0, null);
            }

            foreach (object result in (IEnumerable) this.expression.Evaluate(newContext))
            {
                newContext.SetValue(0, result);
                this.command.Execute(newContext);
            }
        }
    }
}
