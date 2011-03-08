namespace AjScript.Compiler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using AjScript.Commands;
    using AjScript.Expressions;
    using AjScript.Language;

    public class Parser : IDisposable
    {
        private Lexer lexer;
        private IList<string> variables;

        public Parser(string text)
            : this(new Lexer(text))
        {
        }

        public Parser(TextReader reader)
            : this(new Lexer(reader))
        {
        }

        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
        }

        public ICommand ParseCommand()
        {
            Token token = this.lexer.NextToken();

            if (token == null)
                return null;

            if (token.TokenType == TokenType.Name)
            {
                if (token.Value == "if")
                    return this.ParseIfCommand();

                if (token.Value == "while")
                    return this.ParseWhileCommand();

                if (token.Value == "for")
                    return this.ParseForCommand();

                if (token.Value == "return")
                    return this.ParseReturnCommand();

                if (token.Value == "var")
                    return this.ParseVarCommand();
            }

            if (token.TokenType == TokenType.Separator && token.Value == "{")
                return this.ParseCompositeCommand();

            this.lexer.PushToken(token);

            ICommand command = this.ParseSimpleCommand();

            if (command == null)
                throw new UnexpectedTokenException(token);

            this.Parse(TokenType.Separator, ";");

            return command;
        }

        public IExpression ParseExpression()
        {
            return this.ParseBinaryLogicalExpressionLevelOne();
        }

        public void DefineVariable(string name)
        {
            if (this.variables == null)
                this.variables = new List<string>();

            this.variables.Add(name);
        }

        public int GetVariableOffset(string name)
        {
            if (this.variables == null)
                return -1;

            return this.variables.IndexOf(name);
        }

        public void Dispose()
        {
            if (this.lexer != null)
                this.lexer.Dispose();
        }

        private ICommand ParseSimpleCommand()
        {
            if (this.TryParse(TokenType.Name, "var"))
            {
                this.lexer.NextToken();
                return this.ParseVarCommand();
            }

            IExpression expression = this.ParseExpression();

            if (expression == null)
                return null;

            if (this.TryParse(TokenType.Operator, "="))
            {
                this.lexer.NextToken();

                ICommand command = null;

                if (expression is ArrayExpression)
                {
                    ArrayExpression aexpr = (ArrayExpression)expression;
                    command = new SetArrayCommand(aexpr.Expression, aexpr.Arguments, this.ParseExpression());
                }
                else
                    command = new SetCommand(expression, this.ParseExpression());

                return command;
            }

            if (this.TryParse(TokenType.Operator, "<-"))
            {
                this.lexer.NextToken();
                return new SetValueCommand(expression, this.ParseExpression());
            }

            return new ExpressionCommand(expression);
        }

        private static bool IsName(Token token, string value)
        {
            return IsToken(token, value, TokenType.Name);
        }

        private static bool IsToken(Token token, string value, TokenType type)
        {
            if (token == null)
                return false;

            if (token.TokenType != type)
                return false;

            return token.Value.Equals(value);
        }

        private ICollection<IExpression> ParseArrayValues()
        {
            this.Parse(TokenType.Separator, "{");

            List<IExpression> expressions = new List<IExpression>();

            while (!this.TryParse(TokenType.Separator, "}"))
            {
                if (expressions.Count > 0)
                    this.Parse(TokenType.Separator, ",");

                expressions.Add(this.ParseExpression());
            }

            this.Parse(TokenType.Separator, "}");

            return expressions;
        }

        private IExpression ParseBinaryLogicalExpressionLevelOne()
        {
            IExpression expression = this.ParseBinaryLogicalExpressionLevelTwo();

            if (expression == null)
                return null;

            while (this.TryParse(TokenType.Operator, "||"))
            {
                Token oper = this.lexer.NextToken();
                IExpression right = this.ParseBinaryLogicalExpressionLevelTwo();

                expression = new OrExpression(expression, right);
            }

            return expression;
        }

        private IExpression ParseBinaryLogicalExpressionLevelTwo()
        {
            IExpression expression = this.ParseBinaryExpressionZerothLevel();

            if (expression == null)
                return null;

            while (this.TryParse(TokenType.Operator, "&&"))
            {
                Token oper = this.lexer.NextToken();
                IExpression right = this.ParseBinaryExpressionZerothLevel();

                expression = new AndExpression(expression, right);
            }

            return expression;
        }

        private IExpression ParseBinaryExpressionZerothLevel()
        {
            IExpression expression = this.ParseBinaryExpressionFirstLevel();

            if (expression == null)
                return null;

            while (this.TryParse(TokenType.Operator, "<", ">", "==", ">=", "<=", "!="))
            {
                Token oper = this.lexer.NextToken();
                IExpression right = this.ParseBinaryExpressionFirstLevel();

                ComparisonOperator op = 0;

                if (oper.Value == "<")
                    op = ComparisonOperator.Less;
                if (oper.Value == ">")
                    op = ComparisonOperator.Greater;
                if (oper.Value == "<=")
                    op = ComparisonOperator.LessEqual;
                if (oper.Value == ">=")
                    op = ComparisonOperator.GreaterEqual;
                if (oper.Value == "==")
                    op = ComparisonOperator.Equal;
                if (oper.Value == "!=")
                    op = ComparisonOperator.NotEqual;

                expression = new CompareExpression(op, expression, right);
            }

            return expression;
        }

        private IExpression ParseBinaryExpressionFirstLevel()
        {
            IExpression expression = this.ParseBinaryExpressionSecondLevel();

            if (expression == null)
                return null;

            while (this.TryParse(TokenType.Operator, "+", "-"))
            {
                Token oper = this.lexer.NextToken();
                IExpression right = this.ParseBinaryExpressionSecondLevel();
                ArithmeticOperator op = oper.Value == "+" ? ArithmeticOperator.Add : ArithmeticOperator.Subtract;

                expression = new ArithmeticBinaryExpression(op, expression, right);
            }

            return expression;
        }

        private IExpression ParseBinaryExpressionSecondLevel()
        {
            IExpression expression = this.ParseUnaryExpression();

            if (expression == null)
                return null;

            while (this.TryParse(TokenType.Operator, "*", "/", @"\", "%"))
            {
                Token oper = this.lexer.NextToken();
                IExpression right = this.ParseUnaryExpression();
                ArithmeticOperator op;

                if (oper.Value == "*")
                    op = ArithmeticOperator.Multiply;
                else if (oper.Value == "/")
                    op = ArithmeticOperator.Divide;
                else if (oper.Value == "\\")
                    op = ArithmeticOperator.IntegerDivide;
                else if (oper.Value == "%")
                    op = ArithmeticOperator.Modulo;
                else
                    throw new ParserException(string.Format("Invalid operator '{0}'", oper.Value));

                expression = new ArithmeticBinaryExpression(op, expression, right);
            }

            return expression;
        }

        private IExpression ParseUnaryExpression()
        {
            if (this.TryParse(TokenType.Operator, "+", "-", "!"))
            {
                Token oper = this.lexer.NextToken();

                IExpression unaryExpression = this.ParseUnaryExpression();

                if (oper.Value == "!")
                    return new NotExpression(unaryExpression);

                ArithmeticOperator op = oper.Value == "+" ? ArithmeticOperator.Plus : ArithmeticOperator.Minus;

                return new ArithmeticUnaryExpression(op, unaryExpression);
            }

            if (this.TryParse(TokenType.Operator, "++", "--"))
            {
                Token oper = this.lexer.NextToken();

                IExpression expression = this.ParseTermExpression();

                IncrementOperator op = oper.Value == "++" ? IncrementOperator.PreIncrement : IncrementOperator.PreDecrement;

                return new IncrementExpression(expression, op);
            }

            if (this.TryParse(TokenType.Operator, "<-", "@"))
            {
                this.lexer.NextToken();

                IExpression expression = this.ParseTermExpression();

                return new GetValueExpression(expression);
            }

            IExpression termexpr = this.ParseTermExpression();

            if (this.TryParse(TokenType.Operator, "++", "--"))
            {
                Token oper = this.lexer.NextToken();

                IncrementOperator op = oper.Value == "++" ? IncrementOperator.PostIncrement : IncrementOperator.PostDecrement;

                return new IncrementExpression(termexpr, op);
            }

            return termexpr;
        }

        private IExpression ParseTermExpression()
        {
            IExpression expression = this.ParseSimpleTermExpression();

            while (this.TryParse(TokenType.Operator, ".") || this.TryParse(TokenType.Separator, "[", "("))
            {
                if (this.TryParse(TokenType.Operator, "."))
                {
                    this.lexer.NextToken();
                    string name = this.ParseName();
                    List<IExpression> arguments = null;

                    if (this.TryParse(TokenType.Separator, "("))
                        arguments = this.ParseArgumentList();

                    expression = new DotExpression(expression, name, arguments);
                }
                else
                    expression = new ArrayExpression(expression, this.ParseArrayArgumentList());
            }

            return expression;
        }

        private List<IExpression> ParseArgumentList()
        {
            List<IExpression> expressions = new List<IExpression>();

            this.Parse(TokenType.Separator, "(");

            while (!this.TryParse(TokenType.Separator, ")"))
            {
                if (expressions.Count > 0)
                    this.Parse(TokenType.Separator, ",");

                expressions.Add(this.ParseExpression());
            }

            this.Parse(TokenType.Separator, ")");

            return expressions;
        }

        private List<IExpression> ParseArrayArgumentList()
        {
            List<IExpression> expressions = new List<IExpression>();

            this.Parse(TokenType.Separator, "[");

            while (!this.TryParse(TokenType.Separator, "]"))
            {
                if (expressions.Count > 0)
                    this.Parse(TokenType.Separator, ",");

                expressions.Add(this.ParseExpression());
            }

            this.Parse(TokenType.Separator, "]");

            return expressions;
        }

        private IExpression ParseSimpleTermExpression()
        {
            Token token = this.lexer.NextToken();

            if (token == null)
                return null;

            switch (token.TokenType)
            {
                case TokenType.Separator:
                    if (token.Value == "(")
                    {
                        IExpression expression = this.ParseExpression();
                        this.Parse(TokenType.Separator, ")");
                        return expression;
                    }

                    break;
                case TokenType.Boolean:
                    bool booleanValue = Convert.ToBoolean(token.Value);
                    return new ConstantExpression(booleanValue);
                case TokenType.Integer:
                    int intValue = Int32.Parse(token.Value, System.Globalization.CultureInfo.InvariantCulture);
                    return new ConstantExpression(intValue);
                case TokenType.Real:
                    double realValue = Double.Parse(token.Value, System.Globalization.CultureInfo.InvariantCulture);
                    return new ConstantExpression(realValue);
                case TokenType.String:
                    return new ConstantExpression(token.Value);
                case TokenType.Name:
                    IExpression expr = null;

                    int nvariable = this.GetVariableOffset(token.Value);

                    if (nvariable < 0)
                        expr = new NamedVariableExpression(token.Value);
                    else
                        expr = new LocalVariableExpression(nvariable);

                    if (this.TryParse(TokenType.Separator, "("))
                    {
                        List<IExpression> arguments = this.ParseArgumentList();
                        expr = new InvokeExpression(expr, arguments);
                    }

                    return expr;
            }

            throw new UnexpectedTokenException(token);
        }

        private ICommand ParseCompositeCommand()
        {
            IList<ICommand> commands = new List<ICommand>();

            while (!this.TryParse(TokenType.Separator, "}"))
                commands.Add(this.ParseCommand());

            this.lexer.NextToken();

            // TODO Review: set no. of variables
            return new CompositeCommand(commands, 0);
        }

        private ICommand ParseReturnCommand()
        {
            if (this.TryParse(TokenType.Separator, ";"))
            {
                this.lexer.NextToken();
                return new ReturnCommand();
            }

            IExpression expression = this.ParseExpression();

            this.Parse(TokenType.Separator, ";");

            return new ReturnCommand(expression);
        }

        private ICommand ParseIfCommand()
        {
            this.Parse(TokenType.Separator, "(");
            IExpression condition = this.ParseExpression();
            this.Parse(TokenType.Separator, ")");
            ICommand thencmd = this.ParseCommand();

            if (!this.TryParse(TokenType.Name, "else"))
                return new IfCommand(condition, thencmd);

            this.lexer.NextToken();

            ICommand elsecmd = this.ParseCommand();

            return new IfCommand(condition, thencmd, elsecmd);
        }

        private ICommand ParseWhileCommand()
        {
            this.Parse(TokenType.Separator, "(");
            IExpression condition = this.ParseExpression();
            this.Parse(TokenType.Separator, ")");
            ICommand command = this.ParseCommand();

            return new WhileCommand(condition, command);
        }

        private ICommand ParseForInCommand()
        {
            string name = this.ParseName();

            if (name == "var")
            {
                name = this.ParseName();
                this.DefineVariable(name);
            }

            int nvariable = this.GetVariableOffset(name);

            if (nvariable < 0)
                throw new ParserException(string.Format("Undefined Variable {0}", name));

            this.Parse(TokenType.Name, "in");
            IExpression values = this.ParseExpression();
            this.Parse(TokenType.Separator, ")");
            ICommand command = this.ParseCommand();

            return new ForEachCommand(nvariable, values, command);
        }

        private ICommand ParseForCommand()
        {
            this.Parse(TokenType.Separator, "(");

            Token token = this.lexer.NextToken();

            if (token.TokenType == TokenType.Name && token.Value == "var")
            {
                string name = this.ParseName();

                if (this.TryParse(TokenType.Name, "in"))
                {
                    this.lexer.PushToken(new Token() { TokenType = TokenType.Name, Value = name });
                    this.lexer.PushToken(token);

                    return this.ParseForInCommand();
                }

                this.lexer.PushToken(new Token() { TokenType = TokenType.Name, Value = name });
                this.lexer.PushToken(token);
            }

            ICommand initial = this.ParseSimpleCommand();
            //this.Parse(TokenType.Separator, ";");
            IExpression condition = this.ParseExpression();
            this.Parse(TokenType.Separator, ";");
            ICommand endcmd = this.ParseSimpleCommand();
            this.Parse(TokenType.Separator, ")");
            ICommand command = this.ParseCommand();

            return new ForCommand(initial, condition, endcmd, command);
        }

        private ICommand ParseVarCommand()
        {
            string name = this.ParseName();
            this.DefineVariable(name);

            IExpression expression = null;

            if (this.TryParse(TokenType.Operator, "="))
            {
                this.lexer.NextToken();
                expression = this.ParseExpression();
            }
            else
            {
                expression = new ConstantExpression(Undefined.Instance);
            }

            this.Parse(TokenType.Separator, ";");

            return new SetLocalVariableCommand(this.GetVariableOffset(name), expression);
        }

        private void ParseMemberVariable(IList<string> memberNames, IList<IExpression> memberExpressions)
        {
            string name = this.ParseName();
            IExpression expression = null;

            if (this.TryParse(TokenType.Operator, "="))
            {
                this.lexer.NextToken();

                expression = this.ParseExpression();
            }

            this.Parse(TokenType.Separator, ";");

            memberNames.Add(name);
            memberExpressions.Add(expression);
        }

        private void ParseMemberMethod(IList<string> memberNames, IList<IExpression> memberExpressions, bool isdefault)
        {
            string name = this.ParseName();
            bool hasvariableparameters = false;
            string[] parameterNames = this.ParseParameters(ref hasvariableparameters);
            ICommand body = this.ParseCommand();

            memberNames.Add(name);

            throw new NotImplementedException();
            //memberExpressions.Add(new FunctionExpression(parameterNames, body, isdefault, hasvariableparameters));
        }

        private string[] ParseParameters(ref bool hasvariableparameters)
        {
            List<string> names = new List<string>();

            this.Parse(TokenType.Separator, "(");

            while (!this.TryParse(TokenType.Separator, ")"))
            {
                if (names.Count > 0)
                    this.Parse(TokenType.Separator, ",");

                names.Add(this.ParseName());
            }

            this.lexer.NextToken();

            return names.ToArray();
        }

        private bool TryPeekName()
        {
            Token token = this.lexer.PeekToken();

            if (token == null)
                return false;

            return token.TokenType == TokenType.Name;
        }

        private object ParseValue()
        {
            Token token = this.lexer.NextToken();

            if (token == null)
                throw new UnexpectedEndOfInputException();

            if (token.TokenType == TokenType.String)
                return token.Value;

            if (token.TokenType == TokenType.Integer)
                return int.Parse(token.Value);

            throw new UnexpectedTokenException(token);
        }

        private bool TryParse(TokenType type, params string[] values)
        {
            Token token = this.lexer.PeekToken();

            if (token == null)
                return false;

            if (token.TokenType == type)
                foreach (string value in values)
                    if (token.Value == value)
                        return true;

            return false;
        }

        private void Parse(TokenType type, string value)
        {
            Token token = this.lexer.NextToken();

            if (token == null)
                throw new UnexpectedEndOfInputException();

            if (type == TokenType.Name)
                if (IsName(token, value))
                    return;
                else
                    throw new UnexpectedTokenException(token);

            if (token.TokenType != type || token.Value != value)
                throw new UnexpectedTokenException(token);
        }

        private string ParseName()
        {
            Token token = this.lexer.NextToken();

            if (token == null)
                throw new ParserException(string.Format("Unexpected end of input"));

            if (token.TokenType == TokenType.Name)
                return token.Value;

            throw new UnexpectedTokenException(token);
        }

        private string ParseQualifiedName()
        {
            string name = this.ParseName();

            while (this.TryParse(TokenType.Operator, "."))
            {
                this.lexer.NextToken();
                name += "." + this.ParseName();
            }

            return name;
        }
    }
}
