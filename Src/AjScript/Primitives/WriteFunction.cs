namespace AjScript.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AjScript.Language;
    using System.IO;

    public class WriteFunction : ICallable
    {
        private TextWriter writer;

        public WriteFunction()
            : this(System.Console.Out)
        {
        }

        public WriteFunction(TextWriter writer)
        {
            this.writer = writer;
        }

        public int Arity
        {
            get { return 1; }
        }

        public IContext Context
        {
            get { throw new NotImplementedException(); }
        }

        public object Invoke(IContext context, object @this, object[] arguments)
        {
            this.writer.Write(arguments[0]);
            // TODO Review return value
            return null;
        }
    }
}
