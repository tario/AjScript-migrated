namespace AjScript.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AjScript.Compiler;
    using AjScript.Commands;
    using AjScript.Primitives;

    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser(System.Console.In);
            IContext context = new Context(1);
            context.SetValue(0, new WriteFunction());
            parser.DefineVariable("write");

            for (ICommand cmd = parser.ParseCommand(); cmd != null; cmd = parser.ParseCommand())
                cmd.Execute(context);
        }
    }
}
