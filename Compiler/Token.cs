using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    internal class Token
    {
        public readonly int line;
        public readonly int index;
        private Type tokenType;
        public readonly string name;
        public int? tokenValue=null;
        private int? priorityOfOperator = null;


        public int? PriorityOfOperator
        {
            get
            {
                return priorityOfOperator;
            }
            set
            {
                if(!priorityOfOperator.HasValue) priorityOfOperator = value;
            }
        }
        public Type TokenType
        {
            get
            {
                return tokenType;
            }
            set
            {
                if (tokenType == Type.Error)
                {
                    tokenType = value;
                }
            }
        }

        public Token(int line, int index, string name)
        {
            this.line = line;
            this.index = index;
            this.name = name;
        } 

        public string nameOfType()
        {
            switch (tokenType)
            {
                case Type.Error: return "ошибка";
                case Type.Integer: return "целое число";
                case Type.Variable: return "переменная";
                case Type.BeginBorder: return "begin";
                case Type.EndBorder: return "end";
                case Type.Keyword: return $"ключевое слово {name}";
                case Type.Mark: return "метка";
                case Type.Operator: return "математическая операция";
            }
            return "DON'T HAVE nameOFtype";
        }
    }
}
