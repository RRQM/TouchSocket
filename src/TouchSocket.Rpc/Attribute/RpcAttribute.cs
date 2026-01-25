//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TouchSocket.Rpc;

/// <summary>
/// Rpc方法属性基类
/// </summary>
public abstract class RpcAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public RpcAttribute()
    {
        this.Exceptions.Add(typeof(TimeoutException), "调用超时");
        this.Exceptions.Add(typeof(RpcInvokeException), "Rpc调用异常");
        this.Exceptions.Add(typeof(Exception), "其他异常");
    }

    /// <summary>
    /// 类生成器
    /// </summary>
    public ClassCodeGenerator ClassCodeGenerator { get; private set; }

    /// <summary>
    /// 异常提示
    /// </summary>
    public Dictionary<Type, string> Exceptions { get; } = new Dictionary<Type, string>();

    /// <summary>
    /// 生成代码
    /// </summary>
    public CodeGeneratorFlag GeneratorFlag { get; set; } =
        CodeGeneratorFlag.InstanceAsync | CodeGeneratorFlag.ExtensionAsync
         | CodeGeneratorFlag.InterfaceAsync;

    /// <summary>
    /// 生成泛型方法的约束
    /// </summary>
    public Type[] GenericConstraintTypes { get; set; } = new Type[] { typeof(IRpcClient) };

    /// <summary>
    /// 调用键。
    /// </summary>
    public string InvokeKey { get; set; }

    /// <summary>
    /// 是否仅以函数名调用，当为<see langword="true"/>是，调用时仅需要传入方法名即可。
    /// </summary>
    public bool MethodInvoke { get; set; }

    /// <summary>
    /// 重新指定生成的函数名称。可以使用类似“JsonRpc_{0}”的模板格式。
    /// </summary>
    public string MethodName { get; set; }

    /// <summary>
    /// 生成代理时，额外的命名空间
    /// </summary>
    public List<string> Namespaces { get; } = new List<string>();

    /// <summary>
    /// 获取或设置属性名称的字典。
    /// </summary>
    public Dictionary<string, object> PropertyNames { get; private set; }

    /// <summary>
    /// 获取注释信息
    /// </summary>
    /// <param name="rpcMethod"></param>
    /// <returns></returns>
    public virtual string GetDescription(RpcMethod rpcMethod)
    {
        var description = rpcMethod.GetDescription();
        return description.HasValue() ? this.ReplacePatterns(description) : "无注释信息";
    }

    /// <summary>
    /// 获取扩展的代理代码
    /// </summary>
    /// <param name="rpcMethod"></param>
    /// <returns></returns>
    public virtual string GetExtensionsMethodProxyCode(RpcMethod rpcMethod)
    {
        var codeString = new StringBuilder();

        var description = this.GetDescription(rpcMethod);

        var parametersStr = this.GetParameters(rpcMethod, out var parameters);
        var InterfaceTypes = this.GetGenericConstraintTypes();


        //以下生成异步
        if (this.GeneratorFlag.HasFlag(CodeGeneratorFlag.ExtensionAsync))
        {
            codeString.AppendLine("///<summary>");
            codeString.AppendLine($"///{description}");
            codeString.AppendLine("///</summary>");
            if (rpcMethod.HasReturn)
            {
                codeString.Append("public static async ");
            }
            else
            {
                codeString.Append("public static ");
            }
            codeString.Append(this.GetReturn(rpcMethod, true));
            codeString.Append(' ');
            codeString.Append(this.GetMethodName(rpcMethod, true));
            codeString.Append("<TClient>(");//方法参数

            codeString.Append($"this TClient client");

            codeString.Append(',');
            for (var i = 0; i < parametersStr.Count; i++)
            {
                if (i > 0)
                {
                    codeString.Append(',');
                }
                codeString.Append(parametersStr[i]);
            }
            if (parametersStr.Count > 0)
            {
                codeString.Append(',');
            }
            codeString.Append(this.GetInvokeOption());
            codeString.AppendLine(") where TClient:");

            for (var i = 0; i < InterfaceTypes.Length; i++)
            {
                if (i > 0)
                {
                    codeString.Append(',');
                }

                codeString.Append(InterfaceTypes[i].FullName);
            }

            codeString.AppendLine("{");//方法开始

            codeString.AppendLine(this.GetExtensionInstanceMethod(rpcMethod, parametersStr, parameters, true));

            codeString.AppendLine("}");
        }
        return codeString.ToString();
    }

    /// <summary>
    /// 获取生成的函数泛型限定名称。默认<see cref="IRpcClient"/>
    /// </summary>
    /// <returns></returns>
    public virtual Type[] GetGenericConstraintTypes()
    {
        return this.GenericConstraintTypes;
    }

    /// <summary>
    /// 获取生成实体类时的代码块
    /// </summary>
    /// <param name="rpcMethod"></param>
    /// <returns></returns>
    public virtual string GetInstanceProxyCode(RpcMethod rpcMethod)
    {
        var codeString = new StringBuilder();

        var description = this.GetDescription(rpcMethod);
        var parametersStr = this.GetParameters(rpcMethod, out var parameters);

        //以下生成异步
        if (this.GeneratorFlag.HasFlag(CodeGeneratorFlag.InstanceAsync))
        {
            codeString.AppendLine("///<summary>");
            codeString.AppendLine($"///{description}");
            codeString.AppendLine("///</summary>");
            if (rpcMethod.HasReturn)
            {
                codeString.Append("public async ");
            }
            else
            {
                codeString.Append("public ");
            }
            codeString.Append(this.GetReturn(rpcMethod, true));
            codeString.Append(' ');
            codeString.Append(this.GetMethodName(rpcMethod, true));
            codeString.Append('(');//方法参数

            for (var i = 0; i < parametersStr.Count; i++)
            {
                if (i > 0)
                {
                    codeString.Append(',');
                }
                codeString.Append(parametersStr[i]);
            }
            if (parametersStr.Count > 0)
            {
                codeString.Append(',');
            }
            codeString.Append(this.GetInvokeOption());
            codeString.AppendLine(")");

            codeString.AppendLine("{");//方法开始

            codeString.AppendLine(this.GetInstanceMethod(rpcMethod, parametersStr, parameters, true));

            codeString.AppendLine("}");
        }

        return codeString.ToString();
    }

    /// <summary>
    /// 获取接口的代理代码
    /// </summary>
    /// <param name="rpcMethod"></param>
    /// <returns></returns>
    public virtual string GetInterfaceProxyCode(RpcMethod rpcMethod)
    {
        var codeString = new StringBuilder();
        var description = this.GetDescription(rpcMethod);
        var parameters = this.GetParameters(rpcMethod, out _);

        if (this.GeneratorFlag.HasFlag(CodeGeneratorFlag.InterfaceAsync))
        {
            codeString.AppendLine("///<summary>");
            codeString.AppendLine($"///{description}");
            codeString.AppendLine("///</summary>");
            foreach (var item in this.Exceptions)
            {
                codeString.AppendLine($"/// <exception cref=\"{item.Key.FullName}\">{item.Value}</exception>");
            }

            codeString.Append(this.GetReturn(rpcMethod, true));
            codeString.Append(' ');
            codeString.Append(this.GetMethodName(rpcMethod, true));
            codeString.Append('(');//方法参数

            for (var i = 0; i < parameters.Count; i++)
            {
                if (i > 0)
                {
                    codeString.Append(',');
                }
                codeString.Append(parameters[i]);
            }
            if (parameters.Count > 0)
            {
                codeString.Append(',');
            }
            codeString.Append(this.GetInvokeOption());
            codeString.AppendLine(");");
        }

        return codeString.ToString();
    }

    /// <summary>
    /// 获取调用键
    /// </summary>
    /// <param name="rpcMethod"></param>
    /// <returns></returns>
    public virtual string GetInvokeKey(RpcMethod rpcMethod)
    {
        return this.MethodInvoke
            ? this.GetMethodName(rpcMethod, false)
            : !this.InvokeKey.IsNullOrEmpty() ? this.InvokeKey : $"{rpcMethod.ServerFromType.FullName}.{rpcMethod.Name}".ToLower();
    }

    /// <summary>
    /// 获取调用配置
    /// </summary>
    /// <returns></returns>
    public virtual string GetInvokeOption()
    {
        return "InvokeOption invokeOption = default";
    }

    /// <summary>
    /// 获取生成的函数名称
    /// </summary>
    /// <param name="rpcMethod"></param>
    /// <param name="isAsync"></param>
    /// <returns></returns>
    public virtual string GetMethodName(RpcMethod rpcMethod, bool isAsync)
    {
        var name = this.MethodName;
        name = name.HasValue() ? this.ReplacePatterns(name).Format(rpcMethod.Name) : rpcMethod.Name;

        if (isAsync)
        {
            return name.EndsWith("Async") ? name : $"{name}Async";
        }
        else
        {
            return name.EndsWith("Async") ? name.RemoveLastChars(5) : name;
        }
    }

    /// <summary>
    /// 根据指定的RPC方法获取参数信息。
    /// </summary>
    /// <param name="rpcMethod">RPC方法的枚举值，用于指定需要获取参数信息的RPC方法。</param>
    /// <param name="parameters">输出参数，包含RPC方法所有参数的信息。</param>
    /// <returns>返回一个字符串列表，包含RPC方法的参数。</returns>
    public virtual List<string> GetParameters(RpcMethod rpcMethod, out RpcParameter[] parameters)
    {
        var list = new List<string>();

        parameters = rpcMethod.GetNormalParameters().ToArray();

        for (var i = 0; i < parameters.Length; i++)
        {
            var codeString = new StringBuilder();
            codeString.Append(string.Format("{0} {1}", this.GetProxyParameterName(parameters[i].ParameterInfo), parameters[i].Name));

            if (parameters[i].ParameterInfo.HasDefaultValue)
            {
                var defaultValue = parameters[i].ParameterInfo.DefaultValue;
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
    /// 从类型获取代理名
    /// </summary>
    /// <param name="parameterInfo">参数信息对象，用于提取类型信息</param>
    /// <returns>返回根据类型生成的代理名</returns>
    public virtual string GetProxyParameterName(ParameterInfo parameterInfo)
    {
        // 使用ClassCodeGenerator获取参数类型的完整名称作为代理名
        return this.ClassCodeGenerator.GetTypeFullName(parameterInfo);
    }

    /// <summary>
    /// 获取返回值
    /// </summary>
    /// <param name="rpcMethod">远程过程调用方法的信息</param>
    /// <param name="isAsync">是否为异步调用</param>
    /// <returns>返回值类型字符串</returns>
    public virtual string GetReturn(RpcMethod rpcMethod, bool isAsync)
    {
        // 当是异步调用时，返回Task类型或Task<T>类型
        if (isAsync)
        {
            // 如果返回类型为空，则默认为Task；否则，构造Task<T>类型
            return rpcMethod.RealReturnType == null ? "Task" : $"Task<{this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)}>";
        }
        else
        {
            // 当非异步调用时，返回void或方法的返回参数名
            return rpcMethod.RealReturnType == null ? "void" : this.GetProxyParameterName(rpcMethod.Info.ReturnParameter);
        }
    }

    internal void SetClassCodeGenerator(ClassCodeGenerator classCodeGenerator)
    {
        this.ClassCodeGenerator = classCodeGenerator;
        this.LoadPublicPropertiesAsDictionary();
    }

    /// <summary>
    /// 生成扩展函数的内容
    /// </summary>
    /// <param name="rpcMethod"></param>
    /// <param name="parametersStr"></param>
    /// <param name="parameters"></param>
    /// <param name="isAsync"></param>
    /// <returns></returns>
    protected virtual string GetExtensionInstanceMethod(RpcMethod rpcMethod, List<string> parametersStr, RpcParameter[] parameters, bool isAsync)
    {
        var codeString = new StringBuilder();
        var returnTypeString = rpcMethod.HasReturn ? string.Format("typeof({0})", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)) : "null";

        if (isAsync)
        {
            if (parametersStr.Count > 0)
            {
                codeString.Append($"object[] parameters = new object[]");
                codeString.Append('{');

                foreach (var parameter in parameters)
                {
                    codeString.Append(parameter.Name);
                    if (parameter != parameters[^1])
                    {
                        codeString.Append(',');
                    }
                }
                codeString.AppendLine("};");

                if (rpcMethod.HasReturn)
                {
                    codeString.Append(string.Format("return ({0}) await client.InvokeAsync", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)));
                }
                else
                {
                    codeString.Append(string.Format("return client.InvokeAsync"));
                }
                codeString.Append('(');
                codeString.Append($"\"{this.GetInvokeKey(rpcMethod)}\",");
                codeString.Append($"{returnTypeString},");
                codeString.AppendLine("invokeOption, parameters);");
            }
            else
            {
                if (rpcMethod.HasReturn)
                {
                    codeString.Append(string.Format("return ({0}) await client.InvokeAsync", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)));
                }
                else
                {
                    codeString.Append(string.Format("return client.InvokeAsync"));
                }

                codeString.Append('(');
                codeString.Append($"\"{this.GetInvokeKey(rpcMethod)}\",");
                codeString.Append($"{returnTypeString},");

                codeString.AppendLine("invokeOption, null);");
            }
        }
        else
        {
            if (parametersStr.Count > 0)
            {
                codeString.Append($"object[] @_parameters = new object[]");
                codeString.Append('{');

                foreach (var parameter in parameters)
                {
                    codeString.Append(parameter.Name);
                    if (parameter != parameters[^1])
                    {
                        codeString.Append(',');
                    }
                }
                codeString.AppendLine("};");
                if (rpcMethod.HasReturn)
                {
                    codeString.Append(string.Format("{0} returnData=({0})client.Invoke", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)));
                }
                else
                {
                    codeString.Append(string.Format("client.Invoke"));
                }
                codeString.Append('(');
                codeString.Append($"\"{this.GetInvokeKey(rpcMethod)}\",");
                codeString.Append($"{returnTypeString},");
                codeString.AppendLine("invokeOption, @_parameters);");
            }
            else
            {
                if (rpcMethod.HasReturn)
                {
                    codeString.Append(string.Format("{0} returnData=({0})client.Invoke", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)));
                }
                else
                {
                    codeString.Append(string.Format("client.Invoke"));
                }
                codeString.Append('(');
                codeString.Append($"\"{this.GetInvokeKey(rpcMethod)}\",");
                codeString.Append($"{returnTypeString},");
                codeString.AppendLine("invokeOption, null);");
            }

            if (rpcMethod.HasReturn)
            {
                codeString.AppendLine("return returnData;");
            }
        }

        return codeString.ToString();
    }

    /// <summary>
    /// 生成实现函数的内容
    /// </summary>
    /// <param name="rpcMethod"></param>
    /// <param name="parametersStr"></param>
    /// <param name="parameters"></param>
    /// <param name="isAsync"></param>
    /// <returns></returns>
    protected virtual string GetInstanceMethod(RpcMethod rpcMethod, List<string> parametersStr, RpcParameter[] parameters, bool isAsync)
    {
        var codeString = new StringBuilder();
        codeString.AppendLine("if(this.Client==null)");
        codeString.AppendLine("{");
        codeString.AppendLine("throw new RpcException(\"IRpcClient为空，请先初始化或者进行赋值\");");
        codeString.AppendLine("}");

        var returnTypeString = rpcMethod.HasReturn ? string.Format("typeof({0})", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)) : "null";

        if (isAsync)
        {
            if (parametersStr.Count > 0)
            {
                codeString.Append($"object[] parameters = new object[]");
                codeString.Append('{');

                foreach (var parameter in parameters)
                {
                    codeString.Append(parameter.Name);
                    if (parameter != parameters[^1])
                    {
                        codeString.Append(',');
                    }
                }
                codeString.AppendLine("};");

                if (rpcMethod.HasReturn)
                {
                    codeString.Append(string.Format("return ({0}) await this.Client.InvokeAsync", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)));
                }
                else
                {
                    codeString.Append(string.Format("return this.Client.InvokeAsync"));
                }
                codeString.Append('(');
                codeString.Append($"\"{this.GetInvokeKey(rpcMethod)}\",");
                codeString.Append($"{returnTypeString},");
                codeString.AppendLine("invokeOption, parameters);");
            }
            else
            {
                if (rpcMethod.HasReturn)
                {
                    codeString.Append(string.Format("return ({0}) await this.Client.InvokeAsync", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)));
                }
                else
                {
                    codeString.Append(string.Format("return this.Client.InvokeAsync"));
                }

                codeString.Append('(');
                codeString.Append($"\"{this.GetInvokeKey(rpcMethod)}\",");
                codeString.Append($"{returnTypeString},");

                codeString.AppendLine("invokeOption, null);");
            }
        }
        else
        {
            if (parametersStr.Count > 0)
            {
                codeString.Append($"object[] @_parameters = new object[]");
                codeString.Append('{');

                foreach (var parameter in parameters)
                {
                    codeString.Append(parameter.Name);
                    if (parameter != parameters[^1])
                    {
                        codeString.Append(',');
                    }
                }
                codeString.AppendLine("};");
                if (rpcMethod.HasReturn)
                {
                    codeString.Append(string.Format("{0} returnData=({0})this.Client.Invoke", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)));
                }
                else
                {
                    codeString.Append(string.Format("this.Client.Invoke"));
                }
                codeString.Append('(');
                codeString.Append($"\"{this.GetInvokeKey(rpcMethod)}\",");
                codeString.Append($"{returnTypeString},");
                codeString.AppendLine("invokeOption, @_parameters);");
            }
            else
            {
                if (rpcMethod.HasReturn)
                {
                    codeString.Append(string.Format("{0} returnData=({0})this.Client.Invoke", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)));
                }
                else
                {
                    codeString.Append(string.Format("this.Client.Invoke"));
                }
                codeString.Append('(');
                codeString.Append($"\"{this.GetInvokeKey(rpcMethod)}\",");
                codeString.Append($"{returnTypeString},");
                codeString.AppendLine("invokeOption, null);");
            }

            if (rpcMethod.HasReturn)
            {
                codeString.AppendLine("return returnData;");
            }
        }

        return codeString.ToString();
    }

    protected abstract PropertyInfo[] GetPublicProperties();


    private void LoadPublicPropertiesAsDictionary()
    {
        if (this.PropertyNames is not null)
        {
            return;
        }
        this.PropertyNames = new Dictionary<string, object>() ;
        var publicProperties = this.GetPublicProperties();

        // 遍历这些属性并将它们的值存储到字典中
        foreach (var propertyInfo in publicProperties)
        {
            if (propertyInfo.CanRead)
            {
                var value = propertyInfo.GetValue(this);
                this.PropertyNames.TryAdd(propertyInfo.Name, value);
            }
        }
    }

    private string ReplacePatterns(string input)
    {
        this.LoadPublicPropertiesAsDictionary();
        var pairs = this.PropertyNames;

        var sb = new StringBuilder();

        for (var i = 0; i < input.Length; i++)
        {
            if (i < input.Length - 1 && input[i] == '{' && char.IsLetter(input[i + 1]))
            {
                // 检查是否存在下一个字符并且是字母，处理多字符键的情况
                var end = i + 2;
                while (end < input.Length && char.IsLetter(input[end - 1]))
                {
                    end++;
                }
                var key = input.Substring(i + 1, end - i - 2);

                if (pairs.TryGetValue(key, out var value))
                {
                    sb.Append(value?.ToString());
                    i = end - 1; // 跳过"{key}"
                }
                else
                {
                    sb.Append(input[i]); // 保留原始字符
                }
            }
            else
            {
                sb.Append(input[i]);
            }
        }

        return sb.ToString();
    }
}