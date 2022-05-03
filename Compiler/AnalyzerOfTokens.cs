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


            bool dicrement = false;
            for (int i = 1; i < tokens.Count-1; i++)
            {
                if (dicrement) i--;
                dicrement = false;


                if (tokens[i].TokenType==Type.Keyword)
                {
                    List<Token> defination = new List<Token>();
                    defination.Add(tokens[i]);
                    for (int j = i + 1; j < tokens.Count; j++)
                    {
                        if(tokens[i].name=="Real")
                        {
                            if (tokens[j].TokenType == Type.Variable)
                            {
                                defination.Add(tokens[j]);
                                continue;
                            }
                            /*if(defination.Count == 1)
                            {
                                throw new MyException("Определение Real",tokens[i].line,tokens[i].index);
                            }*/

                            if (tokens[j].name!="=" && tokens[j].TokenType!=Type.Mark && tokens[j].TokenType!=Type.Keyword && tokens[j].TokenType != Type.EndBorder)
                            {
                                //ошибка : не пер и не метка или не оператор
                                throw new MyException("После определения Real должно быть:\nметка или\nключевое слово или\nпеременная или\nEnd", tokens[j].line, tokens[j].index);
                            }
                        }
                        if(tokens[i].name=="Integer")
                        {
                            if(tokens[j].TokenType == Type.Integer)
                            {
                                defination.Add(tokens[j]);
                                continue;
                            }

                            if (tokens[j].TokenType != Type.Mark && tokens[j].TokenType != Type.Keyword && tokens[j].TokenType != Type.Variable &&
                                    /*(tokens[j].TokenType == Type.Variable && tokens[j+1].name != "=") &&*/ tokens[j].TokenType != Type.EndBorder)
                            {
                                //ошибка : не пер и не метка или не оператор
                                throw new MyException("После определения Integer должно быть:\nметка или\nключевое слово или\nпеременная или\nEnd ", tokens[j].line, tokens[j].index);
                            }
                        }

                        if(defination.Count==1)
                        {
                            string VarOrInt = (tokens[i].name == "Real") ? "переменной" : "целое число";
                            throw new MyException($"Определение {tokens[i].name} не содержит\n{VarOrInt}",tokens[i].line,tokens[i].index);
                        }

                        if(tokens[j].TokenType==Type.Mark || tokens[j].TokenType==Type.Keyword || tokens[j].TokenType == Type.EndBorder)
                        {
                            i = j; //для пропуска всех добавляемых токенов
                            Definations.Add(defination);
                            defination = null;
                            dicrement = true;
                            break;
                        }

                        if(tokens[j].TokenType==Type.Operator)
                        {
                            i = j - 1;
                            defination.RemoveAt(defination.Count-1);
                            if (defination.Count == 1)
                            {
                                throw new MyException($"Определение Real не содержит\n переменной", tokens[i].line, tokens[i].index);
                            }
                            Definations.Add(defination);
                            dicrement = true;
                            defination = null;
                            break;
                        }

                        if (tokens[j].TokenType == Type.Variable)
                        {
                            i = j-1;
                            Definations.Add(defination);
                            dicrement = true;
                            defination = null;
                            break;
                        }

                    }
                }


                if(tokens[i].TokenType==Type.Mark || tokens[i].TokenType==Type.Variable)
                {
                    List<Token> expression = new List<Token>();
                    expression.Add(tokens[i]);
                    bool hasEqualOper = false;
                    for (int j=i+1;j<tokens.Count;j++)
                    {
                        if (tokens[j].TokenType==Type.Variable || tokens[j].TokenType==Type.Operator || tokens[j].TokenType == Type.Integer)
                        {
                            if (tokens[j].name == "=" && hasEqualOper)
                            {
                                i = j - 1;
                                dicrement = true;
                                expression.RemoveAt(expression.Count - 1);
                                arithmeticExpressions.Add(expression);
                                expression = null;
                                hasEqualOper = false;
                                break;
                            }
                            if (tokens[j].name == "=") hasEqualOper = true;

                            expression.Add(tokens[j]);
                            continue;
                        }

                        if(tokens[j].TokenType==Type.Keyword)
                        {
                            throw new MyException("Определение должно объявляться до операторов", tokens[j].line,tokens[j].index);
                        }

                        if(tokens[j].TokenType!=Type.Mark && tokens[j].TokenType!=Type.EndBorder)
                        {
                            // ошибка :  не метка и не End
                            throw new MyException("После ариф.выражения стоит неверное слово",tokens[j].line,tokens[j].index);
                        }

                        if(tokens[j].TokenType==Type.Mark || tokens[j].TokenType == Type.EndBorder)
                        {
                            i = j;
                            dicrement = true;
                            arithmeticExpressions.Add(expression);
                            expression = null;
                            break;
                        }
                    }
                }
            }
        }
    }
}
