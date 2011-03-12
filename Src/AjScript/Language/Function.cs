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
            int nvariables = this.Arity + 2;

            IContext newctx = new Context(context, nvariables);

            // Set this and arguments
            newctx.SetValue(0, @this);
            newctx.SetValue(1, arguments);

            for (int k = 0; arguments != null && k < arguments.Length && k < this.Arity; k++)
                newctx.SetValue(k + 2, arguments[k]);

            this.Body.Execute(newctx);

            // TODO return Undefined?
            if (newctx.ReturnValue == null)
                return null;

            return newctx.ReturnValue.Value;
        }
    }
}
