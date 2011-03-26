namespace AjScript.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AjScript.Language;
    using AjScript.Expressions;

    public class FunctionBodyCommand : CompositeCommand
    {
        private ICollection<FunctionExpression> functions;

        public FunctionBodyCommand(ICollection<ICommand> commands, ICollection<FunctionExpression> functions)
            : base(commands)
        {
            this.functions = functions;
        }

        public ICollection<FunctionExpression> Functions { get { return this.functions; } }

        public override void Execute(IContext context)
        {
            // Resolve inner functions as closures
            if (this.functions != null && this.functions.Count > 0) 
            {
                foreach (FunctionExpression expression in this.functions)
                {
                    // TODO review: use context? 
                    IFunction function = (IFunction) expression.Evaluate(context);
                    if (expression.Name != null)
                    {
                        context.DefineVariable(expression.Name);
                        context.SetValue(expression.Name, function);
                    }
                }
            }

            base.Execute(context);
        }
    }
}
