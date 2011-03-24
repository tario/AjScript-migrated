namespace AjScript.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using AjScript.Commands;

    public class Function : ICallable
    {
        private string[] parameterNames;
        private ICommand body;
        private int arity;
        private IContext context;

        public Function(string[] parameterNames, ICommand body)
            : this(parameterNames, body, null)
        {
        }

        public Function(string[] parameterNames, ICommand body, IContext context)
        {
            this.parameterNames = parameterNames;
            this.body = body;

            if (parameterNames == null)
                this.arity = 0;
            else
                this.arity = parameterNames.Length;

            this.context = context;
        }

        public int Arity { get { return this.parameterNames == null ? 0 : this.parameterNames.Length; } }

        public string[] ParameterNames { get { return this.parameterNames; } }

        public ICommand Body { get { return this.body; } }

        public IContext Context { get { return this.context; } }

        public object Invoke(IContext context, object @this, object[] arguments)
        {
            IContext newctx = new Context(context);

            // Set this and arguments
            newctx.DefineVariable("this");
            newctx.SetValue("this", @this);
            newctx.DefineVariable("arguments");
            newctx.SetValue("arguments", arguments);

            for (int k = 0; arguments != null && k < arguments.Length && k < this.Arity; k++)
            {
                newctx.DefineVariable(parameterNames[k]);
                newctx.SetValue(parameterNames[k], arguments[k]);
            }

            this.Body.Execute(newctx);

            // TODO Review: return undefined it not this?
            if (newctx.ReturnValue == null)
                if (@this != null)
                    return @this;
                else
                    return Undefined.Instance;

            return newctx.ReturnValue.Value;
        }
    }
}
