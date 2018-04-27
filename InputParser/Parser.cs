using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InputParser
{
    public class Parser
    {
        private static Parser CreateParser(Type parserclass, TextReader reader, TextWriter writer)
        {
            var parser = new Parser();
            var methods = parserclass.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<InputFunctionAttribute>();
                if (attribute != null) parser.Methods.Add((attribute, method));

                var nullattribute = method.GetCustomAttribute<NullInputFunctionAttribute>();
                if (nullattribute != null)  parser.NullInputMethod = method;
            }
            parser.TextReader = reader;
            parser.TextWriter = writer;
            return parser;
        }
        public static void InitFromThisThread(Type parserclass, TextReader reader, TextWriter writer, CancellationToken token)
        {
            var parser = CreateParser(parserclass, reader, writer);
            parser.CancellationToken = token;
            parser.Parse();
        }
        public static Parser Init(Type parserclass, TextReader reader, TextWriter writer)
        {
            var parser = CreateParser(parserclass, reader, writer);
            parser.CreateThreadForParse();
            return parser;
        }

        private Parser() { }
        private readonly List<(InputFunctionAttribute Attribute, MethodInfo Method)> Methods = new List<(InputFunctionAttribute, MethodInfo)>();
        private MethodInfo NullInputMethod {get; set; }
        private TextReader TextReader { get; set; }
        private TextWriter TextWriter { get; set; }
        private Thread ParseThread { get; set; }
        

        private CancellationToken CancellationToken { get; set; }

        private void CreateThreadForParse()
        {
            ParseThread = new Thread(e => Parse());
            ParseThread.Start();
        }

        private static bool IsAsyncMethod(MethodInfo method)
        {
            Type attType = typeof(AsyncStateMachineAttribute);

            // Obtain the custom attribute for the method. 
            // The value returned contains the StateMachineType property. 
            // Null is returned if the attribute isn't present for the method. 
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(attType);

            return (attrib != null);
        }

        private void Parse()
        {
            while (true)
            {
                var task = TextReader.ReadLineAsync();
                task.Wait(CancellationToken);
                if(CancellationToken.IsCancellationRequested) return;
                var str = task.Result;
                if (str == null)
                {
                    NullInputMethod.Invoke(null, new object[0]);
                    continue;
                }

                var strarr = str.Split(' ');
                var methods = Methods.Where(d => d.Attribute.Names.Contains(strarr[0], StringComparer.CurrentCultureIgnoreCase)).ToArray();
                if(methods.Length == 0) continue;

                var method = methods[0];

                var parametersinfo = method.Method.GetParameters();
                var parametersinfoLengthMin = 0;
                var parametersinfoLengthMax = parametersinfo.Length;
                foreach (var parameterInfo in parametersinfo)
                {
                    if (!parameterInfo.IsOptional)
                        parametersinfoLengthMin++;
                }
                var parametersNeed = strarr.Length - 1;
                if (!(parametersinfoLengthMax >= parametersNeed && parametersinfoLengthMin <= parametersNeed)) continue;

                List<object> parameters = new List<object>();
                var j = 1;
                for (int i = 0; i < parametersNeed; i++)
                {
                    var attr = parametersinfo[i].GetCustomAttribute<InputParameterAttribute>();
                    if (attr != null && attr.UseParseMethod)
                    {
                        var obj = attr.ParseMethodClassType.GetMethods().Single(p => p.IsStatic && p.Name == attr.ParseMethodName && p.GetParameters().Length == 1).Invoke(null, new object[]{ strarr[i + 1]});
                        parameters.Add(obj);
                    }
                    else
                    {
                        try
                        {
                            parameters.Add(Convert.ChangeType(strarr[j], parametersinfo[i].ParameterType));
                        }
                        catch (Exception e)
                        {/*
                            if (!parametersinfo[i].IsOptional) throw;
                            parameters.Add(Type.Missing);
                            continue;*/
                            throw;
                        }
                    }
                    j++;
                }
                if (parameters.Count < parametersinfoLengthMax)
                {
                    for (var i = 0; i < parametersinfoLengthMax-parameters.Count; i++)
                    {
                        parameters.Add(Type.Missing);
                    }
                }
                if (IsAsyncMethod(method.Method))
                {
                    throw new NotImplementedException();
                    var taskobj = methods[0].Method.Invoke(null, parameters.ToArray());
                    ((Task<object>)taskobj).ContinueWith(o => TextWriter.WriteLine(o.Result), CancellationToken);
                }
                else
                {
                    TextWriter.WriteLine(methods[0].Method.Invoke(null, parameters.ToArray()));
                }
            }
        }

    }
}
