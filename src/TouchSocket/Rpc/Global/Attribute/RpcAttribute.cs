//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TouchSocket.Core.Extensions;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// Rpc方法属性基类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class RpcAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RpcAttribute()
        {
            this.MethodFlags = MethodFlags.None;
            this.m_exceptions.Add(typeof(TimeoutException), "调用超时");
            this.m_exceptions.Add(typeof(RpcInvokeException), "Rpc调用异常");
            this.m_exceptions.Add(typeof(Exception), "其他异常");
        }

        /// <summary>
        /// 生成代码
        /// </summary>
        public CodeGeneratorFlag GeneratorFlag { get; protected set; } =
            CodeGeneratorFlag.Sync | CodeGeneratorFlag.Async | CodeGeneratorFlag.ExtensionSync | CodeGeneratorFlag.ExtensionAsync
            | CodeGeneratorFlag.IncludeInterface | CodeGeneratorFlag.IncludeInstance | CodeGeneratorFlag.IncludeExtension;

        /// <summary>
        /// 函数标识
        /// </summary>
        public MethodFlags MethodFlags { get; set; }

        /// <summary>
        /// 异常提示
        /// </summary>
        public Dictionary<Type, string> Exceptions => this.m_exceptions;

        /// <summary>
        /// 重新指定生成的函数名称。可以使用类似“JsonRpc_{0}”的模板格式。
        /// </summary>
        public string MethodName { get; set; }

        private readonly Dictionary<Type, string> m_exceptions = new Dictionary<Type, string>();

        /// <summary>
        /// 获取注释信息
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public virtual string GetDescription(MethodInstance methodInstance)
        {
            return string.IsNullOrEmpty(methodInstance.Description) ? "无注释信息" : methodInstance.Description;
        }

        /// <summary>
        /// 获取返回值
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <param name="classCodeGenerator"></param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        public virtual string GetReturn(MethodInstance methodInstance, ClassCodeGenerator classCodeGenerator, bool isAsync)
        {
            if (isAsync)
            {
                if (methodInstance.ReturnType == null)
                {
                    return "Task";
                }
                else
                {
                    return $"Task<{classCodeGenerator.GetTypeFullName(methodInstance.ReturnType)}>";
                }
            }
            else
            {
                if (methodInstance.ReturnType == null)
                {
                    return "void";
                }
                else
                {
                    return classCodeGenerator.GetTypeFullName(methodInstance.ReturnType);
                }
            }
        }

        /// <summary>
        /// 获取生成的函数名称
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        public virtual string GetMethodName(MethodInstance methodInstance, bool isAsync)
        {
            string name;
            if (string.IsNullOrEmpty(this.MethodName))
            {
                name = methodInstance.Name;
            }
            else
            {
                name = this.MethodName.Format(methodInstance.Name);
            }
            return isAsync ? name + "Async" : name;
        }

        /// <summary>
        /// 获取生成的函数泛型限定名称。默认<see cref="IRpcClient"/>
        /// </summary>
        /// <returns></returns>
        public virtual Type[] GetGenericInterfaceTypes()
        {
            return new Type[] { typeof(IRpcClient) };
        }

        /// <summary>
        /// 获取调用键
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public virtual string GetInvokenKey(MethodInstance methodInstance)
        {
            return $"{methodInstance.ServerType.FullName}.{methodInstance.Name}".ToLower();
        }

        /// <summary>
        /// 获取参数生成
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <param name="classCodeGenerator"></param>
        /// <param name="isOut"></param>
        /// <param name="isRef"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual List<string> GetParameters(MethodInstance methodInstance, ClassCodeGenerator classCodeGenerator, out bool isOut, out bool isRef, out ParameterInfo[] parameters)
        {
            List<string> list = new List<string>();
            isOut = false;
            isRef = false;

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
                StringBuilder codeString = new StringBuilder();
                if (parameters[i].ParameterType.Name.Contains("&"))
                {
                    if (parameters[i].IsOut)
                    {
                        isOut = true;
                        codeString.Append(string.Format("out {0} {1}", classCodeGenerator.GetTypeFullName(parameters[i].ParameterType), parameters[i].Name));
                    }
                    else
                    {
                        isRef = true;
                        codeString.Append(string.Format("ref {0} {1}", classCodeGenerator.GetTypeFullName(parameters[i].ParameterType), parameters[i].Name));
                    }
                }
                else
                {
                    codeString.Append(string.Format("{0} {1}", classCodeGenerator.GetTypeFullName(parameters[i].ParameterType), parameters[i].Name));
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
                    else if (defaultValue.GetType() == typeof(bool))
                    {
                        codeString.Append(string.Format("={0}", defaultValue.ToString().ToLower()));
                    }
                    else if (typeof(ValueType).IsAssignableFrom(defaultValue.GetType()))
                    {
                        codeString.Append(string.Format("={0}", defaultValue));
                    }
                }

                list.Add(codeString.ToString());
            }

            return list;
        }

        /// <summary>
        /// 获取调用配置
        /// </summary>
        /// <returns></returns>
        public virtual string GetInvokeOption()
        {
            return "IInvokeOption invokeOption = default";
        }

        /// <summary>
        /// 当使用TryCanInvoke不能调用时，执行的代码。
        /// </summary>
        /// <returns></returns>
        public virtual string GetCannotInvoke(MethodInstance methodInstance, ClassCodeGenerator classCodeGenerator)
        {
            return "throw new RpcException(\"Rpc无法执行。\");";
        }
    }
}