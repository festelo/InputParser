using System;
using System.Reflection;
using System.Threading;
using InputParser;

namespace InputParserExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Init(typeof(InputClass), Console.In, Console.Out);
            while (true)
            {
                Thread.Sleep(10000);
            }
        }
    }

    public static class InputClass
    {
        [InputFunction("say", "hello", "sayhello")]
        public static string SayHello()
        {
            return "Hello";
        }
        [InputFunction]
        public static int Sum(int a, int b)
        {
            return a + b;
        }
        [InputFunction]
        public static TimeSpan AddHours([InputParameter(typeof(TimeSpan), nameof(TimeSpan.Parse))]TimeSpan time, int hourse)
        {
            return time.Add(new TimeSpan(hourse,0,0));
        }
    }
}
