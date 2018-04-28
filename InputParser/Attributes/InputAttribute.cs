using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace InputParser
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InputAttribute : Attribute
    {
        public string[] Names { get; }

        public InputAttribute(string[] names = null, [CallerMemberName] string defname = null)
        {
            Names = names ?? new [] { defname };
        }
    }
}
