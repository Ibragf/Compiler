using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    internal class AnalyzerOfTokens
    {
        private List<Token> tokens;
        public List<List<Token>> Definations { get; private set; }
        public List<List<Token>> arithmeticExpressions { get; private set; }

        public AnalyzerOfTokens(Tokenizer tokenizer)
        {
            this.tokens = tokenizer.tokens;
            if (tokens.Count > 0)
            {
                Definations = new List<List<Token>>();
                arithmeticExpressions = new List<List<Token>>();
                CheckTokens();
            }
        }

        private void CheckTokens()
        {
            //проверка границ программы
            if (tokens[0].TokenType != Type.BeginBorder)
            {
                foreach (Token token in tokens)
                {
                    if (token.TokenType == Type.BeginBorder)
                    {
                        throw new MyException("Перед словом Begin есть текст", tokens[0].line, tokens[0].index);
                    }
                }
                throw new MyException("Программа должна начинаться со слова Begin", tokens[0].line, tokens[0].index);
            }

            int i = 0;
            while(i<tokens.Count)
            {
                if (tokens[i].name == "Real")
                {
                    List<Token> defination=new List<Token>();
                    defination.Add(tokens[i]);
                    if (i+1 >= tokens.Count) throw new MyException("Real не содержит переменной", tokens[i].line, tokens[i].index);
                    if(tokens[i+1].TokenType==Type.Variable)
                    {
                        defination.Add(tokens[i+1]);
                        i=i+2;
                    }
                    else throw new MyException("Real не содержит переменной", tokens[i].line, tokens[i].index);

                    bool wasAdd = false;
                    while(i<tokens.Count && tokens[i].TokenType!=Type.Keyword && tokens[i].name!="="&&tokens[i].TokenType!=Type.Mark)
                    {
                        if (tokens[i].TokenType != Type.Variable) throw new MyException($"Определение Real не может содержать {tokens[i].nameOfType()}",tokens[i].line,tokens[i].index);

                        defination.Add(tokens[i]);



                        i++;
                        if (i<tokens.Count && tokens[i].name == "=")
                        {
                            defination.RemoveAt(defination.Count - 1);
                            if (defination.Count(x => x.TokenType == Type.Variable) == 0)
                                throw new MyException("Real не содержит переменной", tokens[i].line, tokens[i].index);
                            i--;
                            Definations.Add(defination);
                            wasAdd = true;
                            break;
                        }
                    }
                    if(!wasAdd) Definations.Add(defination);
                    continue;
                }

                if(tokens[i].name=="Integer")
                {
                    List<Token> defination=new List<Token>();
                    defination.Add(tokens[i]);
                    if(i+1>=tokens.Count) throw new MyException("Integer не содержит целое число",tokens[i].line,tokens[i].index);
                    if(tokens[i+1].TokenType==Type.Integer)
                    {
                        defination.Add(tokens[i+1]);
                        i=i+2;
                    }
                    else throw new MyException("Integer не содержит целое число", tokens[i].line, tokens[i].index);

                    while (i<tokens.Count && tokens[i].TokenType!=Type.Keyword&&tokens[i].TokenType!=Type.Mark&&tokens[i].TokenType!=Type.Variable)
                    {
                        if (tokens[i].TokenType != Type.Integer) throw new MyException($"Определение Integer не может содержать {tokens[i].nameOfType()}", tokens[i].line, tokens[i].index);

                        defination.Add(tokens[i]);
                        i++;
                    }
                    Definations.Add(defination);
                    continue;
                }
                
                if(tokens[i].TokenType==Type.Mark)
                {
                    if (i + 1 >= tokens.Count) throw new MyException("После метки нет переменной",tokens[i].line, tokens[i].index);
                    if(tokens[i+1].TokenType!=Type.Variable)
                    {
                        throw new MyException("После метки должна идти переменная",tokens[i+1].line,tokens[i+1].index);
                    }
                    i++;
                }

                if(tokens[i].TokenType == Type.Variable)
                {
                    List<Token> expression = new List<Token>();
                    expression.Add(tokens[i]);
                    if(Definations.Count==0) throw new MyException("Отсутствует определение в программе",tokens[i].line,tokens[i].index);
                    if (i + 1 >= tokens.Count) throw new MyException("Отсутствует \"=\" после переменной",tokens[i].line,tokens[i].index);
                    if (tokens[i + 1].name != "=") throw new MyException("Отсутствует \"=\" после переменной", tokens[i].line, tokens[i].index);
                    else
                    {
                        expression.Add(tokens[i + 1]);
                        i = i + 2;
                    }

                    bool wasAdded = false;
                    while(i<tokens.Count && tokens[i].name != "=" && tokens[i].TokenType!=Type.Mark && tokens[i].TokenType!=Type.EndBorder)
                    {
                        if (tokens[i].TokenType != Type.Variable && tokens[i].TokenType != Type.Integer && tokens[i].TokenType != Type.Operator)
                        {
                            throw new MyException($"Оператор не может содержать {tokens[i].nameOfType()}", tokens[i].line, tokens[i].index);
                        }
                        else
                        {
                            if(i+1<tokens.Count && tokens[i-1].TokenType!=Type.Operator && tokens[i].TokenType != Type.Operator && tokens[i+1].TokenType != Type.Operator)
                            {
                                if(tokens[i - 1].TokenType == Type.Variable && tokens[i].TokenType == Type.Variable)
                                {
                                    throw new MyException("Две переменные подряд",tokens[i-1].line,tokens[i-1].index);
                                }
                                throw new MyException($"Между {tokens[i - 1].nameOfType()} и {tokens[i].nameOfType()} нет математической операции", tokens[i-1].line, tokens[i-1].index);
                            }

                            expression.Add(tokens[i]);
                            i++;

                            if (tokens[i].name == "=")
                            {
                                expression.RemoveAt(expression.Count - 1);
                                i--;
                                arithmeticExpressions.Add(expression);
                                wasAdded = true;
                                break;
                            }
                        }
                    }
                    if (!wasAdded) arithmeticExpressions.Add(expression);
                    continue;
                }

                if(tokens[i].TokenType!=Type.BeginBorder && tokens[i].TokenType!=Type.EndBorder)
                {
                    if(i-1>=0 && tokens.Count>0)
                        throw new MyException($"После {tokens[i-1].name} не может идти {tokens[i].nameOfType()}", tokens[i].line, tokens[i].index);
                    throw new MyException($"Неверное использование {tokens[i].nameOfType()}", tokens[i].line, tokens[i].index);
                }
                i++;
            }

            if (tokens[tokens.Count - 1].TokenType != Type.EndBorder)
            {
                foreach (Token token in tokens)
                {
                    if (token.TokenType == Type.EndBorder)
                    {
                        throw new MyException("После слова End есть текст", tokens[tokens.Count - 1].line, tokens[tokens.Count - 1].index);
                    }
                }
                throw new MyException("Программа должна заканчиваться словом End", tokens[tokens.Count - 1].line, tokens[tokens.Count - 1].index);
            }

        }
    }
}
