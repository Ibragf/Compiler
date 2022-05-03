using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Compiler
{
    internal class Tokenizer
    {
        private char[] operators = new char[] {
            '=',
            '-',
            '+',
            '*',
            '/',
            '^',
            '(',
            ')'
        };
        private string[] borders = new string[] {
            "Begin",
            "End"
        };
        private string[] definations = new string[] {
            "Real",
            "Integer"
        };
        public string[] lines { get; private set; }
        bool hasBeginBorder=false;
        bool hasEndBorder = false;
        public string text { get; private set; }
        public List<Token> tokens { get; private set; }

        public Tokenizer(string text)
        {
            this.text = text;
            tokens = new List<Token>();
            if (text != String.Empty && text != null)
            {
                Replacement();
                TokenSeparator();
                SettingType();
                JoinLines();
            }
        }

        private void JoinLines()
        {
            text = String.Empty;
            foreach (string line in lines)
            {
                text += line + "\n";
            }
        }


        //приведение в читаемую форму для разделения на токены
        private void Replacement()
        {
            text = text.Replace("\r", String.Empty);
            text = text.Replace("\t", String.Empty);

            //вставка пробелов возле оператора
            for (int i = 0; i < text.Length; i++)
            {
                for (int j = 0; j < operators.Length; j++)
                {
                    if (text[i] == operators[j])
                    {
                        string s = " " + operators[j].ToString() + " ";
                        text = text.Remove(i, 1);
                        text = text.Insert(i, s);
                        i++;
                        break;
                    }
                }
            }

            text = text.Replace(":", " : ");
            lines = text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                //удалeние пробелов
                Regex regex = new Regex(@"\s+");
                lines[i] = regex.Replace(lines[i], " ");
                regex = new Regex(@"^\s+");
                lines[i] = regex.Replace(lines[i], String.Empty);
                regex = new Regex(@"\s+$");
                lines[i] = regex.Replace(lines[i], String.Empty);

                //работа с метками
                if (lines[i].Contains(":") && lines[i]!=String.Empty)
                {
                    while (lines[i] != String.Empty && lines[i][0] == ':')
                    {
                        lines[i - 1] += lines[i][0];
                        lines[i] = lines[i].Remove(0, 1);
                    }

                    for (int j = 1; j < lines[i].Length; j++)
                    {
                        if (lines[i][0] == ':')
                        {
                            lines[i - 1] += lines[i][0];
                            lines[i] = lines[i].Remove(0, 1);
                            continue;
                        }
                        if (lines[i][j] == ':' && lines[i][j - 1] == ' ')
                        {
                            lines[i] = lines[i].Remove(j - 1, 1);
                            continue;
                        }
                    }
                }
            }

            //for(int i = 1; i < text.Length; i++)
            //{
            //    if (text[i] == ':' && text[i-1]==' ')
            //    {
            //        text = text.Remove(i-1, 1);
            //        continue;
            //    }
            //}
        }
        //разделение на токены
        private void TokenSeparator()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i]!=String.Empty)//(Regex.IsMatch(lines[i], @"[0-9a-z]|[=\-+*/^\(\)]", RegexOptions.IgnoreCase)||lines[i].Contains(":"))
                {
                    //разделение на слова
                    string[] words = lines[i].Split(' ');
                    for (int j = 0; j < words.Length; j++)
                    {
                        if(words[j] !=String.Empty)
                        {
                            Token token = new Token(i + 1, j + 1, words[j]);
                            tokens.Add(token);
                        }
                    }
                }
            }
        }
        //присвоение типа
        private void SettingType()
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                bool flag=false;
                if (tokens[i].name.Contains(":"))
                {
                    int count=tokens[i].name.Split(':').Length-1;
                    if(count > 1)
                    {
                        throw new MyException($"Количество \":\" > 1",tokens[i].line, tokens[i].index);
                    }
                    for(int j = 0; j < tokens[i].name.Length; j++)
                    {
                        if(!Regex.IsMatch(tokens[i].name[j].ToString(),@"[0-9]|[:]") || !Regex.IsMatch(tokens[i].name,@"[0-9]"))
                        {
                            throw new MyException("Метка не целое число",tokens[i].line,tokens[i].index);
                        }
                    }
                    tokens[i].TokenType = Type.Mark;
                    continue;
                    /*if (Regex.IsMatch(tokens[i].name, @"[0-9]", RegexOptions.IgnoreCase) && !check)
                    {
                        tokens[i].TokenType = Type.Mark;
                        flag = true;
                        continue;
                    }*/
                    //else tokens[i].TokenType = Type.Error;
                }
                //if (flag) continue;


                if (tokens[i].name == borders[0])
                {
                    if(hasBeginBorder)
                    {
                        throw new MyException("Ключевое слово Begin уже существует", tokens[i].line,tokens[i].index);
                    }
                    tokens[i].TokenType = Type.BeginBorder;
                    hasBeginBorder=true;
                    continue;
                }
                if (tokens[i].name == borders[1])
                {
                    if (hasEndBorder)
                    {
                        throw new MyException("Ключевое слово End уже существует", tokens[i].line, tokens[i].index);
                    }
                    tokens[i].TokenType = Type.EndBorder;
                    hasEndBorder=true;
                    continue;
                }


                foreach (string defination in definations)
                {
                    if (tokens[i].name == defination)
                    {
                        tokens[i].TokenType = Type.Keyword;
                        flag = true;
                        break;
                    }
                }
                if(flag) continue;

                foreach (char oper in operators)
                {
                    if (tokens[i].name==oper.ToString())
                    {
                        tokens[i].TokenType = Type.Operator;
                        flag=true;
                        break;
                    }
                }
                if (flag) continue;

                VariableOrInteger(tokens[i]);
            }
        }

        private void VariableOrInteger(Token token)
        {
            bool isIntegerOrVar = Int32.TryParse(token.name, out int example);
            if(isIntegerOrVar)
            {
                token.TokenType = Type.Integer;
                token.tokenValue = example;
                return;
            }
            else
            {
                //проверка на наличие неразрешенных символов
                int SymbolIndex = 0;
                for(int i=0; i<token.name.Length; i++)
                {
                    if(!Regex.IsMatch(token.name[i].ToString(), @"[0-9a-z]", RegexOptions.IgnoreCase))
                    {
                        SymbolIndex=i+1;
                        throw new MyException($"Присутствует недопустимый символ.\n" +
                            $"Индекс символа в слове: {SymbolIndex}\n" +
                            $"Символ: {token.name[i]}",
                            token.line, token.index);
                    }
                }
                isIntegerOrVar = Int32.TryParse(token.name[0].ToString(), out example);
                if (isIntegerOrVar) throw new MyException("Первый символ не буква", token.line, token.index);
                if (!isIntegerOrVar)
                {
                    token.TokenType = Type.Variable;
                }
            }
        }
    }
}
