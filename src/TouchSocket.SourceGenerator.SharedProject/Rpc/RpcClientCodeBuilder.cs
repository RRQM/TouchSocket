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

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Rpc;

namespace TouchSocket;

internal enum TaskType
{
    None,
    Task,
    TaskT
}

internal abstract class RpcClientCodeBuilder : CodeBuilder
{
    private readonly INamedTypeSymbol m_rpcApi;
    private readonly Dictionary<string, TypedConstant> m_rpcApiNamedArguments;

    protected RpcClientCodeBuilder(INamedTypeSymbol rpcApi, string rpcAttribute)
    {
        this.m_rpcApi = rpcApi;
        if (rpcApi.HasAttribute(RpcUtils.GeneratorRpcProxyAttributeTypeName, out var attributeData))
        {
            this.m_rpcApiNamedArguments = attributeData.NamedArguments.ToDictionary(a => a.Key, a => a.Value);

            if (this.m_rpcApiNamedArguments.TryGetValue("Prefix", out var typedConstant))
            {
                this.Prefix = typedConstant.Value.ToString();
            }
            else
            {
                this.Prefix = this.m_rpcApi.ToDisplayString();
            }

            this.RpcAttribute = rpcAttribute;
        }
    }

    public override string Id => this.m_rpcApi.ToDisplayString();
    public string Prefix { get; set; }
    public string RpcAttribute { get; private set; }
    public string ServerName { get; set; }

    public override IEnumerable<string> Usings
    {
        get
        {
            yield return "using System;";
            yield return "using System.Diagnostics;";
            yield return "using TouchSocket.Core;";
            yield return "using TouchSocket.Sockets;";
            yield return "using TouchSocket.Rpc;";
            yield return "using System.Threading.Tasks;";
        }
    }

    protected abstract string RpcAttributeName { get; }

    public override string GetFileName()
    {
        return this.m_rpcApi.ToDisplayString() + "Generator";
    }

