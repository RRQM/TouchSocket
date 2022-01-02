using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        public ClassCodeGenerator ClassCodeGenerator
        {
            get { return classCodeGenerator; }
        }

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
        public string GetInterfaceProxy<T>(MethodInstance methodInstance) where T : RPCAttribute
        {
            bool isOut = false;
            bool isRef = false;
            string methodName = CodeGenerator.GetMethodName<T>(methodInstance);
            StringBuilder codeString = new StringBuilder();
            if (methodInstance.DescriptionAttribute != null)
            {
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{methodInstance.DescriptionAttribute.Description}");
                codeString.AppendLine("///</summary>");
            }

            if (methodInstance.ReturnType==null)
            {
                codeString.Append(string.Format("  void {0} ", methodName));
            }
            else
            {
                codeString.Append(string.Format(" {0} {1} ", this.GetName(methodInstance.ReturnType), methodName));
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
                if (methodInstance.DescriptionAttribute != null)
                {
                    codeString.AppendLine("///<summary>");
                    codeString.AppendLine($"///{methodInstance.DescriptionAttribute.Description}");
                    codeString.AppendLine("///</summary>");
                }

                if (methodInstance.ReturnType==null)
                {
                    codeString.Append(string.Format("void {0} ", methodName + "Async"));
                }
                else
                {
                    codeString.Append(string.Format("Task<{0}> {1} ", this.GetName(methodInstance.ReturnType), methodName + "Async"));
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

            return codeString.ToString();
        }

        /// <summary>
        /// 获取函数代码
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public string GetMethodProxy<T>(MethodInstance methodInstance) where T : RPCAttribute
        {
            bool isReturn;
            bool isOut = false;
            bool isRef = false;
            string methodName = CodeGenerator.GetMethodName<T>(methodInstance);
            StringBuilder codeString = new StringBuilder();
            codeString.AppendLine("///<summary>");
            codeString.AppendLine("///<inheritdoc/>");
            codeString.AppendLine("///</summary>");
            if (methodInstance.ReturnType==null)
            {
                isReturn = false;
                codeString.Append(string.Format("public  void {0} ", methodName));
            }
            else
            {
                isReturn = true;
                codeString.Append(string.Format("public {0} {1} ", this.GetName(methodInstance.ReturnType), methodName));
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
                    codeString.Append(string.Format("{0} returnData=Client.Invoke<{0}>", this.GetName(methodInstance.ReturnType)));
                    codeString.Append("(");
                    codeString.Append(string.Format("\"{0}\"", methodName));
                    codeString.AppendLine(",invokeOption,ref parameters,types);");
                }
                else
                {
                    codeString.Append(string.Format("{0} returnData=Client.Invoke<{0}>", this.GetName(methodInstance.ReturnType)));
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
                if (methodInstance.ReturnType==null)
                {
                    isReturn = false;
                    codeString.Append(string.Format("public  async void {0} ", methodName + "Async"));
                }
                else
                {
                    isReturn = true;
                    codeString.Append(string.Format("public  async Task<{0}> {1} ", this.GetName(methodInstance.ReturnType), methodName + "Async"));
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
            return codeString.ToString();
        }
    }
}
