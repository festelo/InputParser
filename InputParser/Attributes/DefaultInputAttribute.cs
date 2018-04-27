using System;

namespace InputParser
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DefaultInputAttribute : InputAttribute
    {
        public DefaultInputAttribute() : base(names: null) { }
    }
}
