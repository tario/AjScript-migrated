﻿namespace AjScript.Tests.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using AjScript.Commands;
    using AjScript.Compiler;
    using AjScript.Expressions;
    using AjScript.Language;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ParserTests
    {
        private Parser parser;

        [TestMethod]
        public void GetDefinedVariablesOffsets()
        {
            this.CreateParser("");

            Assert.AreEqual(0, this.parser.GetVariableOffset("a"));
            Assert.AreEqual(1, this.parser.GetVariableOffset("b"));
            Assert.AreEqual(2, this.parser.GetVariableOffset("c"));
            Assert.AreEqual(-1, this.parser.GetVariableOffset("x"));
        }

        [TestMethod]
        public void DefineVariable()
        {
            this.CreateParser("");
            this.parser.DefineVariable("x");

            Assert.AreEqual(0, this.parser.GetVariableOffset("a"));
            Assert.AreEqual(1, this.parser.GetVariableOffset("b"));
            Assert.AreEqual(2, this.parser.GetVariableOffset("c"));
            Assert.AreEqual(3, this.parser.GetVariableOffset("x"));
        }

        [TestMethod]
        public void ParseConstantExpressions()
        {
            IExpression expression;

            expression = ParseExpression("1");
            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ConstantExpression));
            Assert.AreEqual(1, expression.Evaluate(null));

            expression = ParseExpression("1.2");
            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ConstantExpression));
            Assert.AreEqual(1.2, expression.Evaluate(null));

            expression = ParseExpression("false");
            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ConstantExpression));
            Assert.IsFalse((bool)expression.Evaluate(null));

            expression = ParseExpression("\"foo\"");
            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ConstantExpression));
            Assert.AreEqual("foo", expression.Evaluate(null));

            Assert.IsNull(ParseExpression(""));
        }

        [TestMethod]
        public void ParseSimpleUnaryExpression()
        {
            IExpression expression = ParseExpression("-2");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ArithmeticUnaryExpression));

            ArithmeticUnaryExpression operation = (ArithmeticUnaryExpression)expression;

            Assert.AreEqual(ArithmeticOperator.Minus, operation.Operation);
            Assert.IsNotNull(operation.Expression);
            Assert.IsInstanceOfType(operation.Expression, typeof(ConstantExpression));
        }

        [TestMethod]
        public void ParseFirstDefinedVariable()
        {
            IExpression expression = ParseExpression("a");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(LocalVariableExpression));

            LocalVariableExpression varexpr = (LocalVariableExpression)expression;
            Assert.AreEqual(0, varexpr.NVariable);
        }

        [TestMethod]
        public void ParseSecondDefinedVariable()
        {
            IExpression expression = ParseExpression("b");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(LocalVariableExpression));

            LocalVariableExpression varexpr = (LocalVariableExpression)expression;
            Assert.AreEqual(1, varexpr.NVariable);
        }

        [TestMethod]
        public void ParseNewVariableDefinition()
        {
            ICommand command = ParseCommand("var x;");

            Assert.AreEqual(3, this.parser.GetVariableOffset("x"));
            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(SetLocalVariableCommand));

            SetLocalVariableCommand setcmd = (SetLocalVariableCommand)command;
            Assert.AreEqual(3, setcmd.NVariable);
            Assert.IsInstanceOfType(setcmd.Expression, typeof(ConstantExpression));

            ConstantExpression consexpr = (ConstantExpression)setcmd.Expression;

            Assert.AreEqual(Undefined.Instance, consexpr.Value);
        }

        [TestMethod]
        public void ParseNewVariableDefinitionAndInitialization()
        {
            ICommand command = ParseCommand("var x = 1;");

            Assert.AreEqual(3, this.parser.GetVariableOffset("x"));
            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(SetLocalVariableCommand));

            SetLocalVariableCommand setcmd = (SetLocalVariableCommand)command;
            Assert.AreEqual(3, setcmd.NVariable);
            Assert.IsInstanceOfType(setcmd.Expression, typeof(ConstantExpression));

            ConstantExpression consexpr = (ConstantExpression)setcmd.Expression;

            Assert.AreEqual(1, consexpr.Value);
        }

        [TestMethod]
        public void ParseSimpleBinaryExpression()
        {
            IExpression expression = ParseExpression("a + 2");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ArithmeticBinaryExpression));

            ArithmeticBinaryExpression operation = (ArithmeticBinaryExpression)expression;

            Assert.AreEqual(ArithmeticOperator.Add, operation.Operation);
            Assert.IsNotNull(operation.LeftExpression);
            Assert.IsInstanceOfType(operation.LeftExpression, typeof(LocalVariableExpression));
            Assert.IsNotNull(operation.RightExpression);
            Assert.IsInstanceOfType(operation.RightExpression, typeof(ConstantExpression));
        }

        [TestMethod]
        public void ParseModExpression()
        {
            IExpression expression = ParseExpression("a % 2");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ArithmeticBinaryExpression));

            ArithmeticBinaryExpression operation = (ArithmeticBinaryExpression)expression;

            Assert.AreEqual(ArithmeticOperator.Modulo, operation.Operation);
            Assert.IsNotNull(operation.LeftExpression);
            Assert.IsInstanceOfType(operation.LeftExpression, typeof(LocalVariableExpression));
            Assert.IsNotNull(operation.RightExpression);
            Assert.IsInstanceOfType(operation.RightExpression, typeof(ConstantExpression));
        }

        [TestMethod]
        public void ParseSimpleCompareExpression()
        {
            IExpression expression = ParseExpression("b <= 1");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(CompareExpression));

            CompareExpression operation = (CompareExpression)expression;

            Assert.AreEqual(ComparisonOperator.LessEqual, operation.Operation);
            Assert.IsNotNull(operation.LeftExpression);
            Assert.IsInstanceOfType(operation.LeftExpression, typeof(LocalVariableExpression));
            Assert.IsNotNull(operation.RightExpression);
            Assert.IsInstanceOfType(operation.RightExpression, typeof(ConstantExpression));
        }

        [TestMethod]
        public void ParseSimpleBinaryExpressionWithParenthesis()
        {
            IExpression expression = ParseExpression("((a) + (2))");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ArithmeticBinaryExpression));

            ArithmeticBinaryExpression operation = (ArithmeticBinaryExpression)expression;

            Assert.AreEqual(ArithmeticOperator.Add, operation.Operation);
            Assert.IsNotNull(operation.LeftExpression);
            Assert.IsInstanceOfType(operation.LeftExpression, typeof(LocalVariableExpression));
            LocalVariableExpression varexpr = (LocalVariableExpression)operation.LeftExpression;
            Assert.AreEqual(0, varexpr.NVariable);
            Assert.IsNotNull(operation.RightExpression);
            Assert.IsInstanceOfType(operation.RightExpression, typeof(ConstantExpression));
        }

        [TestMethod]
        public void ParseTwoBinaryExpression()
        {
            IExpression expression = ParseExpression("a + 2 - 3");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ArithmeticBinaryExpression));

            ArithmeticBinaryExpression operation = (ArithmeticBinaryExpression)expression;

            Assert.AreEqual(ArithmeticOperator.Subtract, operation.Operation);
            Assert.IsNotNull(operation.LeftExpression);
            Assert.IsInstanceOfType(operation.LeftExpression, typeof(ArithmeticBinaryExpression));
            Assert.IsNotNull(operation.RightExpression);
            Assert.IsInstanceOfType(operation.RightExpression, typeof(ConstantExpression));
        }

        [TestMethod]
        public void ParseTwoBinaryExpressionDifferentLevels()
        {
            IExpression expression = ParseExpression("a + 2 * 3");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(ArithmeticBinaryExpression));

            ArithmeticBinaryExpression arithmeticExpression = (ArithmeticBinaryExpression)expression;

            Assert.AreEqual(ArithmeticOperator.Add, arithmeticExpression.Operation);
            Assert.IsNotNull(arithmeticExpression.LeftExpression);
            Assert.IsInstanceOfType(arithmeticExpression.LeftExpression, typeof(LocalVariableExpression));
            Assert.IsNotNull(arithmeticExpression.RightExpression);
            Assert.IsInstanceOfType(arithmeticExpression.RightExpression, typeof(ArithmeticBinaryExpression));

            ArithmeticBinaryExpression rigthExpression = (ArithmeticBinaryExpression) arithmeticExpression.RightExpression;

            Assert.AreEqual(ArithmeticOperator.Multiply, rigthExpression.Operation);
            Assert.IsInstanceOfType(rigthExpression.LeftExpression, typeof(ConstantExpression));
            Assert.IsInstanceOfType(rigthExpression.RightExpression, typeof(ConstantExpression));
        }

        [TestMethod]
        public void ParseSetVariableCommand()
        {
            ICommand command = ParseCommand("a = 1;");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(SetCommand));

            SetCommand setcmd = (SetCommand)command;

            Assert.IsInstanceOfType(setcmd.LeftValue, typeof(LocalVariableExpression));
            Assert.AreEqual(0, ((LocalVariableExpression)setcmd.LeftValue).NVariable);
            Assert.IsNotNull(setcmd.Expression);
            Assert.IsInstanceOfType(setcmd.Expression, typeof(ConstantExpression));
            Assert.AreEqual(1, setcmd.Expression.Evaluate(null));
        }

        [TestMethod]
        public void ParseReturnCommand()
        {
            ICommand command = ParseCommand("return;");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(ReturnCommand));

            ReturnCommand retcmd = (ReturnCommand)command;

            Assert.IsNull(retcmd.Expression);
        }

        [TestMethod]
        public void ParseReturnCommandWithExpression()
        {
            ICommand command = ParseCommand("return 1;");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(ReturnCommand));

            ReturnCommand retcmd = (ReturnCommand)command;

            Assert.IsNotNull(retcmd.Expression);
            Assert.IsInstanceOfType(retcmd.Expression, typeof(ConstantExpression));
        }

        [TestMethod]
        public void ParseIfCommand()
        {
            ICommand command = ParseCommand("if (c<=1) return 1;");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(IfCommand));

            IfCommand ifcmd = (IfCommand)command;

            Assert.IsNotNull(ifcmd.Condition);
            Assert.IsNotNull(ifcmd.ThenCommand);
            Assert.IsNull(ifcmd.ElseCommand);
        }

        [TestMethod]
        public void ParseIfCommandWithElse()
        {
            ICommand command = ParseCommand("if (a<=1) return 1; else return a * (b-1);");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(IfCommand));

            IfCommand ifcmd = (IfCommand)command;

            Assert.IsNotNull(ifcmd.Condition);
            Assert.IsNotNull(ifcmd.ThenCommand);
            Assert.IsNotNull(ifcmd.ElseCommand);
        }

        [TestMethod]
        public void ParseSimpleWhile()
        {
            ICommand command = ParseCommand("while (a<10) a=a+1;");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(WhileCommand));

            WhileCommand whilecmd = (WhileCommand)command;

            Assert.IsNotNull(whilecmd.Condition);
            Assert.IsNotNull(whilecmd.Command);
            Assert.IsInstanceOfType(whilecmd.Command, typeof(SetCommand));
        }

        [TestMethod]
        public void ParseSimpleForIn()
        {
            ICommand command = ParseCommand("for (var k in b) a=a+k;");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(ForEachCommand));

            ForEachCommand foreachcmd = (ForEachCommand) command;

            Assert.IsNotNull(foreachcmd.Expression);
            Assert.IsInstanceOfType(foreachcmd.Expression, typeof(LocalVariableExpression));
            Assert.IsNotNull(foreachcmd.Command);
            Assert.IsInstanceOfType(foreachcmd.Command, typeof(SetCommand));
        }

        [TestMethod]
        public void ParseSimpleForInWithLocalVar()
        {
            ICommand command = ParseCommand("for (var x in b) c=c+x;");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(ForEachCommand));

            ForEachCommand foreachcmd = (ForEachCommand)command;

            Assert.IsNotNull(foreachcmd.Expression);
            Assert.IsInstanceOfType(foreachcmd.Expression, typeof(LocalVariableExpression));
            Assert.IsNotNull(foreachcmd.Command);
            Assert.IsInstanceOfType(foreachcmd.Command, typeof(SetCommand));
        }

        [TestMethod]
        public void ParseSimpleIncrement()
        {
            ICommand command = ParseCommand("b++;");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(ExpressionCommand));

            ExpressionCommand expcmd = (ExpressionCommand)command;

            Assert.IsNotNull(expcmd.Expression);
            Assert.IsInstanceOfType(expcmd.Expression, typeof(IncrementExpression));
        }

        [TestMethod]
        public void ParseSimpleFor()
        {
            ICommand command = ParseCommand("for (var k=1; k<=5; k++) a=a+k;");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(ForCommand));

            ForCommand forcommand = (ForCommand)command;

            Assert.IsNotNull(forcommand.InitialCommand);
            Assert.IsNotNull(forcommand.Condition);
            Assert.IsNotNull(forcommand.EndCommand);
            Assert.IsNotNull(forcommand.Body);
        }

        [TestMethod]
        public void ParseCompositeCommand()
        {
            ICommand command = ParseCommand("{ a=1; b=2; }");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(CompositeCommand));

            CompositeCommand compcmd = (CompositeCommand)command;

            Assert.AreEqual(2, compcmd.CommandCount);
            Assert.IsNotNull(compcmd.Commands);
            Assert.AreEqual(2, compcmd.Commands.Count);

            foreach (ICommand cmd in compcmd.Commands)
            {
                Assert.IsNotNull(cmd);
                Assert.IsInstanceOfType(cmd, typeof(SetCommand));
            }
        }

        [TestMethod]
        public void ParseWriteCommand()
        {
            ICommand command = ParseCommand("write(1);");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(ExpressionCommand));
        }

        [TestMethod]
        public void ParseSimpleDotExpression()
        {
            IExpression expression = ParseExpression("a.length");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(DotExpression));            
        }

        [TestMethod]
        public void ParseSimpleDotExpressionWithArguments()
        {
            IExpression expression = ParseExpression("a.c(1,2)");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(DotExpression));
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedTokenException))]
        public void RaiseIfUnexpectedTokenDot()
        {
            ParseExpression(".");
        }

        [TestMethod]
        public void ParseSetPropertyCommand()
        {
            ICommand command = ParseCommand("a.FirstName = \"Adam\";");

            Assert.IsNotNull(command);            
        }

        [TestMethod]
        public void ParsePreIncrementExpressionWithVariable()
        {
            IExpression expression = ParseExpression("++b");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(IncrementExpression));

            IncrementExpression incexpr = (IncrementExpression)expression;

            Assert.AreEqual(IncrementOperator.PreIncrement, incexpr.Operator);
            Assert.IsNotNull(incexpr.Expression);
            Assert.IsInstanceOfType(incexpr.Expression, typeof(LocalVariableExpression));
        }

        [TestMethod]
        public void ParsePreDecrementExpressionWithDotName()
        {
            IExpression expression = ParseExpression("--a.Age");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(IncrementExpression));

            IncrementExpression incexpr = (IncrementExpression)expression;

            Assert.AreEqual(IncrementOperator.PreDecrement, incexpr.Operator);
            Assert.IsNotNull(incexpr.Expression);
            Assert.IsInstanceOfType(incexpr.Expression, typeof(DotExpression));
        }

        [TestMethod]
        public void ParsePostIncrementExpressionWithVariable()
        {
            IExpression expression = ParseExpression("b++");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(IncrementExpression));

            IncrementExpression incexpr = (IncrementExpression)expression;

            Assert.AreEqual(IncrementOperator.PostIncrement, incexpr.Operator);
            Assert.IsNotNull(incexpr.Expression);
            Assert.IsInstanceOfType(incexpr.Expression, typeof(LocalVariableExpression));
        }

        [TestMethod]
        public void ParsePostDecrementExpressionWithDotName()
        {
            IExpression expression = ParseExpression("a.Age--");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(IncrementExpression));

            IncrementExpression incexpr = (IncrementExpression)expression;

            Assert.AreEqual(IncrementOperator.PostDecrement, incexpr.Operator);
            Assert.IsNotNull(incexpr.Expression);
            Assert.IsInstanceOfType(incexpr.Expression, typeof(DotExpression));
        }

        [TestMethod]
        public void ParseSetArrayCommand()
        {
            ICommand command = ParseCommand("b[0] = 1;");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(SetArrayCommand));

            SetArrayCommand setcmd = (SetArrayCommand) command;

            Assert.IsInstanceOfType(setcmd.LeftValue, typeof(LocalVariableExpression));
            Assert.AreEqual(1, setcmd.Arguments.Count);
            Assert.IsInstanceOfType(setcmd.Expression, typeof(ConstantExpression));
        }

        [TestMethod]
        public void ParseSetArrayCommandWithDotExpression()
        {
            ICommand command = ParseCommand("b.Values[0] = 1;");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(SetArrayCommand));

            SetArrayCommand setcmd = (SetArrayCommand)command;

            Assert.IsInstanceOfType(setcmd.LeftValue, typeof(DotExpression));
            Assert.AreEqual(1, setcmd.Arguments.Count);
            Assert.IsInstanceOfType(setcmd.Expression, typeof(ConstantExpression));
        }

        [TestMethod]
        public void ParseNotExpression()
        {
            IExpression expression = ParseExpression("!a");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(NotExpression));

            NotExpression notexpr = (NotExpression)expression;

            Assert.IsInstanceOfType(notexpr.Expression, typeof(LocalVariableExpression));
        }

        [TestMethod]
        public void ParseAndExpression()
        {
            IExpression expression = ParseExpression("a==1 && b==1");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(AndExpression));

            AndExpression andexpr = (AndExpression)expression;

            Assert.IsInstanceOfType(andexpr.LeftExpression, typeof(CompareExpression));
            Assert.IsInstanceOfType(andexpr.RightExpression, typeof(CompareExpression));
        }

        [TestMethod]
        public void ParseOrExpression()
        {
            IExpression expression = ParseExpression("a==1 || b==1");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(OrExpression));

            OrExpression orexpr = (OrExpression)expression;

            Assert.IsInstanceOfType(orexpr.LeftExpression, typeof(CompareExpression));
            Assert.IsInstanceOfType(orexpr.RightExpression, typeof(CompareExpression));
        }

        [TestMethod]
        public void ParseOrAndExpression()
        {
            IExpression expression = ParseExpression("a==1 || b==1 && c==1");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(OrExpression));

            OrExpression orexpr = (OrExpression)expression;

            Assert.IsInstanceOfType(orexpr.LeftExpression, typeof(CompareExpression));
            Assert.IsInstanceOfType(orexpr.RightExpression, typeof(AndExpression));
        }

        [TestMethod]
        public void ParseAndOrExpression()
        {
            IExpression expression = ParseExpression("a==1 && b==1 || c==1");

            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(OrExpression));

            OrExpression orexpr = (OrExpression)expression;

            Assert.IsInstanceOfType(orexpr.LeftExpression, typeof(AndExpression));
            Assert.IsInstanceOfType(orexpr.RightExpression, typeof(CompareExpression));
        }

        private IExpression ParseExpression(string text)
        {
            this.CreateParser(text);

            IExpression expression = this.parser.ParseExpression();

            Assert.IsNull(this.parser.ParseExpression());

            return expression;
        }

        private ICommand ParseCommand(string text)
        {
            this.CreateParser(text);

            ICommand command = this.parser.ParseCommand();

            Assert.IsNull(this.parser.ParseCommand());

            return command;
        }

        private void CreateParser(string text)
        {
            this.parser = new Parser(text);
            this.parser.DefineVariable("a");
            this.parser.DefineVariable("b");
            this.parser.DefineVariable("c");
        }
    }
}