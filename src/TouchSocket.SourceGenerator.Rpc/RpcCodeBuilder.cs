using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.SourceGenerator.Rpc
{
    /// <summary>
    /// RpcApi代码构建器
    /// </summary>
    internal sealed class RpcCodeBuilder
    {
        /// <summary>
        /// 接口符号
        /// </summary>
        private readonly INamedTypeSymbol m_rpcApi;

        private readonly Dictionary<string, TypedConstant> m_rpcApiNamedArguments;

        /// <summary>
        /// RpcApi代码构建器
        /// </summary>
        /// <param name="rpcApi"></param>
        public RpcCodeBuilder(INamedTypeSymbol rpcApi)
        {
            this.m_rpcApi = rpcApi;
            var attributeData = rpcApi.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == RpcSyntaxReceiver.GeneratorRpcProxyAttributeTypeName);

            this.m_rpcApiNamedArguments = attributeData.NamedArguments.ToDictionary(a => a.Key, a => a.Value);

            if (this.m_rpcApiNamedArguments.TryGetValue("Prefix", out var typedConstant))
            {
                this.Prefix = typedConstant.Value.ToString();
            }
            else
            {
                this.Prefix = this.m_rpcApi.ToDisplayString();
            }
        }

        /// <summary>
        /// 代码生成标识
        /// </summary>
        [Flags]
        private enum CodeGeneratorFlag
        {
            /// <summary>
            /// 生成同步代码（源代码生成无效）
            /// </summary>
            [Obsolete("该值已被弃用，请使用颗粒度更小的配置", true)]
            Sync = 1,

            /// <summary>
            /// 生成异步代码（源代码生成无效）
            /// </summary>
            [Obsolete("该值已被弃用，请使用颗粒度更小的配置", true)]
            Async = 2,

            /// <summary>
            /// 生成扩展同步代码
            /// </summary>
            ExtensionSync = 4,

            /// <summary>
            /// 生成扩展异步代码
            /// </summary>
            ExtensionAsync = 8,

            /// <summary>
            /// 包含接口（源代码生成无效）
            /// </summary>
            [Obsolete("该值已被弃用，请使用颗粒度更小的配置", true)]
            IncludeInterface = 16,

            /// <summary>
            /// 包含实例（源代码生成无效）
            /// </summary>
            [Obsolete("该值已被弃用，请使用颗粒度更小的配置", true)]
            IncludeInstance = 32,

            /// <summary>
            /// 包含扩展（源代码生成无效）
            /// </summary>
            [Obsolete("该值已被弃用，请使用颗粒度更小的配置", true)]
            IncludeExtension = 64,

            /// <summary>
            /// 生成实例类同步代码（源代码生成无效）
            /// </summary>
            InstanceSync = 128,

            /// <summary>
            /// 生成实例类异步代码（源代码生成无效）
            /// </summary>
            InstanceAsync = 256,

            /// <summary>
            /// 生成接口同步代码
            /// </summary>
            InterfaceSync = 512,

            /// <summary>
            /// 生成接口异步代码
            /// </summary>
            InterfaceAsync = 1024,
        }

        public string Prefix { get; set; }

        public string ServerName { get; set; }

        /// <summary>
        /// using
        /// </summary>
        public IEnumerable<string> Usings
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

        public string GetFileName()
        {
            return this.m_rpcApi.ToDisplayString() + "Generator";
        }

        /// <summary>
        /// 转换为SourceText
        /// </summary>
        /// <returns></returns>
        public SourceText ToSourceText()
        {
            var code = this.ToString();
            return SourceText.From(code, Encoding.UTF8);
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var item in this.Usings)
            {
                builder.AppendLine(item);
            }
            builder.AppendLine($"namespace {this.GetNamespace()}");
            builder.AppendLine("{");

            if (this.AllowAsync(CodeGeneratorFlag.InterfaceSync) || this.AllowAsync(CodeGeneratorFlag.InterfaceAsync))
            {
                this.BuildIntereface(builder);
            }

            if (this.AllowAsync(CodeGeneratorFlag.ExtensionSync) || this.AllowAsync(CodeGeneratorFlag.ExtensionAsync))
            {
                this.BuildMethod(builder);
            }
            builder.AppendLine("}");

            // System.Diagnostics.Debugger.Launch();
            return builder.ToString();
        }

        private bool AllowAsync(CodeGeneratorFlag flag, IMethodSymbol method = default, Dictionary<string, TypedConstant> namedArguments = default)
        {
            if (method != null && method.Name.EndsWith("Async"))
            {
                return true;
            }
            if (namedArguments != null && namedArguments.TryGetValue("GeneratorFlag", out var typedConstant))
            {
                return ((CodeGeneratorFlag)typedConstant.Value).HasFlag(flag);
            }
            else if (this.m_rpcApiNamedArguments != null && this.m_rpcApiNamedArguments.TryGetValue("GeneratorFlag", out typedConstant))
            {
                return ((CodeGeneratorFlag)typedConstant.Value).HasFlag(flag);
            }
            return true;
        }

        private bool AllowSync(CodeGeneratorFlag flag, IMethodSymbol method = default, Dictionary<string, TypedConstant> namedArguments = default)
        {
            if (method != null && method.Name.EndsWith("Async"))
            {
                return false;
            }
            if (namedArguments != null && namedArguments.TryGetValue("GeneratorFlag", out var typedConstant))
            {
                return ((CodeGeneratorFlag)typedConstant.Value).HasFlag(flag);
            }
            else if (this.m_rpcApiNamedArguments != null && this.m_rpcApiNamedArguments.TryGetValue("GeneratorFlag", out typedConstant))
            {
                return ((CodeGeneratorFlag)typedConstant.Value).HasFlag(flag);
            }
            return true;
        }

        private void BuildIntereface(StringBuilder builder)
        {
            var interfaceNames = new List<string>();
            if (this.IsInheritedInterface())
            {
                var interfaceNames1 = this.m_rpcApi.Interfaces
               .Where(a => RpcSyntaxReceiver.IsRpcApiInterface(a))
               .Select(a => $"I{new RpcCodeBuilder(a).GetClassName()}");

                var interfaceNames2 = this.m_rpcApi.Interfaces
                   .Where(a => !RpcSyntaxReceiver.IsRpcApiInterface(a))
                   .Select(a => a.ToDisplayString());

                interfaceNames.AddRange(interfaceNames1);
                interfaceNames.AddRange(interfaceNames2);
            }

            if (interfaceNames.Count == 0)
            {
                builder.AppendLine($"public interface I{this.GetClassName()}");
            }
            else
            {
                builder.AppendLine($"public interface I{this.GetClassName()} :{string.Join(",", interfaceNames)}");
            }

            builder.AppendLine("{");
            //Debugger.Launch();

            foreach (var method in this.FindApiMethods())
            {
                var methodCode = this.BuildMethodInterface(method);
                builder.AppendLine(methodCode);
            }

            builder.AppendLine("}");
        }

        private void BuildMethod(StringBuilder builder)
        {
            builder.AppendLine($"public static class {this.GetClassName()}Extensions");
            builder.AppendLine("{");
            //Debugger.Launch();

            foreach (var method in this.FindApiMethods())
            {
                var methodCode = this.BuildMethod(method);
                builder.AppendLine(methodCode);
            }

            builder.AppendLine("}");
        }

        private string BuildMethod(IMethodSymbol method)
        {
            //Debugger.Launch();
            var attributeData = method.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == RpcSyntaxReceiver.RpcMethodAttributeTypeName);
            if (attributeData is null)
            {
                return string.Empty;
            }

            //Debugger.Launch();

            var namedArguments = attributeData.NamedArguments.ToDictionary(a => a.Key, a => a.Value);

            var invokeKey = this.GetInvokeKey(method, namedArguments);
            var methodName = this.GetMethodName(method, namedArguments);
            var genericConstraintTypes = this.GetGenericConstraintTypes(method, namedArguments);
            var isIncludeCallContext = this.IsIncludeCallContext(method, namedArguments);
            var allowSync = this.AllowSync(CodeGeneratorFlag.ExtensionSync, method, namedArguments);
            var allowAsync = this.AllowAsync(CodeGeneratorFlag.ExtensionAsync, method, namedArguments);
            var returnType = this.GetReturnType(method, namedArguments);

            var parameters = method.Parameters;
            if (isIncludeCallContext)
            {
                parameters = parameters.RemoveAt(0);
            }

            //生成开始
            var codeString = new StringBuilder();
            var isOut = false;
            var isRef = false;

            if (allowSync)
            {
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{this.GetDescription(method)}");
                codeString.AppendLine("///</summary>");
                codeString.Append($"public static {returnType} {methodName}");
                codeString.Append("<TClient>(");//方法参数

                codeString.Append($"this TClient client");

                codeString.Append(",");
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(",");
                    }
                    codeString.Append($"{parameters[i].Type.ToDisplayString()} {parameters[i].Name}");
                }
                if (parameters.Length > 0)
                {
                    codeString.Append(",");
                }
                codeString.Append("IInvokeOption invokeOption = default");
                codeString.AppendLine($") where TClient:{string.Join(",", genericConstraintTypes)}");

                codeString.AppendLine("{");//方法开始
                if (parameters.Length > 0)
                {
                    codeString.Append($"object[] parameters = new object[]");
                    codeString.Append("{");

                    foreach (var parameter in parameters)
                    {
                        if (parameter.RefKind == RefKind.Ref)
                        {
                            isRef = true;
                        }
                        if (parameter.RefKind == RefKind.Out)
                        {
                            isOut = true;
                            codeString.Append($"default({this.GetRealTypeString(parameter)})");
                        }
                        else
                        {
                            codeString.Append(parameter.Name);
                        }
                        if (!parameter.Equals(parameters[parameters.Length - 1], SymbolEqualityComparer.Default))
                        {
                            codeString.Append(",");
                        }
                    }
                    codeString.AppendLine("};");

                    if (isOut || isRef)
                    {
                        codeString.Append($"Type[] types = new Type[]");
                        codeString.Append("{");
                        foreach (var parameter in parameters)
                        {
                            codeString.Append($"typeof({this.GetRealTypeString(parameter)})");
                            if (!parameter.Equals(parameters[parameters.Length - 1], SymbolEqualityComparer.Default))
                            {
                                codeString.Append(",");
                            }
                        }
                        codeString.AppendLine("};");
                    }
                }

                if (!method.ReturnsVoid)
                {
                    if (parameters.Length == 0)
                    {
                        codeString.Append(string.Format("{0} returnData=({0})client.Invoke", returnType));
                        codeString.Append("(");
                        codeString.Append(string.Format("typeof({0}),", returnType));
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else if (isOut || isRef)
                    {
                        codeString.Append(string.Format("{0} returnData=({0})client.Invoke", returnType));
                        codeString.Append("(");
                        codeString.Append(string.Format("typeof({0}),", returnType));
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption,ref parameters,types);");
                    }
                    else
                    {
                        codeString.Append(string.Format("{0} returnData=({0})client.Invoke", returnType));
                        codeString.Append("(");
                        codeString.Append(string.Format("typeof({0}),", returnType));
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }
                else
                {
                    if (parameters.Length == 0)
                    {
                        codeString.Append("client.Invoke(");
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else if (isOut || isRef)
                    {
                        codeString.Append("client.Invoke(");
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption,ref parameters,types);");
                    }
                    else
                    {
                        codeString.Append("client.Invoke(");
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }
                //Debugger.Launch();
                if (isOut || isRef)
                {
                    codeString.AppendLine("if(parameters!=null)");
                    codeString.AppendLine("{");
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        codeString.AppendLine(string.Format("{0}=({1})parameters[{2}];", parameters[i].Name, this.GetRealTypeString(parameters[i]), i));
                    }
                    codeString.AppendLine("}");
                    if (isOut)
                    {
                        codeString.AppendLine("else");
                        codeString.AppendLine("{");
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].RefKind == RefKind.Out)
                            {
                                codeString.AppendLine(string.Format("{0}=default({1});", parameters[i].Name, this.GetRealTypeString(parameters[i])));
                            }
                        }
                        codeString.AppendLine("}");
                    }
                }

                if (!method.ReturnsVoid)
                {
                    codeString.AppendLine("return returnData;");
                }

                codeString.AppendLine("}");
            }

            if (isOut || isRef)
            {
                return codeString.ToString();
            }

            if (allowAsync)
            {
                //以下生成异步
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{this.GetDescription(method)}");
                codeString.AppendLine("///</summary>");
                if (method.ReturnsVoid)
                {
                    codeString.Append($"public static Task {methodName}Async");
                }
                else
                {
                    codeString.Append($"public static async Task<{returnType}> {methodName}Async");
                }

                codeString.Append("<TClient>(");//方法参数

                codeString.Append($"this TClient client");

                codeString.Append(",");
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(",");
                    }
                    codeString.Append($"{parameters[i].Type.ToDisplayString()} {parameters[i].Name}");
                }
                if (parameters.Length > 0)
                {
                    codeString.Append(",");
                }
                codeString.Append("IInvokeOption invokeOption = default");
                codeString.AppendLine($") where TClient:{string.Join(",", genericConstraintTypes)}");

                codeString.AppendLine("{");//方法开始

                if (parameters.Length > 0)
                {
                    codeString.Append($"object[] parameters = new object[]");
                    codeString.Append("{");
                    foreach (var parameter in parameters)
                    {
                        codeString.Append(parameter.Name);
                        if (!parameter.Equals(parameters[parameters.Length - 1], SymbolEqualityComparer.Default))
                        {
                            codeString.Append(",");
                        }
                    }
                    codeString.AppendLine("};");
                }

                if (!method.ReturnsVoid)
                {
                    if (parameters.Length == 0)
                    {
                        codeString.Append(string.Format("return ({0}) await client.InvokeAsync", returnType));
                        codeString.Append("(");
                        codeString.Append(string.Format("typeof({0}),", returnType));
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else
                    {
                        codeString.Append(string.Format("return ({0}) await client.InvokeAsync", returnType));
                        codeString.Append("(");
                        codeString.Append(string.Format("typeof({0}),", returnType));
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }
                else
                {
                    if (parameters.Length == 0)
                    {
                        codeString.Append("return client.InvokeAsync(");
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else
                    {
                        codeString.Append("return client.InvokeAsync(");
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }
                codeString.AppendLine("}");
            }

            return codeString.ToString();
        }

        private string BuildMethodInterface(IMethodSymbol method)
        {
            //Debugger.Launch();
            var attributeData = method.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == RpcSyntaxReceiver.RpcMethodAttributeTypeName);
            if (attributeData is null)
            {
                return string.Empty;
            }

            //Debugger.Launch();

            var namedArguments = attributeData.NamedArguments.ToDictionary(a => a.Key, a => a.Value);

            var invokeKey = this.GetInvokeKey(method, namedArguments);
            var methodName = this.GetMethodName(method, namedArguments);
            var genericConstraintTypes = this.GetGenericConstraintTypes(method, namedArguments);
            var isIncludeCallContext = this.IsIncludeCallContext(method, namedArguments);
            var allowSync = this.AllowSync(CodeGeneratorFlag.InterfaceSync, method, namedArguments);
            var allowAsync = this.AllowAsync(CodeGeneratorFlag.InterfaceAsync, method, namedArguments);
            var returnType = this.GetReturnType(method, namedArguments);

            var parameters = method.Parameters;
            if (isIncludeCallContext)
            {
                parameters = parameters.RemoveAt(0);
            }

            //生成开始
            var codeString = new StringBuilder();
            var isOut = false;
            var isRef = false;

            if (allowSync)
            {
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{this.GetDescription(method)}");
                codeString.AppendLine("///</summary>");
                codeString.Append($"{returnType} {methodName}");
                codeString.Append("(");//方法参数
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(",");
                    }

                    codeString.Append($"{parameters[i].Type.ToDisplayString()} {parameters[i].Name}");
                }
                if (parameters.Length > 0)
                {
                    codeString.Append(",");
                }
                codeString.Append("IInvokeOption invokeOption = default");
                codeString.AppendLine($");");
            }

            if (isOut || isRef)
            {
                return codeString.ToString();
            }

            if (allowAsync)
            {
                //以下生成异步
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{this.GetDescription(method)}");
                codeString.AppendLine("///</summary>");
                if (method.ReturnsVoid)
                {
                    codeString.Append($"Task {methodName}Async");
                }
                else
                {
                    codeString.Append($"Task<{returnType}> {methodName}Async");
                }

                codeString.Append("(");//方法参数
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(",");
                    }
                    codeString.Append($"{parameters[i].Type.ToDisplayString()} {parameters[i].Name}");
                }
                if (parameters.Length > 0)
                {
                    codeString.Append(",");
                }
                codeString.Append("IInvokeOption invokeOption = default");
                codeString.AppendLine($");");
            }

            return codeString.ToString();
        }

        /// <summary>
        /// 查找接口类型及其继承的接口的所有方法
        /// </summary>
        /// <param name="httpApi">接口</param>
        /// <returns></returns>
        private IEnumerable<IMethodSymbol> FindApiMethods()
        {
            return this.m_rpcApi
                .GetMembers()
                .OfType<IMethodSymbol>();
        }

        private string GetClassName()
        {
            if (this.m_rpcApiNamedArguments.TryGetValue("ClassName", out var typedConstant))
            {
                return typedConstant.Value?.ToString();
            }
            else if (this.m_rpcApi.Name.StartsWith("I"))
            {
                return this.m_rpcApi.Name.Remove(0, 1);
            }
            return this.m_rpcApi.Name;
        }

        private string GetDescription(IMethodSymbol method)
        {
            var desattribute = method.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == typeof(DescriptionAttribute).FullName);
            if (desattribute is null || desattribute.ConstructorArguments.Length == 0)
            {
                return "无注释信息";
            }

            return desattribute.ConstructorArguments[0].Value?.ToString();
        }

        private string[] GetGenericConstraintTypes(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
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

        private string GetInvokeKey(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
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

        private string GetMethodName(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
        {
            if (namedArguments.TryGetValue("MethodName", out var typedConstant))
            {
                return typedConstant.Value?.ToString() ?? string.Empty;
            }

            return method.Name.EndsWith("Async") ? method.Name.Replace("Async", string.Empty) : method.Name;
        }

        private string GetNamespace()
        {
            if (this.m_rpcApiNamedArguments.TryGetValue("Namespace", out var typedConstant))
            {
                return typedConstant.Value?.ToString() ?? "TouchSocket.Rpc.Generators";
            }
            return "TouchSocket.Rpc.Generators";
        }

        private string GetRealTypeString(IParameterSymbol parameterSymbol)
        {
            switch (parameterSymbol.RefKind)
            {
                case RefKind.Ref:
                    return parameterSymbol.ToDisplayString().Replace("ref", string.Empty);

                case RefKind.Out:
                    return parameterSymbol.ToDisplayString().Replace("out", string.Empty);

                case RefKind.None:
                case RefKind.In:
                default:
                    return parameterSymbol.ToDisplayString();
            }
        }

        private string GetReturnType(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
        {
            if (method.ReturnType.ToDisplayString().Contains(typeof(Task).FullName))
            {
                var methodname = method.ReturnType.ToDisplayString().Trim().Replace($"{typeof(Task).FullName}<", string.Empty);
                methodname = methodname.Remove(methodname.Length - 1);
                return methodname;
            }
            return method.ReturnType.ToDisplayString();
        }

        private bool HasFlags(int value, int flag)
        {
            return (value & flag) == flag;
        }

        private bool HasReturn(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
        {
            if (method.ReturnsVoid || method.ReturnType.ToDisplayString() == typeof(Task).FullName)
            {
                return false;
            }
            return true;
        }

        private bool IsIncludeCallContext(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
        {
            if (namedArguments.TryGetValue("MethodFlags", out var typedConstant))
            {
                return typedConstant.Value is int value && this.HasFlags(value, 2);
            }
            else if (this.m_rpcApiNamedArguments.TryGetValue("MethodFlags", out typedConstant))
            {
                return typedConstant.Value is int value && this.HasFlags(value, 2);
            }
            return false;
        }

        private bool IsInheritedInterface()
        {
            if (this.m_rpcApiNamedArguments.TryGetValue("InheritedInterface", out var typedConstant))
            {
                return typedConstant.Value is bool value && value;
            }
            return true;
        }
    }
}