namespace AjScript.Tests
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public void SetAndGetValues()
        {
            Context context = new Context(10);

            for (int k=0; k < 10; k++)
                context.SetValue(k, k);

            for (int k = 0; k < 10; k++)
                Assert.AreEqual(k, context.GetValue(k));
        }
    }
}
