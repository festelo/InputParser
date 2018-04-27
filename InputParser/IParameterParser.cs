using System;
using System.Collections.Generic;
using System.Text;

namespace InputParser
{
    public interface IParameterParser
    {
        object Parse(string source);
    }
}
