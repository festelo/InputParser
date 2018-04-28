using System;
using System.Globalization;
using System.Threading;
using InputParser;

namespace InputParserExample
{
    class TimeSpanParsedParameterAttribute : ParsedParameterAttribute
    {
        public override object Parse(string s) => TimeSpan.Parse(s, CultureInfo.InvariantCulture);
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
        /*
        [DefaultInput]
        public static string Default()
        {
            return "Command not found";
        }

        OR
        */

        [DefaultInput]
        public static string Default(string inp)
        {
            return "Command not found. Your input is " + inp;
        }

        [ReadingEnded]
        public static string Null()
        {
            return "Reading ended. ^C received";
        }

        [Input]
        public static string Method([TimeSpanParsedParameter]TimeSpan time = default(TimeSpan), int a = -1)
        {
            return "Hours is " + time.Hours + "\nNumber plus five is " + (a+5) + "\n";
        }

        [Input(new[] { "say", "hello", "sayhello" })]
        public static string SayHello(string name = null)
        {
            return name == null? "Hello!" : "Hello, " + name;
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
