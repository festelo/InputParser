using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace InputParser
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InputFunctionAttribute : Attribute
    {
        public string[] Names { get; set; }

        public InputFunctionAttribute([CallerMemberName] string name = "") : this(new [] {name})
        {
        }
        public InputFunctionAttribute(params string[] names)
        {
            Names = names;
        }
    }
}
