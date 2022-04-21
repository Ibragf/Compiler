using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    internal class MyException : Exception
    {
        public readonly int line;
        public readonly int index;
        public MyException(string message, int line, int index) : base(message)
        {
            this.line = line;
            this.index = index;
        }
    }
}
