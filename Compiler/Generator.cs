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

        public List<Token> GetVariables()
        {
            return variables;
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
                        if (j + 1 >= expression.Count && expression[j].name != ")") {

                            throw new MyException("После оператора должны быть переменная или целое число", expression[j].line, expression[j].index);
                        }
                        //if(j+1>=expression.Count) throw new MyException("После оператора должны быть переменная или целое число", expression[j].line, expression[j].index);
                        if(j + 1 < expression.Count)
                        {
                            if (expression[j].name == "(" && expression[j + 1].TokenType == Type.Operator && expression[j + 1].name != "(")
                                throw new MyException("После оператора должны быть переменная или целое число", expression[j].line, expression[j].index);

                            if (expression[j].name != ")" && expression[j + 1].name != "(")
                            {
                                throw new MyException("После оператора должны быть переменная или целое число", expression[j].line, expression[j].index);
                            }

                            if (expression[j].name == ")" && expression[j + 1].TokenType != Type.Operator)
                            {
                                throw new MyException("После оператора должeн быть оператор", expression[j].line, expression[j].index);
                            }
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
                bool flag = false;
                rightPart=RightPartsOfExpressions[i];
                if (rightPart[0].TokenType ==Type.Operator  && operators.Count == 0)
                {
                    if(rightPart[0].name !="-" && rightPart[0].name != "(")
                        throw new MyException("Перед оператором должны быть переменная или целое число",rightPart[0].line, rightPart[0].index);

                    if((rightPart[1].TokenType ==Type.Integer || rightPart[1].TokenType==Type.Variable) && rightPart[0].name!="(")
                    {
                        rightPart[1].tokenValue = -rightPart[1].tokenValue;
                        rightPart.RemoveAt(0);
                    }
                    else
                    {
                        if(rightPart[0].name != "(")
                        {
                            flag = true;
                            rightPart.RemoveAt(0);
                        }
                        else
                        {
                            operators.Push(rightPart[0]);
                            rightPart.RemoveAt(0);
                        }
                    }
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

                    if(rightPart[j].name=="(")
                    {
                        operators.Push(rightPart[j]);
                        continue;
                    }

                    if(rightPart[j].name=="^")
                    {
                        rightPart[j].PriorityOfOperator = 2;
                        if (operators.Count == 0)
                        {
                            operators.Push(rightPart[j]);
                            continue;
                        }

                        Token oper = operators.Peek();
                        if (oper.name == "(")
                        {
                            operators.Push(rightPart[j]);
                            continue;
                        }

                        operators.Push(rightPart[j]);
                        continue;
                    }

                    if(rightPart[j].name=="+" || rightPart[j].name == "-")
                    {
                        rightPart[j].PriorityOfOperator = 0;
                        if (operators.Count == 0)
                        {
                            operators.Push(rightPart[j]);
                            continue;
                        }

                        Token oper = operators.Peek();
                        if (oper.name == "(")
                        {
                            operators.Push(rightPart[j]);
                            continue;
                        }


                        if(rightPart[j].PriorityOfOperator<=oper.PriorityOfOperator)
                        {
                            while(operators.Count!=0 || oper.name == "(")
                            {
                                oper = operators.Pop();
                                int SecondNumber = IntOrVariables.Pop();
                                int FirstNumber = IntOrVariables.Pop();
                                int result = 0;

                                switch (oper.name)
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
                                        if (SecondNumber == 0) throw new MyException("Попытка деления на 0", oper.line, oper.index + 1);
                                        result = FirstNumber / SecondNumber;
                                        break;
                                    case "^":
                                        result = (int)Math.Pow(FirstNumber,SecondNumber);
                                        break;
                                }
                                IntOrVariables.Push(result);
                            }
                            operators.Push(rightPart[j]);
                            continue;
                        }
                    }

                    if(rightPart[j].name == "*" || rightPart[j].name == "/")
                    {
                        rightPart[j].PriorityOfOperator = 1;
                        if (operators.Count == 0)
                        {
                            operators.Push(rightPart[j]);
                            continue;
                        }

                        Token oper = operators.Peek();
                        if (oper.name == "(")
                        {
                            operators.Push(rightPart[j]);
                            continue;
                        }

                        if (rightPart[j].PriorityOfOperator <= oper.PriorityOfOperator)
                        {
                            while (operators.Count != 0 || oper.name == "(" || rightPart[j].PriorityOfOperator > oper.PriorityOfOperator)
                            {
                                oper = operators.Pop();
                                int SecondNumber = IntOrVariables.Pop();
                                int FirstNumber = IntOrVariables.Pop();
                                int result = 0;

                                switch (oper.name)
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
                                        if (SecondNumber == 0) throw new MyException("Попытка деления на 0", oper.line, oper.index + 1);
                                        result = FirstNumber / SecondNumber;
                                        break;
                                    case "^":
                                        result = (int)Math.Pow(FirstNumber, SecondNumber);
                                        break;
                                }
                                IntOrVariables.Push(result);
                            }
                            operators.Push(rightPart[j]);
                        }
                        else operators.Push(rightPart[j]);
                        continue;
                    }

                    if(rightPart[j].name == ")")
                    {
                        Token oper=operators.Pop();
                        while (oper.name != "(")
                        {
                            //oper = operators.Pop();
                            int SecondNumber = IntOrVariables.Pop();
                            int FirstNumber = IntOrVariables.Pop();
                            int result = 0;

                            switch (oper.name)
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
                                    if (SecondNumber == 0) throw new MyException("Попытка деления на 0", oper.line, oper.index + 1);
                                    result = FirstNumber / SecondNumber;
                                    break;
                                case "^":
                                    result = (int)Math.Pow(FirstNumber, SecondNumber);
                                    break;
                            }
                            oper = operators.Pop();

                            if (flag)
                            {
                                bool hasOpen = false;
                                foreach (Token token in operators)
                                {
                                    if (token.name == "(")
                                    {
                                        hasOpen = true;
                                        break;
                                    }
                                }
                                if(!hasOpen)
                                {
                                    result = -result;
                                    flag = false;
                                }
                            }

                            IntOrVariables.Push(result);
                        }
                        //oper = operators.Pop();
                    }
                }


                while(operators.Count > 0)
                {
                    Token oper = operators.Pop();
                    int SecondNumber = IntOrVariables.Pop();
                    int FirstNumber = IntOrVariables.Pop();
                    int result = 0;

                    switch (oper.name)
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
                            if (SecondNumber == 0) throw new MyException("Попытка деления на 0", oper.line, oper.index + 1);
                            result = FirstNumber / SecondNumber;
                            break;
                        case "^":
                            result = (int)Math.Pow(FirstNumber, SecondNumber);
                            break;
                    }
                    IntOrVariables.Push(result);
                }
                variables[i].tokenValue = IntOrVariables.Pop();
                //int count=0;
                for(int k=0;k<variables.Count;k++)
                {
                    /*if(variables[k].name == variables[i].name && i==k)
                    {
                        variables[k] = variables[i];
                        break;
                    }*/
                    if(variables[k].name == variables[i].name && variables[k].tokenValue != variables[i].tokenValue && i != k)
                    {
                        //попробовать связать с позицией (i = k)
                        variables[k] = variables[i];
                        variables.RemoveAt(i);
                        RightPartsOfExpressions.RemoveAt(i);
                        i--;
                        break;
                    }
                }
                operators = new Stack<Token>();
                IntOrVariables = new Stack<int>();
            }
        }
    }
}
