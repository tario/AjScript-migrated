namespace AjScript.Commands.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using AjScript;
    using AjScript.Commands;
    using AjScript.Expressions;
    using AjScript.Language;

    [TestClass]
    public class CommandsTests
    {
        [TestMethod]
        public void ExecuteCompositeCommand()
        {
            Context context = new Context(3);

            SetLocalVariableCommand command1 = new SetLocalVariableCommand(0, new ConstantExpression("bar"));
            SetLocalVariableCommand command2 = new SetLocalVariableCommand(1, new ConstantExpression(1));
            SetLocalVariableCommand command3 = new SetLocalVariableCommand(2, new LocalVariableExpression(0));

            List<ICommand> commands = new List<ICommand>();
            commands.Add(command1);
            commands.Add(command2);
            commands.Add(command3);

            CompositeCommand command = new CompositeCommand(commands, 0);

            context.SetValue(0, null);
            context.SetValue(1, null);

            command.Execute(context);

            Assert.AreEqual("bar", context.GetValue(0));
            Assert.AreEqual(1, context.GetValue(1));
            Assert.AreEqual("bar", context.GetValue(2));
        }

        [TestMethod]
        public void ExecuteIfCommandWhenTrue()
        {
            IExpression condition = new ConstantExpression(true);
            ICommand setCommand = new SetLocalVariableCommand(0, new ConstantExpression(1));
            IfCommand command = new IfCommand(condition, setCommand);

            Context context = new Context(1);

            command.Execute(context);

            Assert.AreEqual(1, context.GetValue(0));
        }

        [TestMethod]
        public void ExecuteIfCommandWhenFalse()
        {
            IExpression condition = new ConstantExpression(false);
            ICommand setCommand = new SetLocalVariableCommand(0, new ConstantExpression(1));
            IfCommand command = new IfCommand(condition, setCommand);

            Context context = new Context(1);

            command.Execute(context);

            Assert.AreEqual(Undefined.Instance, context.GetValue(0));
        }

        [TestMethod]
        public void ExecuteIfCommandElseWhenFalse()
        {
            IExpression condition = new ConstantExpression(false);
            ICommand setXCommand = new SetLocalVariableCommand(0, new ConstantExpression(1));
            ICommand setYCommand = new SetLocalVariableCommand(1, new ConstantExpression(2));
            IfCommand command = new IfCommand(condition, setXCommand, setYCommand);

            Context context = new Context(2);

            command.Execute(context);

            Assert.AreEqual(Undefined.Instance, context.GetValue(0));
            Assert.AreEqual(2, context.GetValue(1));
        }

        [TestMethod]
        public void ExecuteWhileCommand()
        {
            IExpression incrementX = new ArithmeticBinaryExpression(ArithmeticOperator.Add, new ConstantExpression(1), new LocalVariableExpression(0));
            IExpression decrementY = new ArithmeticBinaryExpression(ArithmeticOperator.Subtract, new LocalVariableExpression(1), new ConstantExpression(1));
            ICommand setX = new SetLocalVariableCommand(0, incrementX);
            ICommand setY = new SetLocalVariableCommand(1, decrementY);
            List<ICommand> commands = new List<ICommand>();
            commands.Add(setX);
            commands.Add(setY);
            ICommand command = new CompositeCommand(commands, 0);
            IExpression yexpr = new LocalVariableExpression(1);

            WhileCommand whilecmd = new WhileCommand(yexpr, command);

            Context context = new Context(2);

            context.SetValue(0, 0);
            context.SetValue(1, 5);

            whilecmd.Execute(context);

            Assert.AreEqual(0, context.GetValue(1));
            Assert.AreEqual(5, context.GetValue(0));
        }

        [TestMethod]
        public void ExecuteForEachCommand()
        {
            IExpression addToX = new ArithmeticBinaryExpression(ArithmeticOperator.Add, new LocalVariableExpression(1), new LocalVariableExpression(0));
            ICommand setX = new SetLocalVariableCommand(0, addToX);
            IExpression values = new ConstantExpression(new int [] { 1, 2, 3 } );

            ForEachCommand foreachcmd = new ForEachCommand(1, values, setX);

            Context context = new Context(2);

            context.SetValue(0, 0);

            foreachcmd.Execute(context);

            Assert.AreEqual(6, context.GetValue(0));
        }

        [TestMethod]
        public void ExecuteForCommand()
        {
            ICommand setX = new SetLocalVariableCommand(0, new ConstantExpression(0));
            ICommand setY = new SetLocalVariableCommand(1, new ConstantExpression(0));
            List<ICommand> commands = new List<ICommand>();
            commands.Add(setX);
            commands.Add(setY);
            ICommand initialCommand = new CompositeCommand(commands, 0);

            IExpression condition = new CompareExpression(ComparisonOperator.Less, new LocalVariableExpression(0), new ConstantExpression(6));

            IExpression addXtoY = new ArithmeticBinaryExpression(ArithmeticOperator.Add, new LocalVariableExpression(1), new LocalVariableExpression(0));
            ICommand addToY = new SetLocalVariableCommand(1, addXtoY);

            ICommand endCommand = new SetLocalVariableCommand(0, new ArithmeticBinaryExpression(ArithmeticOperator.Add, new LocalVariableExpression(0), new ConstantExpression(1)));

            ForCommand forcmd = new ForCommand(initialCommand, condition, endCommand, addToY);

            Context context = new Context(2);

            context.SetValue(1, null);

            forcmd.Execute(context);

            Assert.AreEqual(15, context.GetValue(1));
        }

        [TestMethod]
        public void ExecuteSetLocalVariableCommandWithVariable()
        {
            Context context = new Context(1);
            SetLocalVariableCommand command = new SetLocalVariableCommand(0, new ConstantExpression("bar"));

            command.Execute(context);

            Assert.AreEqual("bar", context.GetValue(0));
        }
    }
}
