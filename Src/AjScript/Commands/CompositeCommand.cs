namespace AjScript.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CompositeCommand : ICommand
    {
        private ICollection<ICommand> commands;
        private int nvariables;

        public CompositeCommand(ICollection<ICommand> commands, int nvariables)
        {
            this.commands = commands;
            this.nvariables = nvariables;
        }

        public int CommandCount { get { return this.commands.Count; } }

        public ICollection<ICommand> Commands { get { return this.commands; } }

        public void Execute(IContext context)
        {
            foreach (ICommand command in this.commands)
            {
                command.Execute(context);

                if (context.ReturnValue != null)
                    return;
            }
        }
    }
}
