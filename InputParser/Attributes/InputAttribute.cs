using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace InputParser
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InputAttribute : Attribute
    {
        public string[] Names { get; set; }

        public InputAttribute([CallerMemberName] string name = "") : this(new [] {name})
        {
        }
        public InputAttribute(params string[] names)
        {
            Names = names;
        }
    }
}
