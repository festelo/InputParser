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
                var attribute = method.GetCustomAttribute<InputAttribute>();
                if (attribute != null) parser.Methods.Add((attribute, method));

                var defaultattriubte = method.GetCustomAttribute<DefaultInputAttribute>();
                if (defaultattriubte != null)
                {
                    var prms = method.GetParameters();
                    if (prms.Length == 1 && prms[0].ParameterType == typeof(string))
                        parser.isDefaultMethodWithParams = true;
                    parser.defaultMethod = method;
                }

                var nullattribute = method.GetCustomAttribute<ReadingEndedAttribute>();
                if (nullattribute != null)
                {
                    parser.readingEndedMethod = method;
                }
            }
            parser.textReader = reader;
            parser.textWriter = writer;
            return parser;
        }
        public static void InitFromThisThread(Type parserclass, TextReader reader, TextWriter writer, CancellationToken token = default(CancellationToken))
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
        private readonly List<(InputAttribute Attribute, MethodInfo Method)> Methods = new List<(InputAttribute, MethodInfo)>();

        private bool isDefaultMethodWithParams { get; set; }
        private MethodInfo readingEndedMethod { get; set; }
        private MethodInfo defaultMethod {get; set; }
        private TextReader textReader { get; set; }
        private TextWriter textWriter { get; set; }
        private Thread parseThread { get; set; }
        

        private CancellationToken CancellationToken { get; set; }

        private void CreateThreadForParse()
        {
            parseThread = new Thread(e => Parse());
            parseThread.Start();
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
                MethodInfo method;
                var parameters = new List<object>();
                string str;
                void SetDefaultMethod()
                {
                    method = defaultMethod;
                    if (isDefaultMethodWithParams)
                    {
                        parameters.Add(str);
                    }
                }

                //will be changed
                var task = textReader.ReadLineAsync();
                task.Wait(CancellationToken);
                if(CancellationToken.IsCancellationRequested) return;
                str = task.Result;
                if (str == null)
                {
                    method = readingEndedMethod;
                }
                else if (string.IsNullOrWhiteSpace(str))
                {
                    SetDefaultMethod();
                }
                else
                {
                    var strarr = str.Split(' ');
                    var methods = Methods.Where(d => d.Attribute.Names?.Contains(strarr[0], StringComparer.CurrentCultureIgnoreCase) ?? false).ToArray();
                    if (methods.Length == 0)
                    {
                        SetDefaultMethod();
                    }
                    else
                    {
                        method = methods[0].Method;

                        (ParameterInfo Parameter, bool Used)[] parametersinfo = method.GetParameters().Select(c => (c, false)).ToArray();
                        var parametersinfoLengthMin = 0;
                        var parametersinfoLengthMax = parametersinfo.Length;
                        foreach (var parameterInfo in parametersinfo)
                        {
                            if (!parameterInfo.Parameter.IsOptional)
                                parametersinfoLengthMin++;
                        }
                        var parametersNeed = strarr.Length - 1;
                        if (!(parametersinfoLengthMax >= parametersNeed && parametersinfoLengthMin <= parametersNeed)) continue;
                        var j = 1;
                        for (int i = 0; i < parametersNeed; i++)
                        {
                            if(parametersinfo[i].Used) continue;
                            try
                            {
                                var attr = parametersinfo[i].Parameter.GetCustomAttribute<ParsedParameterAttribute>();
                                if (attr != null)
                                {
                                    var obj = attr.Parse(strarr[i + 1]);
                                    parameters.Add(obj);
                                }
                                else
                                    parameters.Add(Convert.ChangeType(strarr[j], parametersinfo[i].Parameter.ParameterType));
                                parametersinfo[i].Used = true;
                            }
                            catch (Exception e)
                            {
                                if (!parametersinfo[i].Parameter.IsOptional) throw;
                                parameters.Add(Type.Missing);
                                parametersinfo[i].Used = true;
                            }
                            j++;
                        }
                        if (parameters.Count < parametersinfoLengthMax)
                        {
                            for (var i = 0; parameters.Count != parametersinfoLengthMax; i++)
                            {
                                parameters.Add(Type.Missing);
                            }
                        }
                    }
                }
                if (IsAsyncMethod(method))
                {
                    throw new NotImplementedException();
                    var taskobj = method.Invoke(null, parameters.ToArray());
                    ((Task<object>)taskobj).ContinueWith(o => textWriter.WriteLine(o.Result), CancellationToken);
                }
                else
                {
                    textWriter.WriteLine(method.Invoke(null, parameters.ToArray()));
                }
            }
        }

    }
}
