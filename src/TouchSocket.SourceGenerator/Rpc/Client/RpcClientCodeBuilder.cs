using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket
{
    internal sealed class RpcClientCodeBuilder
    {
        private readonly INamedTypeSymbol m_rpcApi;

        private readonly Dictionary<string, TypedConstant> m_rpcApiNamedArguments;

        public RpcClientCodeBuilder(INamedTypeSymbol rpcApi)
        {
            this.m_rpcApi = rpcApi;
            var attributeData = rpcApi.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == RpcClientSyntaxReceiver.GeneratorRpcProxyAttributeTypeName);

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
        public enum CodeGeneratorFlag
        {
            /// <summary>
            /// 生成扩展同步代码
            /// </summary>
            ExtensionSync = 1,

            /// <summary>
            /// 生成扩展异步代码
            /// </summary>
            ExtensionAsync = 2,

            /// <summary>
            /// 包含扩展（源代码生成无效）
            /// </summary>
            [Obsolete("该值已被弃用，请使用颗粒度更小的配置", true)]
            IncludeExtension = 4,

            /// <summary>
            /// 生成实例类同步代码（源代码生成无效）
            /// </summary>
            InstanceSync = 8,

            /// <summary>
            /// 生成实例类异步代码（源代码生成无效）
            /// </summary>
            InstanceAsync = 16,

            /// <summary>
            /// 生成接口同步代码
            /// </summary>
            InterfaceSync = 32,

            /// <summary>
            /// 生成接口异步代码
            /// </summary>
            InterfaceAsync = 64,
        }

        public string Prefix { get; set; }

        public string ServerName { get; set; }

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

        public SourceText ToSourceText()
        {
            var code = this.ToString();
            return SourceText.From(code, Encoding.UTF8);
        }

        public override string ToString()
        {
            var codeString = new StringBuilder();
            codeString.AppendLine("/*");
            codeString.AppendLine("此代码由Rpc工具直接生成，非必要请不要修改此处代码");
            codeString.AppendLine("*/");
            codeString.AppendLine("#pragma warning disable");

            foreach (var item in this.Usings)
            {
                codeString.AppendLine(item);
            }
            codeString.AppendLine($"namespace {this.GetNamespace()}");
            codeString.AppendLine("{");

            if (this.AllowAsync(CodeGeneratorFlag.InterfaceSync) || this.AllowAsync(CodeGeneratorFlag.InterfaceAsync))
            {
                this.BuildIntereface(codeString);
            }

            if (this.AllowAsync(CodeGeneratorFlag.ExtensionSync) || this.AllowAsync(CodeGeneratorFlag.ExtensionAsync))
            {
                this.BuildMethod(codeString);
            }
            codeString.AppendLine("}");

            // System.Diagnostics.Debugger.Launch();
            return codeString.ToString();
        }

        private bool AllowAsync(CodeGeneratorFlag flag, IMethodSymbol method = default, Dictionary<string, TypedConstant> namedArguments = default)
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

        private void BuildIntereface(StringBuilder codeString)
        {
            var interfaceNames = new List<string>();
            if (this.IsInheritedInterface())
            {
                var interfaceNames1 = this.m_rpcApi.Interfaces
               .Where(a => RpcClientSyntaxReceiver.IsRpcApiInterface(a))
               .Select(a => $"I{new RpcClientCodeBuilder(a).GetClassName()}");

                var interfaceNames2 = this.m_rpcApi.Interfaces
                   .Where(a => !RpcClientSyntaxReceiver.IsRpcApiInterface(a))
                   .Select(a => a.ToDisplayString());

                interfaceNames.AddRange(interfaceNames1);
                interfaceNames.AddRange(interfaceNames2);
            }

            if (interfaceNames.Count == 0)
            {
                codeString.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"TouchSocket.SourceGenerator\",\"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}\")]");
                codeString.AppendLine($"public interface I{this.GetClassName()}");
            }
            else
            {
                codeString.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"TouchSocket.SourceGenerator\",\"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}\")]");
                codeString.AppendLine($"public interface I{this.GetClassName()} :{string.Join(",", interfaceNames)}");
            }

            codeString.AppendLine("{");
            //Debugger.Launch();

            foreach (var method in this.FindApiMethods())
            {
                var methodCode = this.BuildMethodInterface(method);
                codeString.AppendLine(methodCode);
            }

            codeString.AppendLine("}");
        }

        private void BuildMethod(StringBuilder codeString)
        {
            codeString.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"TouchSocket.SourceGenerator\",\"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}\")]");
            codeString.AppendLine($"public static class {this.GetClassName()}Extensions");
            codeString.AppendLine("{");
            //Debugger.Launch();

            foreach (var method in this.FindApiMethods())
            {
                var methodCode = this.BuildMethod(method);
                codeString.AppendLine(methodCode);
            }

            codeString.AppendLine("}");
        }

        private string BuildMethod(IMethodSymbol method)
        {
            //Debugger.Launch();
            var attributeData = method.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == RpcClientSyntaxReceiver.RpcMethodAttributeTypeName);
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
            var returnType = this.GetReturnType(method);
            var taskType = this.GetTaskType(method);

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
                    codeString.Append($"{parameters[i].ToDisplayString()}");
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

                if (method.HasReturn())
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

                if (method.HasReturn())
                {
                    codeString.AppendLine("return returnData;");
                }

                codeString.AppendLine("}");
            }

            //if (isOut || isRef)
            //{
            //    return codeString.ToString();
            //}

            if (allowAsync)
            {
                //以下生成异步
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{this.GetDescription(method)}");
                codeString.AppendLine("///</summary>");
                if (!method.HasReturn())
                {
                    codeString.Append($"public static Task {methodName}Async");
                }
                else
                {
                    if (isOut || isRef)
                    {
                        codeString.Append($"public static Task<{returnType}> {methodName}Async");
                    }
                    else
                    {
                        codeString.Append($"public static async Task<{returnType}> {methodName}Async");
                    }
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
                    codeString.Append($"{parameters[i].ToDisplayString()}");
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

                if (method.HasReturn())
                {
                    if (parameters.Length == 0)
                    {
                        codeString.Append(string.Format("return ({0}) await client.InvokeAsync", returnType));
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
                    else if (isOut || isRef)
                    {
                        codeString.Append("client.Invoke(");
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption,ref parameters,types);");
                    }
                    else
                    {
                        codeString.Append("return client.InvokeAsync(");
                        codeString.Append($"\"{invokeKey}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }

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

                    if (method.HasReturn())
                    {
                        codeString.AppendLine(string.Format("return Task.FromResult<{0}>(returnData);", returnType));
                    }
                    else
                    {
                        codeString.AppendLine(string.Format("return  EasyTask.CompletedTask;"));
                    }
                }
                codeString.AppendLine("}");
            }

            return codeString.ToString();
        }

        private string BuildMethodInterface(IMethodSymbol method)
        {
            //Debugger.Launch();
            var attributeData = method.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == RpcClientSyntaxReceiver.RpcMethodAttributeTypeName);
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
            var returnType = this.GetReturnType(method);
          
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

                    codeString.Append($"{parameters[i].ToDisplayString()}");
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

                if (!method.HasReturn())
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
                    codeString.Append($"{parameters[i].ToDisplayString()}");
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
                    return parameterSymbol.Type.ToDisplayString().Replace("ref", string.Empty);

                case RefKind.Out:
                    return parameterSymbol.Type.ToDisplayString().Replace("out", string.Empty);

                case RefKind.None:
                case RefKind.In:
                default:
                    return parameterSymbol.Type.ToDisplayString();
            }
        }

        private string GetReturnType(IMethodSymbol method)
        {
            if (method.HasReturn())
            {
                if (method.ReturnType.ToDisplayString().Contains($"{typeof(Task).FullName}<"))
                {
                    var methodname = method.ReturnType.ToDisplayString().Trim().Replace($"{typeof(Task).FullName}<", string.Empty);
                    methodname = methodname.Remove(methodname.Length - 1);
                    return methodname;
                }
                return method.ReturnType.ToDisplayString();
            }
            else
            {
                return "void";
            }
        }

        private TaskType GetTaskType(IMethodSymbol method)
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

        enum TaskType
        {
            None,
            Task,
            TaskT
        }


        private bool IsIncludeCallContext(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
        {
            if (method.Parameters.Length > 0)
            {
                var parameter = method.Parameters[0];
                return parameter.Type.IsInheritFrom("TouchSocket.Rpc.ICallContext");
            }
            return false;
            //if (namedArguments.TryGetValue("MethodFlags", out var typedConstant))
            //{
            //    return typedConstant.Value is int value && this.HasFlags(value, 2);
            //}
            //else if (this.m_rpcApiNamedArguments.TryGetValue("MethodFlags", out typedConstant))
            //{
            //    return typedConstant.Value is int value && this.HasFlags(value, 2);
            //}
            //return false;
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