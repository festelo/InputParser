using System;
using System.Threading;
using InputParser;

namespace InputParserExample
{
    class TimeSpanParsedParameterAttribute : ParsedParameterAttribute
    {
        public override object Parse(string s) => TimeSpan.Parse(s);
    }
    class Program
    {
        static void Main(string[] args)
        {
            Parser.InitFromThisThread(typeof(InputClass), Console.In, Console.Out);
        }
    }

    public static class InputClass
    {
        [DefaultInput]
        public static string Default()
        {
            return "Input is empty";
        }

        [Input("say", "hello", "sayhello")]
        public static string SayHello()
        {
            return "Hello";
        }
        [Input]
        public static int Sum(int a, int b)
        {
            return a + b;
        }
        [Input]
        public static TimeSpan AddHours([TimeSpanParsedParameter] TimeSpan time, int hourse)
        {
            return time.Add(new TimeSpan(hourse,0,0));
        }
    }
}
