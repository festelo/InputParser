using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InputParser
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ParsedParameterAttribute : Attribute
    {
        public abstract object Parse(string source);
    }
}
