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

            if (Definations != null && Expressions != null)
            {
                CheckDefinations();
                isParenthesesBalanced();
                SeparatorOfExpressions();
                CheckRightPart();
                Calculate();
            }
        }

        public List<Token> GetVariables()
        {
            return variables;
        }


        private void CheckDefinations()
        {
            if(Definations.Count==0)
            {
                throw new MyException("Отсутствует определение в программе", 1,1);
            }
            if(Expressions.Count==0)
            {
                Token defination = Definations.Last().Last();
                throw new MyException("Отсутствует оператор в программе", defination.line, defination.index);
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
                        throw new MyException("Нарушен баланс скобок.\nСлишком много закрывающих скобок",token.line,token.index);
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
                    if (expression[0].TokenType == Type.Mark && 1 >= expression.Count) 
                        throw new MyException("После метки должна быть переменная", expression[0].line, expression[0].index);
                    if (expression[0].TokenType == Type.Mark && 1 < expression.Count && expression[1].TokenType != Type.Variable)
                        throw new MyException("После метки должна быть переменная", expression[0].line, expression[0].index);
                    if(expression[0].TokenType == Type.Mark && 1<expression.Count && expression[1].TokenType==Type.Variable)
                        throw new MyException("После переменной должен быть знак \"=\"",expression[1].line,expression[1].index);
                    throw new MyException("После переменной должен быть знак \"=\"", expression[0].line, expression[0].index);
                }
                if (stack.Count > 0)
                {
                    Token token = stack.Pop();
                    throw new MyException("Нарушен баланс скобок.\nСлишком много открывающих скобок", token.line, token.index);
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
                    throw new MyException("Нет правой части", expression[1].line, expression[expression.Count-1].index);
                }
            }
        }

        private void SeparatorOfExpressions()
        {
            for (int i = 0; i < Expressions.Count; i++)
            {
                List<Token> expression = Expressions[i];

                int j = 0;
                int count = 0;
                while (expression[j].name != "=")
                {
                    if (expression[j].TokenType!=Type.Mark && expression[j].TokenType!=Type.Variable)
                    {
                        throw new MyException($"Левая часть оператора не может содержать {expression[j].nameOfType()}",expression[j].line,expression[j].index) ;
                    }
                    if (expression[j].TokenType == Type.Variable)
                    {
                        count++;
                        variables.Add(expression[j]);
                    }
                    if (count > 1)
                    {
                        Token t = expression.First(x => x.TokenType == Type.Variable);
                        throw new MyException($"После переменной должен быть знак \"=\"", t.line, t.index);
                    }
                    j++;
                }
                if (count == 0)
                {
                    throw new MyException($"Левая часть оператора не содержит переменной", expression[j].line, expression[j].index);
                }

                j++;
                List<Token> rightPart = new List<Token>();
                while (j < expression.Count)
                {
                    if(expression[j].TokenType==Type.Operator && (j+1>=expression.Count ||
                        (expression[j+1].TokenType!=Type.Variable && expression[j+1].TokenType != Type.Integer)))
                    {
                        if (j == expression.Count-1 && expression[j].name != ")") {

                            throw new MyException("Выражение не может заканчиваться математической операцией", expression[j].line, expression[j].index);
                        }
                        //if(j+1>=expression.Count) throw new MyException("После оператора должны быть переменная или целое число", expression[j].line, expression[j].index);
                        if(j + 1 < expression.Count)
                        {
                            if (expression[j].name == "-" && expression[j + 1].name == ")")
                            {
                                throw new MyException("После математической операции не может идти закрывающая скобка", expression[j].line, expression[j].index);
                            }

                            if (expression[j].name=="-" && expression[j+1].name!="(")
                            {
                                throw new MyException("Две математические операции подряд", expression[j].line, expression[j].index);
                            }

                            if(expression[j].name!="-" && expression[j].name!=")" && expression[j+1].name==")")
                            {
                                throw new MyException("После математической операции не может идти закрывающая скобка", expression[j].line, expression[j].index);
                            }

                            if (expression[j].name == "(" && expression[j + 1].TokenType == Type.Operator && expression[j + 1].name != "(" && expression[j + 1].name != "-")
                                throw new MyException($"После открывающей скобки не может стоять знак \"{expression[j+1].name}\"", expression[j].line, expression[j].index);
                            /*if (expression[j].name == ")" && expression[j + 1].TokenType == Type.Variable && expression[j + 1].name != ")")
                                throw new MyException("После оператора должны быть переменная или целое число", expression[j].line, expression[j].index);
                            */
                            /*if (expression[j+1].name == "-" &&  (expression[j].name != ")" && expression[j].name != "^" && expression[j + 1].name != "("))
                            {
                                throw new MyException("После оператора должны быть переменная или целое число1", expression[j].line, expression[j].index);
                            }*/

                            if((expression[j+1].TokenType==Type.Operator && expression[j+1].name!="(" && expression[j+1].name!="^" && expression[j].name != ")") &&
                                !(expression[j].name=="(" && expression[j+1].name=="-"))
                            {
                                throw new MyException("Две математические операции подряд", expression[j].line, expression[j].index);
                            }

                            if(expression[j].name!=")" && expression[j+1].name == "^")
                            {
                                throw new MyException("Две математические операции подряд", expression[j].line, expression[j].index);
                            }

                            if (expression[j].name == ")" && (expression[j + 1].TokenType != Type.Operator || expression[j + 1].name == "("))
                            {
                                throw new MyException("После закрывающей скобки должна быть математическая операция", expression[j].line, expression[j].index);
                            }
                        }
                    }
                    if (j + 1 < expression.Count && expression[j].name == ")" && expression[j + 1].TokenType != Type.Operator)
                    {
                        throw new MyException("После закрывающей скобки должна быть математическая операция", expression[j].line, expression[j].index);
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
                bool variableFlag = false;
                Token globalOpenPar = null;
                rightPart=RightPartsOfExpressions[i];
                if (rightPart[0].TokenType ==Type.Operator  && operators.Count == 0)
                {
                    if(rightPart[0].name !="-" && rightPart[0].name != "(")
                        throw new MyException("Перед математической операцией должны быть переменная или целое число",rightPart[0].line, rightPart[0].index);
                    bool isChanged=false;
                    if(rightPart[1].TokenType ==Type.Integer && rightPart[0].name!="(")
                    {
                        rightPart[1].tokenValue = -rightPart[1].tokenValue;
                        rightPart.RemoveAt(0);
                        isChanged=true;
                    }

                    if (rightPart.Count>1 && rightPart[1].TokenType == Type.Variable && rightPart[0].name != "(")
                    {
                        variableFlag= true;
                        rightPart.RemoveAt(0);
                    }
                    else if(!isChanged)
                    {
                        if(rightPart[0].name != "(")
                        {
                            flag = true;
                            globalOpenPar = rightPart[1];
                            rightPart.RemoveAt(0);
                        }
                        else
                        {
                            operators.Push(rightPart[0]);
                            rightPart.RemoveAt(0);
                        }
                    }
                }
                bool localFlag = false;
                Token openParenthesis = null;
                for (int j = 0; j < rightPart.Count; j++)
                {
                    if (rightPart[j].TokenType==Type.Variable || rightPart[j].TokenType==Type.Integer)
                    {
                        int? value=null;
                        value = rightPart[j].tokenValue;
                        foreach (var variable in variables)
                        {
                            if(rightPart[j].name == variable.name)
                            {
                                value = variable.tokenValue;
                                if(variableFlag) value=-value;
                                variableFlag= false;
                                break;
                            }
                        }
                        if (!value.HasValue)
                        {
                            throw new MyException("Использование не инициализированной переменной",rightPart[j].line, rightPart[j].index);
                        }
                        IntOrVariables.Push((int)value);
                        continue;
                    }

                    #region минус перед значением
                    if (j > 0)
                    {
                        if (rightPart[j].name == "-" && (rightPart[j - 1].name != ")" && rightPart[j-1].TokenType!=Type.Variable && rightPart[j-1].TokenType!=Type.Integer)
                            && rightPart[j + 1].TokenType != Type.Operator && operators.Peek().name == "(")
                        {
                            rightPart[j + 1].tokenValue = -rightPart[j + 1].tokenValue;
                            continue;
                        }

                        if (rightPart[j].name == "-" && operators.Count > 0 && operators.Peek().name == "(" && j + 1 < rightPart.Count && rightPart[j + 1].name == "("
                        && rightPart[j-1].name!=")")
                        {
                            localFlag = true;
                            openParenthesis = rightPart[j + 1];
                            continue;
                        }
                    }
                    else if(j==0)
                    {
                        if(rightPart[j].name == "-"  && rightPart[j + 1].TokenType != Type.Operator && operators.Peek().name == "(")
                        {
                            rightPart[j + 1].tokenValue = -rightPart[j + 1].tokenValue;
                            continue;
                        }
                    }

                    if (rightPart[j].name == "-" && operators.Count>0 && operators.Peek().name == "(" && j + 1 < rightPart.Count && rightPart[j + 1].name == "("
                        && j==0)
                    {
                        localFlag = true;
                        openParenthesis = rightPart[j + 1];
                        continue;
                    }
                    #endregion


                    if (rightPart[j].name=="(")
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
                            while(operators.Count!=0 && oper.name != "(")
                            {
                                oper = operators.Pop();
                                int SecondNumber = IntOrVariables.Pop();
                                int FirstNumber = IntOrVariables.Pop();
                                int result = 0;

                                operation(oper, FirstNumber, SecondNumber, ref result);
                                if (operators.Count > 0) oper = operators.Peek();
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

                                operation(oper, FirstNumber, SecondNumber, ref result);
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

                            operation(oper, FirstNumber, SecondNumber, ref result);
                            oper = operators.Pop();

                            if (localFlag && oper.name=="(")
                            {
                                if(oper.index==openParenthesis.index)
                                {
                                    result = -result;
                                    localFlag = false;
                                }
                            }

                            if(flag && oper.name=="(")
                            {
                                if(oper.index==globalOpenPar.index)
                                {
                                    result = -result;
                                    flag = false;
                                }
                            }

                            IntOrVariables.Push(result);
                        }
                    }
                }


                while(operators.Count > 0)
                {
                    Token oper = operators.Pop();
                    int SecondNumber = IntOrVariables.Pop();
                    int FirstNumber = IntOrVariables.Pop();
                    int result = 0;

                    operation(oper, FirstNumber, SecondNumber, ref result);
                    IntOrVariables.Push(result);
                }
                if(IntOrVariables.Count>1)
                {
                    int SecondNumber = IntOrVariables.Pop();
                    int FirstNumber = IntOrVariables.Pop();
                    IntOrVariables.Push(SecondNumber + FirstNumber);
                }
                variables[i].tokenValue = IntOrVariables.Pop();
                if(flag)
                {
                    variables[i].tokenValue=-variables[i].tokenValue;
                    flag = false;
                }
                for(int k=0;k<variables.Count;k++)
                {

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
                IntOrVariables.Clear();
            }
        }

        private void operation(Token oper, int FirstNumber, int SecondNumber, ref int result)
        {
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
            if (result == Int32.MaxValue) result = result+ 1000000;
            if (result == Int32.MinValue) result = result-1000000;
        }
    }
}
