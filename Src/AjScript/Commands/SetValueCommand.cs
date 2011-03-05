namespace AjScript.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using AjScript.Expressions;
    using AjScript.Language;

    [Serializable]
    public class SetValueCommand : ICommand
    {
        private IExpression leftValue;
        private IExpression expression;

        public SetValueCommand(IExpression leftValue, IExpression expression)
        {
            this.leftValue = leftValue;
            this.expression = expression;
        }

        public IExpression LeftValue { get { return this.leftValue; } }

        public IExpression Expression { get { return this.expression; } }

        public void Execute(IContext context)
        {
            object leftvalue = this.LeftValue.Evaluate(context);
            object value = this.expression.Evaluate(context);

            ((IReference)leftvalue).SetValue(value);
        }
    }
}
