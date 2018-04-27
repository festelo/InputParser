using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace InputParser
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class InputParameterAttribute : Attribute
    {
        public Type ParseMethodClassType { get; set; }
        public string ParseMethodName { get; set; }
        public bool UseParseMethod { get; set; }
        public InputParameterAttribute(Type parsemethodclasstype, string parsemethodname, bool useParseMethod = true)
        {
            ParseMethodClassType = parsemethodclasstype;
            ParseMethodName = parsemethodname;
            UseParseMethod = useParseMethod;
        }
    }
}
