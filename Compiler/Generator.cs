using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    internal class Generator
    {
        private List<List<Token>> Definations;
        private List<List<Token>> Expressions;
        private List<Token> variables;
        private List<List<Token>> RightPartsOfExpressions;
        private Dictionary<string, string> parentheses=new Dictionary<string, string>()
        {
            { "(", ")" },
        };
        string[] ArrOfOper = new string[]
        { "+", "-", "/", "*" };

        public Generator(AnalyzerOfTokens analyzer)
        {
            variables = new List<Token>();
            RightPartsOfExpressions = new List<List<Token>>();
            Definations = analyzer.Definations;
            Expressions = analyzer.arithmeticExpressions;

            CheckDefinations();
            isParenthesesBalanced();
            CheckRightPart();
            SeparatorOfExpressions();
            Calculate();
        }

        /*private void FindErrorType()
        {
            for(int i = 0; i < Definations.Count; i++)
            {
                List<Token> defination = Definations[i];
                foreach (Token token in defination)
                {
                    if (token.TokenType == Type.Error)
                    {
                        throw new MyException("Использование недопустимых символов",token.line,token.index);
                    }
                }
            }
            for (int i = 0; i < Expressions.Count; i++)
            {
                List<Token> expression = Definations[i];
                foreach (Token token in expression)
                {
                    if (token.TokenType == Type.Error)
                    {
                        throw new MyException("Использование недопустимых символов", token.line, token.index);
                    }
                }
            }
        }
        */
        private void CheckDefinations()
        {
            if(Definations.Count==0)
            {
                throw new MyException("Отсутствует определение в программе", 1,1);
            }
        }

        private void isParenthesesBalanced()
        {
            for (int i = 0; i < Expressions.Count; i++)
            {
                List<Token> expression = Expressions[i];
                Stack<Token> stack = new Stack<Token>();
                bool hasEqualOperator = false;
                foreach (Token token in expression)
                {
                    if (token.name == "=")
                    {
                        hasEqualOperator = true;
                    }
                    if (parentheses.ContainsKey(token.name))
                    {
                        stack.Push(token);
                    }
                    else if (!parentheses.ContainsValue(token.name)) continue;
                    else if(stack.Count == 0)
                    {
                        throw new MyException("Нет открывающей скобки для закрывающей",token.line,token.index);
                    }
                    else
                    {
                        Token newToken=stack.Pop();
                        if(parentheses[newToken.name]!=token.name)
                        {
                            throw new Exception();
                        }
                    }
                }
                if(!hasEqualOperator)
                {
                    throw new MyException("После переменной должен быть знак \"=\"",expression[0].line,expression[0].index);
                }
                if (stack.Count > 0)
                {
                    Token token = stack.Pop();
                    throw new MyException("Нет закрывающей скобки для открывающей", token.line, token.index);
                }
            }
        }
        
        private void CheckRightPart()
        {
            for (int i = 0; i < Expressions.Count; i++)
            {
                List<Token> expression=Expressions[i];
                int j = 2;
                if (expression[0].TokenType == Type.Mark) j = 3;
                if (j>=expression.Count)
                {
                    throw new MyException("Нет правой части", expression[1].line, expression[i].index);
                }
            }
        }

        private void SeparatorOfExpressions()
        {
            for (int i = 0; i < Expressions.Count; i++)
            {
                List<Token> expression = Expressions[i];

                int j = 0;
                while (expression[j].name != "=")
                {
                    if (expression[j].TokenType == Type.Variable)
                    {
                        variables.Add(expression[j]);
                    }
                    j++;
                }

                j++;
                List<Token> rightPart = new List<Token>();
                while (j < expression.Count)
                {
                    if(expression[j].TokenType==Type.Operator && (j+1>=expression.Count ||
                        (expression[j+1].TokenType!=Type.Variable && expression[j+1].TokenType != Type.Integer)))
                    {
                        //if(j+1>=expression.Count) throw new MyException("После оператора должны быть переменная или целое число", expression[j].line, expression[j].index);
                        if(expression[j].name=="(" && expression[j + 1].TokenType == Type.Operator)
                        throw new MyException("После оператора должны быть переменная или целое число", expression[j].line, expression[j].index);

                        if(expression[j+1].name!="(" && expression[j].name != ")")
                        {
                            throw new MyException("После оператора должны быть переменная или целое число", expression[j].line, expression[j].index);
                        }

                        if(expression[j].name==")"&&expression[j+1].TokenType!=Type.Operator)
                        {
                            throw new MyException("После оператора должeн быть оператор", expression[j].line, expression[j].index);
                        }
                    }
                    /*if(expression[j-1].name!="=" && expression[j-1].TokenType==Type.Operator && expression[j].TokenType == Type.Operator)
                    {
                        throw new MyException("Два оператора подряд",expression[j].line, expression[j].index);
                    }*/
                    rightPart.Add(expression[j]);
                    j++;
                }
                RightPartsOfExpressions.Add(rightPart);
            }
        }

        private void Calculate()
        {
            Stack<int> IntOrVariables=new Stack<int>(); 
            Stack<Token> operators=new Stack<Token>();
            List<Token> rightPart;

            for (int i = 0; i < RightPartsOfExpressions.Count; i++)
            {
                rightPart=RightPartsOfExpressions[i];
                if (rightPart[0].name == "-" && operators.Count == 0)
                {
                    rightPart[1].tokenValue = -rightPart[1].tokenValue;
                    rightPart.RemoveAt(0);
                }
                for (int j = 0; j < rightPart.Count; j++)
                {
                    if(rightPart[j].TokenType==Type.Variable || rightPart[j].TokenType==Type.Integer)
                    {
                        foreach(var variable in variables)
                        {
                            if(rightPart[j].name == variable.name)
                            {
                                rightPart[j] = variable;
                                break;
                            }
                        }
                        if(!rightPart[j].tokenValue.HasValue)
                        {
                            throw new MyException("Использование не инициализированной переменной",rightPart[j].line, rightPart[j].index);
                        }
                        IntOrVariables.Push((int)rightPart[j].tokenValue);
                        continue;
                    }

                    if(rightPart[j].TokenType==Type.Operator)
                    {
                        if (rightPart[j].name=="-" || rightPart[j].name == "+") rightPart[j].PriorityOfOperator = 0;
                        if (rightPart[j].name == "/" || rightPart[j].name == "*") rightPart[j].PriorityOfOperator = 1;
                        if (rightPart[j].name == "^") rightPart[j].PriorityOfOperator = 2;

                        if(operators.Count == 0)
                        {
                            operators.Push(rightPart[j]);
                            continue;
                        }

                        if (rightPart[j].name == "(" || rightPart[j].name == ")")
                        {
                            operators.Push(rightPart[j]);
                            continue;
                        }

                        Token operator_= operators.Peek();
                        if(operator_.name == "("  || rightPart[j].PriorityOfOperator>operator_.PriorityOfOperator)
                        {
                            operators.Push(rightPart[j]);
                            continue;
                        }

                        if (rightPart[j].name=="^")
                        {
                            int number = IntOrVariables.Pop();
                            int result = number * number;
                            IntOrVariables.Push(result);
                            continue;
                        }

                        if (rightPart[j].name==")")
                        {
                            while (operator_.name != "(")
                            {
                                operator_ = operators.Pop();
                                int SecondNumber = IntOrVariables.Pop();
                                int FirstNumber = IntOrVariables.Pop();
                                int result = 0;

                                switch (operator_.name)
                                {
                                    case "+":
                                        result = FirstNumber + SecondNumber;
                                        break;
                                    case "-":
                                        result = FirstNumber - SecondNumber;
                                        break;
                                    case "*":
                                        result = FirstNumber * SecondNumber;
                                        break;
                                    case "/":
                                        if (SecondNumber == 0) throw new MyException("Попытка деления на 0", operator_.line, operator_.index + 1);
                                        result = FirstNumber / SecondNumber;
                                        break;
                                }
                                IntOrVariables.Push(result);
                            }
                        }

                        while (rightPart[j].PriorityOfOperator <= operator_.PriorityOfOperator)
                        {
                            int SecondNumber = IntOrVariables.Pop();
                            int FirstNumber= IntOrVariables.Pop();
                            int result = 0;
                            operator_ = operators.Pop();

                            switch (operator_.name)
                            {
                                case "+":
                                    result = FirstNumber + SecondNumber;
                                    break;
                                case "-":
                                    result = FirstNumber - SecondNumber;
                                    break;
                                case "*":
                                    result = FirstNumber * SecondNumber;
                                    break;
                                case "/":
                                    if (SecondNumber == 0) throw new MyException("Попытка деления на 0", operator_.line, operator_.index + 1);
                                    result = FirstNumber / SecondNumber;
                                    break;
                            }

                            IntOrVariables.Push(result);
                            if(operators.Count > 0)
                            {
                                operator_ = operators.Peek();
                                if (operator_.name == "(" || rightPart[j].PriorityOfOperator > operator_.PriorityOfOperator)
                                {
                                    operators.Push(rightPart[j]);
                                    break;
                                }
                            }
                        }
                        //ht
                    }
                }
                if (operators.Count > 0)
                {
                    int SecondNumber = IntOrVariables.Pop();
                    int FirstNumber = IntOrVariables.Pop();
                    int result = 0;
                    Token operator_ = operators.Pop();

                    switch (operator_.name)
                    {
                        case "+":
                            result = FirstNumber + SecondNumber;
                            break;
                        case "-":
                            result = FirstNumber - SecondNumber;
                            break;
                        case "*":
                            result = FirstNumber * SecondNumber;
                            break;
                        case "/":
                            if (SecondNumber == 0) throw new MyException("Попытка деления на 0",operator_.line, operator_.index+1);
                            result = FirstNumber / SecondNumber;
                            break;
                    }
                    IntOrVariables.Push(result);
                }
                variables[i].tokenValue = IntOrVariables.Pop();
            }
        }
    }
}
