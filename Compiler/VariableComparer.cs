using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    internal class VariableComparer : IComparer<Token>
    {
        public int Compare(Token x, Token y)
        {
            return x.line - y.line;
        }
    }
}
