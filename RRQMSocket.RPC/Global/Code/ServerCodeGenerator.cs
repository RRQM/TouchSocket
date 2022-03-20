//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 服务代码生成
    /// </summary>
    public class ServerCodeGenerator
    {
        private ClassCodeGenerator classCodeGenerator;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="classCodeGenerator"></param>
        public ServerCodeGenerator(ClassCodeGenerator classCodeGenerator)
        {
            this.classCodeGenerator = classCodeGenerator;
        }

        /// <summary>
        /// 类代码生成器
        /// </summary>
        public ClassCodeGenerator ClassCodeGenerator => this.classCodeGenerator;

        /// <summary>
        /// 从类型获取代理名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetName(Type type)
        {
            return this.classCodeGenerator.GetTypeFullName(type);
        }

        /// <summary>
        /// 获取接口代码
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public string GetInterfaceProxy<T>(MethodInstance methodInstance) where T : RpcAttribute
        {
            bool isOut = false;
            bool isRef = false;
            StringBuilder codeString = new StringBuilder();
            string description;
            if (methodInstance.DescriptionAttribute != null)
            {
                description = methodInstance.DescriptionAttribute.Description;
            }
            else
            {
                description = "无注释信息";
            }
            codeString.AppendLine("///<summary>");
            codeString.AppendLine($"///{description}");
            codeString.AppendLine("///</summary>");
            codeString.AppendLine("/// <exception cref=\"TimeoutException\">调用超时</exception>");
            codeString.AppendLine("/// <exception cref=\"RpcSerializationException\">序列化异常</exception>");
            codeString.AppendLine("/// <exception cref=\"RRQMRpcInvokeException\">Rpc异常</exception>");
            codeString.AppendLine("/// <exception cref=\"RRQMException\">其他异常</exception>");
            if (methodInstance.ReturnType == null)
            {
                codeString.Append(" void {0} ");
            }
            else
            {
                codeString.Append(this.GetName(methodInstance.ReturnType));
                codeString.Append(" {0} ");
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
            codeString.AppendLine("IInvokeOption invokeOption = default);");

            if (!isOut && !isRef)//没有out或者ref
            {
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{description}");
                codeString.AppendLine("///</summary>");
                codeString.AppendLine("/// <exception cref=\"TimeoutException\">调用超时</exception>");
                codeString.AppendLine("/// <exception cref=\"RpcSerializationException\">序列化异常</exception>");
                codeString.AppendLine("/// <exception cref=\"RRQMRpcInvokeException\">Rpc异常</exception>");
                codeString.AppendLine("/// <exception cref=\"RRQMException\">其他异常</exception>");

                if (methodInstance.ReturnType == null)
                {
                    codeString.Append("Task {0}Async ");
                }
                else
                {
                    codeString.Append($"Task<{this.GetName(methodInstance.ReturnType)}> ");
                    codeString.Append("{0}Async ");
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
                codeString.AppendLine("IInvokeOption invokeOption = default);");
            }

            return codeString.ToString();
        }

        /// <summary>
        /// 获取函数代码
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public string GetMethodProxy<T>(MethodInstance methodInstance) where T : RpcAttribute
        {
            bool isReturn;
            bool isOut = false;
            bool isRef = false;
            StringBuilder codeString = new StringBuilder();
            codeString.AppendLine("///<summary>");
            codeString.AppendLine("///<inheritdoc/>");
            codeString.AppendLine("///</summary>");
            codeString.AppendLine("/// <exception cref=\"TimeoutException\">调用超时</exception>");
            codeString.AppendLine("/// <exception cref=\"RpcSerializationException\">序列化异常</exception>");
            codeString.AppendLine("/// <exception cref=\"RRQMRpcInvokeException\">Rpc异常</exception>");
            codeString.AppendLine("/// <exception cref=\"RRQMException\">其他异常</exception>");

            if (methodInstance.ReturnType == null)
            {
                isReturn = false;
                codeString.Append("public  void {0} ");
            }
            else
            {
                isReturn = true;
                codeString.Append($"public {this.GetName(methodInstance.ReturnType)}");
                codeString.Append(" {0} ");
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
            codeString.AppendLine("IInvokeOption invokeOption = default)");

            codeString.AppendLine("{");//方法开始

            codeString.AppendLine("if(Client==null)");
            codeString.AppendLine("{");
            codeString.AppendLine("throw new RpcException(\"IRpcClient为空，请先初始化或者进行赋值\");");
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
                    codeString.Append(string.Format("{0} returnData=Client.Invoke<{0}>", this.GetName(methodInstance.ReturnType)));
                    codeString.Append("(");
                    codeString.Append("\"{1}\"");
                    codeString.AppendLine(",invokeOption,ref parameters,types);");
                }
                else
                {
                    codeString.Append(string.Format("{0} returnData=Client.Invoke<{0}>", this.GetName(methodInstance.ReturnType)));
                    codeString.Append("(");
                    codeString.Append("\"{1}\"");
                    codeString.AppendLine(",invokeOption, parameters);");
                }
            }
            else
            {
                if (isOut || isRef)
                {
                    codeString.Append("Client.Invoke(");
                    codeString.Append("\"{1}\"");
                    codeString.AppendLine(",invokeOption,ref parameters,types);");
                }
                else
                {
                    codeString.Append("Client.Invoke(");
                    codeString.Append("\"{1}\"");
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
                codeString.AppendLine("/// <exception cref=\"TimeoutException\">调用超时</exception>");
                codeString.AppendLine("/// <exception cref=\"RpcSerializationException\">序列化异常</exception>");
                codeString.AppendLine("/// <exception cref=\"RRQMRpcInvokeException\">Rpc异常</exception>");
                codeString.AppendLine("/// <exception cref=\"RRQMException\">其他异常</exception>");
                if (methodInstance.ReturnType == null)
                {
                    isReturn = false;
                    codeString.Append("public Task {0}Async ");
                }
                else
                {
                    isReturn = true;
                    codeString.Append($"public Task<{this.GetName(methodInstance.ReturnType)}> ");
                    codeString.Append("{0}Async ");
                }
                codeString.Append("(");//方法参数

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(",");
                    }
                    codeString.Append(string.Format("{0} {1}", this.GetName(parameters[i].ParameterType), parameters[i].Name));

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
                codeString.AppendLine("IInvokeOption invokeOption = default)");

                codeString.AppendLine("{");//方法开始

                codeString.AppendLine("if(Client==null)");
                codeString.AppendLine("{");
                codeString.AppendLine("throw new RpcException(\"IRpcClient为空，请先初始化或者进行赋值\");");
                codeString.AppendLine("}");

                codeString.Append($"object[] parameters = new object[]");
                codeString.Append("{");
                foreach (ParameterInfo parameter in parameters)
                {
                    codeString.Append(parameter.Name);
                    if (parameter != parameters[parameters.Length - 1])
                    {
                        codeString.Append(",");
                    }
                }
                codeString.AppendLine("};");

                if (isReturn)
                {
                    codeString.Append(string.Format("return Client.InvokeAsync<{0}>", this.GetName(methodInstance.ReturnType)));
                    codeString.Append("(");
                    codeString.Append("\"{1}\"");
                    codeString.AppendLine(",invokeOption, parameters);");
                }
                else
                {
                    codeString.Append("return Client.InvokeAsync(");
                    codeString.Append("\"{1}\"");
                    codeString.AppendLine(",invokeOption, parameters);");
                }
                codeString.AppendLine("}");
            }
            return codeString.ToString();
        }
    }
}