    public string ReplacePatterns(Dictionary<string, TypedConstant> pairs, string input)
    {
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
                    sb.Append(this.ReplacePattern(key, value));
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

    protected virtual bool AllowAsync(GeneratorFlag flag, IMethodSymbol method = default, Dictionary<string, TypedConstant> namedArguments = default)
    {
        if (method != null)
        {
            if (method.Name.EndsWith("Async"))
            {
                return true;
            }

            if (method.ReturnType != null && method.ReturnType.IsInheritFrom(typeof(Task).FullName))
            {
                return true;
            }
        }
        if (namedArguments != null && namedArguments.TryGetValue("GeneratorFlag", out var typedConstant))
        {
            return ((GeneratorFlag)typedConstant.Value).HasFlag(flag);
        }
        else if (this.m_rpcApiNamedArguments != null && this.m_rpcApiNamedArguments.TryGetValue("GeneratorFlag", out typedConstant))
        {
            return ((GeneratorFlag)typedConstant.Value).HasFlag(flag);
        }
        return true;
    }

    protected virtual bool AllowSync(GeneratorFlag flag, IMethodSymbol method = default, Dictionary<string, TypedConstant> namedArguments = default)
    {
        if (method != null && method.Name.EndsWith("Async"))
        {
            return false;
        }
        if (namedArguments != null && namedArguments.TryGetValue("GeneratorFlag", out var typedConstant))
        {
            return ((GeneratorFlag)typedConstant.Value).HasFlag(flag);
        }
        else if (this.m_rpcApiNamedArguments != null && this.m_rpcApiNamedArguments.TryGetValue("GeneratorFlag", out typedConstant))
        {
            return ((GeneratorFlag)typedConstant.Value).HasFlag(flag);
        }
        return true;
    }

    protected virtual void BuildIntereface(StringBuilder codeBuilder)
    {
        var interfaceNames = new List<string>();
        if (this.IsInheritedInterface())
        {
            var interfaceNames1 = this.m_rpcApi.Interfaces
           .Where(a => RpcUtils.IsRpcApiInterface(a))
           .Select(a => $"I{this.GetInheritedClassName(a)}");

            var interfaceNames2 = this.m_rpcApi.Interfaces
               .Where(a => !RpcUtils.IsRpcApiInterface(a))
               .Select(a => a.ToDisplayString());

            interfaceNames.AddRange(interfaceNames1);
            interfaceNames.AddRange(interfaceNames2);
        }

        if (interfaceNames.Count == 0)
        {
            codeBuilder.AppendLine($"public partial interface I{this.GetClassName()}");
        }
        else
        {
            codeBuilder.AppendLine($"public partial interface I{this.GetClassName()} :{string.Join(",", interfaceNames)}");
        }

        codeBuilder.AppendLine("{");
        //Debugger.Launch();

        foreach (var method in this.FindApiMethods())
        {
            var methodCode = this.BuildMethodInterface(method);
            codeBuilder.AppendLine(methodCode);
        }

        codeBuilder.AppendLine("}");
    }

    protected virtual void BuildMethod(StringBuilder codeBuilder)
    {
        codeBuilder.AppendLine($"public static partial class {this.GetClassName()}Extensions");
        codeBuilder.AppendLine("{");
        //Debugger.Launch();

        foreach (var method in this.FindApiMethods())
        {
            var methodCode = this.BuildMethod(method);
            codeBuilder.AppendLine(methodCode);
        }

        codeBuilder.AppendLine("}");
    }

    protected virtual string BuildMethod(IMethodSymbol method)
    {
        //Debugger.Launch();
        var attributeData = RpcUtils.GetRpcAttribute(method, this.RpcAttribute);
        if (attributeData is null)
        {
            return string.Empty;
        }

        //Debugger.Launch();

        var namedArguments = attributeData.NamedArguments.ToDictionary(a => a.Key, a => a.Value);

        var invokeKey = this.GetInvokeKey(method, namedArguments);
        var methodName = this.GetMethodName(method, namedArguments);
        var genericConstraintTypes = this.GetGenericConstraintTypes(method, namedArguments);
        //var isIncludeCallContext = this.IsIncludeCallContext(method);
        var allowSync = this.AllowSync(GeneratorFlag.ExtensionSync, method, namedArguments);
        var allowAsync = this.AllowAsync(GeneratorFlag.ExtensionAsync, method, namedArguments);
        var returnType = this.GetReturnType(method);
        var taskType = this.GetTaskType(method);

        var parameters = method.Parameters.Where(a => (!RpcUtils.IsCallContext(a)) && (!RpcUtils.IsFromServices(a))).ToImmutableArray();

        //生成开始
        var codeBuilder = new StringBuilder();

        if (allowSync)
        {
            codeBuilder.AppendLine("///<summary>");
            codeBuilder.AppendLine($"///{this.GetDescription(method)}");
            codeBuilder.AppendLine("///</summary>");
            codeBuilder.AppendLine("[AsyncToSyncWarning]");
            codeBuilder.Append($"public static {returnType} {methodName}");
            codeBuilder.Append("<TClient>(");//方法参数

            codeBuilder.Append($"this TClient client");

            codeBuilder.Append(",");
            for (var i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                {
                    codeBuilder.Append(",");
                }
                codeBuilder.Append(this.GetMethodParameterString(parameters[i]));
            }
            if (parameters.Length > 0)
            {
                codeBuilder.Append(",");
            }
            codeBuilder.Append("InvokeOption invokeOption = default");
            codeBuilder.AppendLine($") where TClient:{string.Join(",", genericConstraintTypes)}");

            codeBuilder.AppendLine("{");//方法开始

            var parametersString = this.GetParametersString(parameters);

            if (method.HasReturn())
            {
                codeBuilder.AppendLine($"return ({returnType}) client.Invoke(\"{invokeKey}\",typeof({returnType}),invokeOption,{parametersString});");
            }
            else
            {
                codeBuilder.AppendLine($"client.Invoke(\"{invokeKey}\",default,invokeOption,{parametersString});");
            }
            codeBuilder.AppendLine("}");
        }

        if (allowAsync)
        {
            //以下生成异步
            codeBuilder.AppendLine("///<summary>");
            codeBuilder.AppendLine($"///{this.GetDescription(method)}");
            codeBuilder.AppendLine("///</summary>");
            if (method.HasReturn())
            {
                codeBuilder.Append($"public static async Task<{returnType}> {methodName}Async");
            }
            else
            {
                codeBuilder.Append($"public static Task {methodName}Async");
            }

            codeBuilder.Append("<TClient>(");//方法参数

            codeBuilder.Append($"this TClient client");

            codeBuilder.Append(",");
            for (var i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                {
                    codeBuilder.Append(",");
                }
                codeBuilder.Append(this.GetMethodParameterString(parameters[i]));
            }
            if (parameters.Length > 0)
            {
                codeBuilder.Append(",");
            }
            codeBuilder.Append("InvokeOption invokeOption = default");
            codeBuilder.AppendLine($") where TClient:{string.Join(",", genericConstraintTypes)}");

            codeBuilder.AppendLine("{");//方法开始
            var parametersString = this.GetParametersString(parameters);

            if (method.HasReturn())
            {
                codeBuilder.AppendLine($"return ({returnType}) await client.InvokeAsync(\"{invokeKey}\",typeof({returnType}),invokeOption,{parametersString});");
            }
            else
            {
                codeBuilder.AppendLine($"return client.InvokeAsync(\"{invokeKey}\",default,invokeOption,{parametersString});");
            }
            codeBuilder.AppendLine("}");
        }

        return codeBuilder.ToString();
    }

    protected virtual string BuildMethodInterface(IMethodSymbol method)
    {
        //Debugger.Launch();
        var attributeData = RpcUtils.GetRpcAttribute(method, this.RpcAttribute);

        if (attributeData is null)
        {
            return string.Empty;
        }

        //Debugger.Launch();

        var namedArguments = attributeData.NamedArguments.ToDictionary(a => a.Key, a => a.Value);

        var invokeKey = this.GetInvokeKey(method, namedArguments);
        var methodName = this.GetMethodName(method, namedArguments);
        var genericConstraintTypes = this.GetGenericConstraintTypes(method, namedArguments);
        //var isIncludeCallContext = this.IsIncludeCallContext(method);
        var allowSync = this.AllowSync(GeneratorFlag.InterfaceSync, method, namedArguments);
        var allowAsync = this.AllowAsync(GeneratorFlag.InterfaceAsync, method, namedArguments);
        var returnType = this.GetReturnType(method);

        var parameters = method.Parameters.Where(a => (!RpcUtils.IsCallContext(a)) && (!RpcUtils.IsFromServices(a))).ToImmutableArray();
        //if (isIncludeCallContext)
        //{
        //    parameters = parameters.RemoveAt(0);
        //}

        //生成开始
        var codeBuilder = new StringBuilder();

        if (allowSync)
        {
            codeBuilder.AppendLine("///<summary>");
            codeBuilder.AppendLine($"///{this.GetDescription(method)}");
            codeBuilder.AppendLine("///</summary>");
            codeBuilder.AppendLine("[AsyncToSyncWarning]");
            codeBuilder.Append($"{returnType} {methodName}");
            codeBuilder.Append("(");//方法参数
            for (var i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                {
                    codeBuilder.Append(",");
                }

                codeBuilder.Append(this.GetMethodParameterString(parameters[i]));
            }
            if (parameters.Length > 0)
            {
                codeBuilder.Append(",");
            }
            codeBuilder.Append("InvokeOption invokeOption = default");
            codeBuilder.AppendLine($");");
        }

        if (allowAsync)
        {
            //以下生成异步
            codeBuilder.AppendLine("///<summary>");
            codeBuilder.AppendLine($"///{this.GetDescription(method)}");
            codeBuilder.AppendLine("///</summary>");

            if (!method.HasReturn())
            {
                codeBuilder.Append($"Task {methodName}Async");
            }
            else
            {
                codeBuilder.Append($"Task<{returnType}> {methodName}Async");
            }

            codeBuilder.Append("(");//方法参数
            for (var i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                {
                    codeBuilder.Append(",");
                }
                codeBuilder.Append(this.GetMethodParameterString(parameters[i]));
            }
            if (parameters.Length > 0)
            {
                codeBuilder.Append(",");
            }
            codeBuilder.Append("InvokeOption invokeOption = default");
            codeBuilder.AppendLine($");");
        }

        return codeBuilder.ToString();
    }

    protected virtual IEnumerable<IMethodSymbol> FindApiMethods()
    {
        return this.m_rpcApi
            .GetMembers()
            .OfType<IMethodSymbol>();
    }

    protected override bool GeneratorCode(StringBuilder codeBuilder)
    {
        codeBuilder.AppendLine($"namespace {this.GetNamespace()}");
        codeBuilder.AppendLine("{");

        if (this.AllowAsync(GeneratorFlag.InterfaceSync) || this.AllowAsync(GeneratorFlag.InterfaceAsync))
        {
            this.BuildIntereface(codeBuilder);
        }

        if (this.AllowAsync(GeneratorFlag.ExtensionSync) || this.AllowAsync(GeneratorFlag.ExtensionAsync))
        {
            this.BuildMethod(codeBuilder);
        }
        codeBuilder.AppendLine("}");

        // System.Diagnostics.Debugger.Launch();
        return true;
    }

    protected virtual string GetClassName()
    {
        var className = this.m_rpcApi.Name;
        if (className.StartsWith("I"))
        {
            className = className.Remove(0, 1);
        }

        if (!this.m_rpcApiNamedArguments.TryGetValue("ClassName", out var typedConstant))
        {
            return className;
        }

        var classConstantName = typedConstant.Value?.ToString();
        if (!string.IsNullOrEmpty(classConstantName))
        {
            return string.Format(classConstantName.Replace("{Attribute}", this.RpcAttributeName), className);
        }
        return className;
    }

    protected virtual string GetDescription(IMethodSymbol method)
    {
        var desattribute = method.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == typeof(DescriptionAttribute).FullName);
        if (desattribute is null || desattribute.ConstructorArguments.Length == 0)
        {
            return "无注释信息";
        }

        return desattribute.ConstructorArguments[0].Value?.ToString();
    }

