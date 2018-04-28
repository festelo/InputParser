using System;
using System.Collections.Generic;
using System.Text;

namespace InputParser
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ReadingEndedAttribute : InputAttribute
    {
        public ReadingEndedAttribute() : base(names: null) { }
    }
}
