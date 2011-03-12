namespace AjScript.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AjScript.Language;

    public class ObjectFunction : IFunction
    {
        public object NewInstance(object[] parameters)
        {
            return new DynamicObject();
        }
    }
}