    protected virtual string[] GetGenericConstraintTypes(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
    {
        if (namedArguments.TryGetValue("GenericConstraintTypes", out var typedConstant))
        {
            return typedConstant.Values.Sort((a, b) =>
            {
                if (a.Type.IsAbstract)
                {
                    return 1;
                }
                return -1;
            }).Select(a => a.Value.ToString()).ToArray();
        }
        else if (this.m_rpcApiNamedArguments.TryGetValue("GenericConstraintTypes", out typedConstant))
        {
            return typedConstant.Values.Sort((a, b) =>
            {
                if (a.Type.IsAbstract)
                {
                    return 1;
                }
                return -1;
            }).Select(a => a.Value.ToString()).ToArray();
        }
        else
        {
            return new string[] { "TouchSocket.Rpc.IRpcClient" };
        }
    }

    protected abstract string GetInheritedClassName(INamedTypeSymbol rpcApi);

    protected virtual string GetInvokeKey(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
    {
        if (namedArguments.TryGetValue("InvokeKey", out var typedConstant))
        {
            return typedConstant.Value?.ToString() ?? string.Empty;
        }
        else if (namedArguments.TryGetValue("MethodInvoke", out typedConstant) && (typedConstant.Value is bool b && b))
        {
            return this.GetMethodName(method, namedArguments);
        }
        else if (this.m_rpcApiNamedArguments.TryGetValue("MethodInvoke", out typedConstant) && (typedConstant.Value is bool c && c))
        {
            return this.GetMethodName(method, namedArguments);
        }
        else
        {
            return $"{this.Prefix}.{method.Name}".ToLower();
        }
    }

    protected virtual string GetMethodName(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
    {
        //Debugger.Launch();
        string name;

        if (namedArguments.TryGetValue("MethodName", out var typedConstant))
        {
            name = typedConstant.Value?.ToString() ?? method.Name;
        }
        else
        {
            name = method.Name;
        }

        if (!string.IsNullOrEmpty(name))
        {
            name = string.Format(this.ReplacePatterns(namedArguments, name), method.Name);
        }
        else
        {
            name = method.Name;
        }

        return name.EndsWith("Async") ? name.Replace("Async", string.Empty) : name;
    }

    protected string GetMethodParameterString(IParameterSymbol parameter)
    {
        //Debugger.Launch();
        if (parameter.HasExplicitDefaultValue)
        {
            var data = parameter.ExplicitDefaultValue;
            if (data == null)
            {
                return $"{parameter.Type} {parameter.Name}=null";
            }
            else if (data.GetType() == typeof(string))
            {
                return $"{parameter.Type} {parameter.Name}=\"{parameter.ExplicitDefaultValue}\"";
            }
            else if (data is bool b)
            {
                return $"{parameter.Type} {parameter.Name}={b.ToString().ToLower()}";
            }
            else
            {
                return $"{parameter.Type} {parameter.Name}={parameter.ExplicitDefaultValue.ToString()}";
            }
        }
        return parameter.ToDisplayString();
    }

    protected virtual string GetNamespace()
    {
        var defaultNamespace = $"TouchSocket.Rpc.{this.RpcAttributeName}.Generators";
        if (!this.m_rpcApiNamedArguments.TryGetValue("Namespace", out var typedConstant))
        {
            return defaultNamespace;
        }
        var namespaceValue = typedConstant.Value?.ToString();
        if (string.IsNullOrEmpty(namespaceValue))
        {
            return defaultNamespace;
        }
        return string.Format(namespaceValue, this.RpcAttributeName);
    }

    protected virtual string GetParametersString(ImmutableArray<IParameterSymbol> parameters)
    {
        if (parameters.Length == 0)
        {
            return "default";
        }
        var codeBuilder = new StringBuilder();

        codeBuilder.Append($"new object[]");
        codeBuilder.Append("{");

        foreach (var parameter in parameters)
        {
            codeBuilder.Append(parameter.Name);
            if (!parameter.Equals(parameters[parameters.Length - 1], SymbolEqualityComparer.Default))
            {
                codeBuilder.Append(",");
            }
        }
        codeBuilder.AppendLine("}");

        return codeBuilder.ToString();
    }

    protected virtual string GetReturnType(IMethodSymbol method)
    {
        if (!method.HasReturn())
        {
            return "void";
        }

        if (method.ReturnType is IArrayTypeSymbol arrayTypeSymbol)
        {
            return arrayTypeSymbol.ToDisplayString();
        }

        if (method.ReturnType is not INamedTypeSymbol returnTypeSymbol)
        {
            return "void";
        }

        if (returnTypeSymbol.IsGenericType && returnTypeSymbol.IsInheritFrom(Utils.Task))
        {
            returnTypeSymbol = returnTypeSymbol.TypeArguments[0] as INamedTypeSymbol;
        }

        if (returnTypeSymbol.IsValueType)
        {
            return returnTypeSymbol.ToDisplayString();
        }
        else
        {
            return returnTypeSymbol.GetNullableType().ToDisplayString();
        }
    }

    protected virtual TaskType GetTaskType(IMethodSymbol method)
    {
        if (method.ReturnType.ToDisplayString().Contains($"{typeof(Task).FullName}<"))
        {
            return TaskType.TaskT;
        }
        else if (method.ReturnType.ToDisplayString().Contains($"{typeof(Task).FullName}"))
        {
            return TaskType.Task;
        }
        else
        {
            return TaskType.None;
        }
    }

    protected virtual bool IsInheritedInterface()
    {
        if (this.m_rpcApiNamedArguments.TryGetValue("InheritedInterface", out var typedConstant))
        {
            return typedConstant.Value is bool value && value;
        }
        return true;
    }

    protected virtual string ReplacePattern(string key, TypedConstant typedConstant)
    {
        return typedConstant.Value?.ToString();
    }
}