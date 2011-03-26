namespace AjScript.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CompositeCommand : ICommand
    {
        private ICollection<ICommand> commands;

        public CompositeCommand(ICollection<ICommand> commands)
        {
            this.commands = commands;
        }

        public int CommandCount { get { return this.commands.Count; } }

        public ICollection<ICommand> Commands { get { return this.commands; } }

        public virtual void Execute(IContext context)
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
