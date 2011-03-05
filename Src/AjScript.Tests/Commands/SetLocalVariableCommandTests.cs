namespace AjScript.Tests.Commands
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using AjScript.Commands;
    using AjScript.Expressions;

    [TestClass]
    public class SetLocalVariableCommandTests
    {
        [TestMethod]
        public void SetLocalVariableToInteger()
        {
            Context context = new Context(3);
            ConstantExpression one = new ConstantExpression(1);
            SetLocalVariableCommand command = new SetLocalVariableCommand(0, one);

            Assert.AreEqual(0, command.NVariable);
            Assert.AreEqual(one, command.Expression);

            command.Execute(context);

            Assert.AreEqual(1, context.GetValue(0));
        }

        [TestMethod]
        public void SetLocalVariablesToIntegers()
        {
            Context context = new Context(3);

            for (int k = 0; k < 3; k++)
            {
                SetLocalVariableCommand command = new SetLocalVariableCommand(k, new ConstantExpression(k+1));
                command.Execute(context);
                Assert.AreEqual(k+1, context.GetValue(k));
            }
        }
    }
}
