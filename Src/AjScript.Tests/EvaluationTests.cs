namespace AjScript.Tests
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using AjScript.Language;
    using AjScript.Compiler;
    using AjScript.Commands;

    [TestClass]
    public class EvaluationTests
    {
        private IContext context;

        [TestInitialize]
        public void Setup()
        {
            this.context = new Context(0);
        }

        [TestMethod]
        public void EvaluateVar()
        {
            Evaluate("var x;");
            Assert.AreEqual(Undefined.Instance, this.context.GetValue("x"));
        }

        [TestMethod]
        public void DefineVar()
        {
            Evaluate("var x=1;");
            Assert.AreEqual(1, this.context.GetValue("x"));
        }

        [TestMethod]
        public void DefineVarWithInitialValue()
        {
            Evaluate("var x=1+2;");
            Assert.AreEqual(3, this.context.GetValue("x"));
        }

        [TestMethod]
        public void DefineVarWithInitialExpressionValue()
        {
            Evaluate("var x=1+2;");
            Assert.AreEqual(3, this.context.GetValue("x"));
        }

        [TestMethod]
        public void SetUndefinedVar()
        {
            Evaluate("x = 1+2;");
            Assert.AreEqual(3, this.context.GetValue("x"));
        }

        [TestMethod]
        public void PreIncrementVar()
        {
            Evaluate("var x = 0; y = ++x;");
            Assert.AreEqual(1, this.context.GetValue("x"));
            Assert.AreEqual(1, this.context.GetValue("y"));
        }

        [TestMethod]
        public void PostIncrementVar()
        {
            Evaluate("var x = 0; y = x++;");
            Assert.AreEqual(1, this.context.GetValue("x"));
            Assert.AreEqual(0, this.context.GetValue("y"));
        }

        [TestMethod]
        public void PreDecrementVar()
        {
            Evaluate("var x = 0; y = --x;");
            Assert.AreEqual(-1, this.context.GetValue("x"));
            Assert.AreEqual(-1, this.context.GetValue("y"));
        }

        [TestMethod]
        public void PostDecrementVar()
        {
            Evaluate("var x = 0; y = x--;");
            Assert.AreEqual(-1, this.context.GetValue("x"));
            Assert.AreEqual(0, this.context.GetValue("y"));
        }

        [TestMethod]
        public void SimpleFor()
        {
            Evaluate("var y = 1; for (var x=1; x<4; x++) y = y*x;");
            Assert.AreEqual(4, this.context.GetValue("x"));
            Assert.AreEqual(6, this.context.GetValue("y"));
        }

        [TestMethod]
        public void SimpleForWithBlock()
        {
            Evaluate("var y = 1; for (var x=1; x<4; x++) { y = y*x; y = y*2; }");
            Assert.AreEqual(4, this.context.GetValue("x"));
            Assert.AreEqual(48, this.context.GetValue("y"));
        }

        private void Evaluate(string text)
        {
            Parser parser = new Parser(text, this.context);

            for (ICommand cmd = parser.ParseCommand(); cmd != null; cmd = parser.ParseCommand())
                cmd.Execute(this.context);
        }
    }
}
