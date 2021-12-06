//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 代码生成器
    /// </summary>
    public class CodeGenerator
    {
        internal static List<Type> proxyType = new List<Type>();

        private StringBuilder codeString;

        internal CodeGenerator()
        {
            codeString = new StringBuilder();
        }

        internal static string Namespace { get; set; }

        internal static PropertyCodeGenerator PropertyCode { get; set; }

        internal string ClassName { get; set; }

        internal MethodInstance[] MethodInstances { get; set; }

        /// <summary>
        /// 添加代理类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="deepSearch"></param>
        public static void AddProxyType(Type type, bool deepSearch = true)
        {
            if (type.IsPrimitive || type == typeof(string))
            {
                return;
            }
            if (!proxyType.Contains(type))
            {
                proxyType.Add(type);
                if (deepSearch)
                {
                    PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
                    foreach (var item in properties)
                    {
                        AddProxyType(item.PropertyType);
                    }
                }
            }
        }

        internal static string GetAssemblyInfo(string assemblyName, string version)
        {
            CodeGenerator codeMap = new CodeGenerator();
            codeMap.AppendAssemblyInfo(assemblyName, version);
            return codeMap.codeString.ToString();
        }

        internal string GetCode()
        {
            codeString.AppendLine("using System;");
            codeString.AppendLine("using RRQMSocket.RPC;");
            codeString.AppendLine("using RRQMSocket.RPC.RRQMRPC;");
            codeString.AppendLine("using RRQMCore.Exceptions;");
            codeString.AppendLine("using System.Collections.Generic;");
            codeString.AppendLine("using System.Diagnostics;");
            codeString.AppendLine("using System.Text;");
            codeString.AppendLine("using System.Threading.Tasks;");
            codeString.AppendLine(string.Format("namespace {0}", Namespace));
            codeString.AppendLine("{");
            this.GetInterface("I" + this.ClassName);
            this.GetClass(this.ClassName);
            codeString.AppendLine("}");//空间结束

            return codeString.ToString();
        }

        internal string GetName(Type type)
        {
            return PropertyCode.GetTypeFullName(type);
        }

        private void AppendAssemblyInfo(string assemblyName, string version)
        {
            codeString.AppendLine("using System.Reflection;");
            codeString.AppendLine("using System.Runtime.CompilerServices;");
            codeString.AppendLine("using System.Runtime.InteropServices;");
            codeString.AppendLine("[assembly: AssemblyTitle(\"RRQMRPC\")]");
            codeString.AppendLine("[assembly: AssemblyProduct(\"RRQMRPC\")]");
            codeString.AppendLine("[assembly: AssemblyCopyright(\"Copyright © 2020 若汝棋茗\")]");
            codeString.AppendLine("[assembly: ComVisible(false)]");

            codeString.AppendLine(string.Format("[assembly: AssemblyVersion(\"{0}\")]", version));
            codeString.AppendLine(string.Format("[assembly: AssemblyFileVersion(\"{0}\")]", version.ToString()));
        }

        private void AppendInterfaceMethods()
        {
            if (MethodInstances != null)
            {
                foreach (MethodInstance methodInstance in MethodInstances)
                {
                    bool isOut = false;
                    bool isRef = false;
                    MethodInfo method = methodInstance.Method;
                    string methodName = method.GetCustomAttribute<RRQMRPCAttribute>().MemberKey == null ? method.Name : method.GetCustomAttribute<RRQMRPCAttribute>().MemberKey;

                    if (methodInstance.DescriptionAttribute!=null)
                    {
                        codeString.AppendLine("///<summary>");
                        codeString.AppendLine($"///{methodInstance.DescriptionAttribute.Description}");
                        codeString.AppendLine("///</summary>");
                    }
                   

                    if (method.ReturnType.FullName == "System.Void" || method.ReturnType.FullName == "System.Threading.Tasks.Task")
                    {
                        codeString.Append(string.Format("  void {0} ", methodName));
                    }
                    else
                    {
                        codeString.Append(string.Format(" {0} {1} ", this.GetName(method.ReturnType), methodName));
                    }
                    codeString.Append("(");//方法参数

                    ParameterInfo[] parameters;
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        List<ParameterInfo> infos = new List<ParameterInfo>(methodInstance.Parameters);
                        infos.RemoveAt(0);
                        parameters = infos.ToArray();
                    }
                    else
                    {
                        parameters = methodInstance.Parameters;
                    }

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i > 0)
                        {
                            codeString.Append(",");
                        }
                        if (parameters[i].ParameterType.Name.Contains("&"))
                        {
                            if (parameters[i].IsOut)
                            {
                                isOut = true;
                                codeString.Append(string.Format("out {0} {1}", this.GetName(parameters[i].ParameterType), parameters[i].Name));
                            }
                            else
                            {
                                isRef = true;
                                codeString.Append(string.Format("ref {0} {1}", this.GetName(parameters[i].ParameterType), parameters[i].Name));
                            }
                        }
                        else
                        {
                            codeString.Append(string.Format("{0} {1}", this.GetName(parameters[i].ParameterType), parameters[i].Name));
                        }

                        if (parameters[i].HasDefaultValue)
                        {
                            object defaultValue = parameters[i].DefaultValue;
                            if (defaultValue == null)
                            {
                                codeString.Append(string.Format("=null"));
                            }
                            else if (defaultValue.ToString() == string.Empty)
                            {
                                codeString.Append(string.Format("=\"\""));
                            }
                            else if (defaultValue.GetType() == typeof(string))
                            {
                                codeString.Append(string.Format("=\"{0}\"", defaultValue));
                            }
                            else if (typeof(ValueType).IsAssignableFrom(defaultValue.GetType()))
                            {
                                codeString.Append(string.Format("={0}", defaultValue));
                            }
                        }
                    }
                    if (parameters.Length > 0)
                    {
                        codeString.Append(",");
                    }
                    codeString.AppendLine("InvokeOption invokeOption = null);");

                    if (!isOut && !isRef)//没有out或者ref
                    {
                        
                        if (methodInstance.DescriptionAttribute!=null)
                        {
                            codeString.AppendLine("///<summary>");
                            codeString.AppendLine($"///{methodInstance.DescriptionAttribute.Description}");
                            codeString.AppendLine("///</summary>");
                        }
                        
                        if (method.ReturnType.FullName == "System.Void" || method.ReturnType.FullName == "System.Threading.Tasks.Task")
                        {
                            codeString.Append(string.Format("void {0} ", methodName + "Async"));
                        }
                        else
                        {
                            codeString.Append(string.Format("Task<{0}> {1} ", this.GetName(method.ReturnType), methodName + "Async"));
                        }

                        codeString.Append("(");//方法参数

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (i > 0)
                            {
                                codeString.Append(",");
                            }

                            codeString.Append(string.Format("{0} {1}", this.GetName(parameters[i].ParameterType), parameters[i].Name));
                            if (parameters[i].DefaultValue != System.DBNull.Value)
                            {
                                object defaultValue = parameters[i].DefaultValue;
                                if (defaultValue == null)
                                {
                                    codeString.Append(string.Format("=null"));
                                }
                                else if (defaultValue.ToString() == string.Empty)
                                {
                                    codeString.Append(string.Format("=\"\""));
                                }
                                else if (defaultValue.GetType() == typeof(string))
                                {
                                    codeString.Append(string.Format("=\"{0}\"", defaultValue));
                                }
                                else if (typeof(ValueType).IsAssignableFrom(defaultValue.GetType()))
                                {
                                    codeString.Append(string.Format("={0}", defaultValue));
                                }
                            }
                        }

                        if (parameters.Length > 0)
                        {
                            codeString.Append(",");
                        }
                        codeString.AppendLine("InvokeOption invokeOption = null);");
                    }
                }
            }
        }

        private void AppendMethods()
        {
            if (MethodInstances != null)
            {
                foreach (MethodInstance methodInstance in MethodInstances)
                {
                    bool isReturn;
                    bool isOut = false;
                    bool isRef = false;
                    MethodInfo method = methodInstance.Method;
                    string methodName = method.GetCustomAttribute<RRQMRPCAttribute>().MemberKey == null ? method.Name : method.GetCustomAttribute<RRQMRPCAttribute>().MemberKey;
                    codeString.AppendLine("///<summary>");
                    codeString.AppendLine("///<inheritdoc/>");
                    codeString.AppendLine("///</summary>");
                    if (method.ReturnType.FullName == "System.Void" || method.ReturnType.FullName == "System.Threading.Tasks.Task")
                    {
                        isReturn = false;
                        codeString.Append(string.Format("public  void {0} ", methodName));
                    }
                    else
                    {
                        isReturn = true;
                        codeString.Append(string.Format("public {0} {1} ", this.GetName(method.ReturnType), methodName));
                    }
                    codeString.Append("(");//方法参数

                    ParameterInfo[] parameters;
                    if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
                    {
                        List<ParameterInfo> infos = new List<ParameterInfo>(methodInstance.Parameters);
                        infos.RemoveAt(0);
                        parameters = infos.ToArray();
                    }
                    else
                    {
                        parameters = methodInstance.Parameters;
                    }

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i > 0)
                        {
                            codeString.Append(",");
                        }
                        if (parameters[i].ParameterType.Name.Contains("&"))
                        {
                            if (parameters[i].IsOut)
                            {
                                isOut = true;
                                codeString.Append(string.Format("out {0} {1}", this.GetName(parameters[i].ParameterType), parameters[i].Name));
                            }
                            else
                            {
                                isRef = true;
                                codeString.Append(string.Format("ref {0} {1}", this.GetName(parameters[i].ParameterType), parameters[i].Name));
                            }
                        }
                        else
                        {
                            codeString.Append(string.Format("{0} {1}", this.GetName(parameters[i].ParameterType), parameters[i].Name));
                        }

                        if (parameters[i].HasDefaultValue)
                        {
                            object defaultValue = parameters[i].DefaultValue;
                            if (defaultValue == null)
                            {
                                codeString.Append(string.Format("=null"));
                            }
                            else if (defaultValue.ToString() == string.Empty)
                            {
                                codeString.Append(string.Format("=\"\""));
                            }
                            else if (defaultValue.GetType() == typeof(string))
                            {
                                codeString.Append(string.Format("=\"{0}\"", defaultValue));
                            }
                            else if (typeof(ValueType).IsAssignableFrom(defaultValue.GetType()))
                            {
                                codeString.Append(string.Format("={0}", defaultValue));
                            }
                        }
                    }
                    if (parameters.Length > 0)
                    {
                        codeString.Append(",");
                    }
                    codeString.AppendLine("InvokeOption invokeOption = null)");

                    codeString.AppendLine("{");//方法开始

                    codeString.AppendLine("if(Client==null)");
                    codeString.AppendLine("{");
                    codeString.AppendLine("throw new RRQMRPCException(\"IRPCClient为空，请先初始化或者进行赋值\");");
                    codeString.AppendLine("}");

                    codeString.Append($"object[] parameters = new object[]");
                    codeString.Append("{");
                    foreach (ParameterInfo parameter in parameters)
                    {
                        if (parameter.ParameterType.Name.Contains("&") && parameter.IsOut)
                        {
                            codeString.Append($"default({this.GetName(parameter.ParameterType)})");
                        }
                        else
                        {
                            codeString.Append(parameter.Name);
                        }
                        if (parameter != parameters[parameters.Length - 1])
                        {
                            codeString.Append(",");
                        }
                    }
                    codeString.AppendLine("};");

                    if (isOut || isRef)
                    {
                        codeString.Append($"Type[] types = new Type[]");
                        codeString.Append("{");
                        foreach (ParameterInfo parameter in parameters)
                        {
                            codeString.Append($"typeof({this.GetName(parameter.ParameterType)})");
                            if (parameter != parameters[parameters.Length - 1])
                            {
                                codeString.Append(",");
                            }
                        }
                        codeString.AppendLine("};");
                    }

                    if (isReturn)
                    {
                        if (isOut || isRef)
                        {
                            codeString.Append(string.Format("{0} returnData=Client.Invoke<{0}>", this.GetName(method.ReturnType)));
                            codeString.Append("(");
                            codeString.Append(string.Format("\"{0}\"", methodName));
                            codeString.AppendLine(",invokeOption,ref parameters,types);");
                        }
                        else
                        {
                            codeString.Append(string.Format("{0} returnData=Client.Invoke<{0}>", this.GetName(method.ReturnType)));
                            codeString.Append("(");
                            codeString.Append(string.Format("\"{0}\"", methodName));
                            codeString.AppendLine(",invokeOption, parameters);");
                        }
                    }
                    else
                    {
                        if (isOut || isRef)
                        {
                            codeString.Append("Client.Invoke(");
                            codeString.Append(string.Format("\"{0}\"", methodName));
                            codeString.AppendLine(",invokeOption,ref parameters,types);");
                        }
                        else
                        {
                            codeString.Append("Client.Invoke(");
                            codeString.Append(string.Format("\"{0}\"", methodName));
                            codeString.AppendLine(",invokeOption, parameters);");
                        }
                    }
                    if (isOut || isRef)
                    {
                        codeString.AppendLine("if(parameters!=null)");
                        codeString.AppendLine("{");
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            codeString.AppendLine(string.Format("{0}=({1})parameters[{2}];", parameters[i].Name, this.GetName(parameters[i].ParameterType), i));
                        }
                        codeString.AppendLine("}");
                        if (isOut)
                        {
                            codeString.AppendLine("else");
                            codeString.AppendLine("{");
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                if (parameters[i].IsOut)
                                {
                                    codeString.AppendLine(string.Format("{0}=default({1});", parameters[i].Name, this.GetName(parameters[i].ParameterType)));
                                }
                            }
                            codeString.AppendLine("}");
                        }
                    }

                    if (isReturn)
                    {
                        codeString.AppendLine("return returnData;");
                    }

                    codeString.AppendLine("}");

                    //以下生成异步
                    if (!isOut && !isRef)//没有out或者ref
                    {
                        codeString.AppendLine("///<summary>");
                        codeString.AppendLine("///<inheritdoc/>");
                        codeString.AppendLine("///</summary>");
                        if (method.ReturnType.FullName == "System.Void" || method.ReturnType.FullName == "System.Threading.Tasks.Task")
                        {
                            isReturn = false;
                            codeString.Append(string.Format("public  async void {0} ", methodName + "Async"));
                        }
                        else
                        {
                            isReturn = true;
                            codeString.Append(string.Format("public  async Task<{0}> {1} ", this.GetName(method.ReturnType), methodName + "Async"));
                        }

                        codeString.Append("(");//方法参数

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (i > 0)
                            {
                                codeString.Append(",");
                            }

                            codeString.Append(string.Format("{0} {1}", this.GetName(parameters[i].ParameterType), parameters[i].Name));
                            if (parameters[i].DefaultValue != System.DBNull.Value)
                            {
                                object defaultValue = parameters[i].DefaultValue;
                                if (defaultValue == null)
                                {
                                    codeString.Append(string.Format("=null"));
                                }
                                else if (defaultValue.ToString() == string.Empty)
                                {
                                    codeString.Append(string.Format("=\"\""));
                                }
                                else if (defaultValue.GetType() == typeof(string))
                                {
                                    codeString.Append(string.Format("=\"{0}\"", defaultValue));
                                }
                                else if (typeof(ValueType).IsAssignableFrom(defaultValue.GetType()))
                                {
                                    codeString.Append(string.Format("={0}", defaultValue));
                                }
                            }
                        }

                        if (parameters.Length > 0)
                        {
                            codeString.Append(",");
                        }
                        codeString.AppendLine("InvokeOption invokeOption = null)");
                        codeString.AppendLine("{");//方法开始
                        codeString.AppendLine("if(Client==null)");
                        codeString.AppendLine("{");
                        codeString.AppendLine("throw new RRQMRPCException(\"RPCClient为空，请先初始化或者进行赋值\");");
                        codeString.AppendLine("}");
                        if (isReturn)
                        {
                            codeString.AppendLine("return await Task.Run(() =>{");
                            codeString.Append(string.Format("return {0}(", methodName));
                        }
                        else
                        {
                            codeString.AppendLine("await Task.Run(() =>{");
                            codeString.Append(string.Format("{0}(", methodName));
                        }

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (i > 0)
                            {
                                codeString.Append(",");
                            }

                            codeString.Append(string.Format("{0}", parameters[i].Name));
                        }
                        if (parameters.Length > 0)
                        {
                            codeString.Append(",");
                        }
                        codeString.Append("invokeOption);");
                        codeString.AppendLine("});");
                        codeString.AppendLine("}");
                    }
                }
            }
        }

        private void AppendProperties()
        {
            codeString.AppendLine("public IRpcClient Client{get;private set; }");
        }

        private void GetClass(string className)
        {
            codeString.AppendLine(string.Format("public class {0} :I{0}", className));//类开始
            codeString.AppendLine("{");
            codeString.AppendLine($"public {className}(IRpcClient client)");
            codeString.AppendLine("{");
            codeString.AppendLine("this.Client=client;");
            codeString.AppendLine("}");
            AppendProperties();
            AppendMethods();
            codeString.AppendLine("}");//类结束
        }

        private void GetInterface(string interfaceName)
        {
            codeString.AppendLine(string.Format("public interface {0}:IRemoteServer", interfaceName));//类开始
            codeString.AppendLine("{");
            AppendInterfaceMethods();
            codeString.AppendLine("}");//类结束
        }
    }
}