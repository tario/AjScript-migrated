namespace AjScript.Tests
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using AjScript.Language;

    [TestClass]
    public class ContextTests
    {
        [TestMethod]
        public void SetAndGetValue()
        {
            Context context = new Context(1);
            context.SetValue(0, "One");

            Assert.AreEqual("One", context.GetValue(0));
        }

        [TestMethod]
        public void GetUndefined()
        {
            Context context = new Context(10);

            for (int k = 0; k < 10; k++)
                Assert.AreEqual(Undefined.Instance, context.GetValue(k));
        }

        [TestMethod]
        public void SetAndGetValues()
        {
            Context context = new Context(10);

            for (int k=0; k < 10; k++)
                context.SetValue(k, k);

            for (int k = 0; k < 10; k++)
                Assert.AreEqual(k, context.GetValue(k));
        }

        [TestMethod]
        public void DefineVariable()
        {
            Context context = new Context(10);

            int nvariable = context.DefineVariable("x");

            Assert.AreEqual(10, nvariable);
            Assert.AreEqual(Undefined.Instance, context.GetValue(10));
            Assert.AreEqual(Undefined.Instance, context.GetValue("x"));
        }

        [TestMethod]
        public void DefineSetAndGetVariable()
        {
            Context context = new Context(10);

            int nvariable = context.DefineVariable("x");

            Assert.AreEqual(10, nvariable);

            context.SetValue("x", 123);
            Assert.AreEqual(123, context.GetValue("x"));
            Assert.AreEqual(123, context.GetValue(10));
        }

        [TestMethod]
        public void GetUndefinedIfNameIsUnknown()
        {
            Context context = new Context(0);

            Assert.AreEqual(Undefined.Instance, context.GetValue("x"));
        }
    }
}
