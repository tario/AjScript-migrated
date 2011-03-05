namespace AjScript.Commands
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Text;

    using AjScript.Expressions;

    public class ForCommand : ICommand
    {
        private ICommand initialCommand;
        private IExpression condition;
        private ICommand body;
        private ICommand endCommand;
        private int nvariables;

        public ForCommand(ICommand initialCommand, IExpression condition, ICommand endCommand, ICommand body, int nvariables)
        {
            this.initialCommand = initialCommand;
            this.condition = condition;
            this.endCommand = endCommand;
            this.body = body;
            this.nvariables = nvariables;
        }

        public ICommand InitialCommand { get { return this.initialCommand; } }

        public IExpression Condition { get { return this.condition; } }

        public ICommand EndCommand { get { return this.endCommand; } }

        public ICommand Body { get { return this.body; } }

        public int NVariables { get { return this.nvariables; } }

        public void Execute(IContext context)
        {
            IContext newContext = new Context(context, this.nvariables);

            if (this.initialCommand != null)
                this.initialCommand.Execute(newContext);

            while (this.condition == null || Predicates.IsTrue(this.condition.Evaluate(newContext)))
            {
                if (this.body != null)
                    this.body.Execute(newContext);
                if (this.endCommand != null)
                    this.endCommand.Execute(newContext);
            }
        }
    }
}
