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
using System.Linq;
using System.Text;

namespace TouchSocket;

internal class MethodInvokeClassCodeBuilder : MethodCodeBuilder
{
    private readonly Compilation m_compilation;

    public MethodInvokeClassCodeBuilder(INamedTypeSymbol type, Compilation compilation, List<IMethodSymbol> methodSymbols) : base(type)
    {
        //Debugger.Launch();
        this.m_compilation = compilation;
        this.MethodSymbols = methodSymbols;
    }

    public List<IMethodSymbol> MethodSymbols { get; }

    public override string GetFileName()
    {
        return this.GeneratorTypeNamespace + this.GetGeneratorTypeName() + "Class.Generator.g.cs";
    }

    protected override bool GeneratorCode(StringBuilder codeBuilder)
    {
        using (this.CreateNamespaceIfNotGlobalNamespace(codeBuilder, this.GeneratorTypeNamespace))
        {
            codeBuilder.AppendLine($"partial class {this.GetGeneratorTypeName()}");
            using (this.CreateCodeSpace(codeBuilder))
            {
                var methods = this.MethodSymbols;

                foreach (var item in methods)
                {
                    //Debugger.Launch();
                    this.BuildMethodFunc(codeBuilder, item);
                    this.BuildClass(codeBuilder, item);
                }
            }
        }
        return true;
    }

    private void BuildClass(StringBuilder codeBuilder, IMethodSymbol method)
    {
        var isTypeAwaitable = this.IsTypeAwaitable(method.ReturnType, out var returnType);

        codeBuilder.AppendLine($"class {this.GetMethodName(method)}Class : IDynamicMethodInfo");
        using (this.CreateCodeSpace(codeBuilder))
        {
            if (method.ReturnType.IsVoid())
            {
                codeBuilder.AppendLine("public Type RealReturnType => default;");
                codeBuilder.AppendLine("public MethodReturnKind ReturnKind => MethodReturnKind.Void;");

                this.BuildMethod(codeBuilder, method);

                codeBuilder.AppendLine("public Task<object> GetResultAsync(object o)");
                using (this.CreateCodeSpace(codeBuilder))
                {
                    codeBuilder.AppendLine("throw new NotImplementedException();");
                }
            }
            else if (isTypeAwaitable)
            {
                var hasConfigureAwait = this.HasConfigureAwait(method.ReturnType);
                if (returnType == null)
                {
                    codeBuilder.AppendLine("public Type RealReturnType => default;");
                    codeBuilder.AppendLine("public MethodReturnKind ReturnKind => MethodReturnKind.Awaitable;");

                    this.BuildMethod(codeBuilder, method);

                    codeBuilder.AppendLine("public async Task<object> GetResultAsync(object o)");
                    using (this.CreateCodeSpace(codeBuilder))
                    {
                        if (method.ReturnType.IsValueType)
                        {
                            codeBuilder.AppendLine($"var result = ({method.ReturnType.ToDisplayString()})o;");
                        }
                        else
                        {
                            codeBuilder.AppendLine($"var result = System.Runtime.CompilerServices.Unsafe.As<{method.ReturnType.ToDisplayString()}>(o);");
                        }

                        if (hasConfigureAwait)
                        {
                            codeBuilder.AppendLine("await result.ConfigureAwait(EasyTask.ContinueOnCapturedContext);");
                        }
                        else
                        {
                            codeBuilder.AppendLine("await result;");
                        }

                        codeBuilder.AppendLine("return default;");
                    }
                }
                else
                {
                    codeBuilder.AppendLine($"public Type RealReturnType =>typeof({returnType.GetTypeofString()}) ;");
                    codeBuilder.AppendLine("public MethodReturnKind ReturnKind => MethodReturnKind.AwaitableObject;");

                    this.BuildMethod(codeBuilder, method);

                    codeBuilder.AppendLine("public async Task<object> GetResultAsync(object o)");
                    using (this.CreateCodeSpace(codeBuilder))
                    {
                        if (method.ReturnType.IsValueType)
                        {
                            codeBuilder.AppendLine($"var result = ({method.ReturnType.ToDisplayString()})o;");
                        }
                        else
                        {
                            codeBuilder.AppendLine($"var result = System.Runtime.CompilerServices.Unsafe.As<{method.ReturnType.ToDisplayString()}>(o);");
                        }

                        if (hasConfigureAwait)
                        {
                            codeBuilder.AppendLine("return await result.ConfigureAwait(EasyTask.ContinueOnCapturedContext);");
                        }
                        else
                        {
                            codeBuilder.AppendLine("return await result;");
                        }
                        codeBuilder.AppendLine("return await result;");
                    }
                }
            }
            else
            {
                codeBuilder.AppendLine($"public Type RealReturnType => typeof({method.ReturnType.GetTypeofString()});");
                codeBuilder.AppendLine("public MethodReturnKind ReturnKind => MethodReturnKind.Object;");

                this.BuildMethod(codeBuilder, method);
                codeBuilder.AppendLine("public Task<object> GetResultAsync(object o)");
                using (this.CreateCodeSpace(codeBuilder))
                {
                    codeBuilder.AppendLine("throw new NotImplementedException();");
                }
            }
        }
    }

    private bool HasConfigureAwait(ITypeSymbol taskType)
    {
        var configureAwaitMethod = taskType?.GetMembers("ConfigureAwait")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Parameters.Length == 1 && m.Parameters[0].Type.SpecialType == SpecialType.System_Boolean);
        return configureAwaitMethod != null;
    }

    private void BuildMethod(StringBuilder codeBuilder, IMethodSymbol method)
    {
        //var objectName = this.GetObjectName(method);
        codeBuilder.AppendLine($"public object Invoke(object instance, object[] ps)");
        codeBuilder.AppendLine("{");

        var ps = new List<string>();
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var parameter = method.Parameters[i];
            codeBuilder.AppendLine($"var {parameter.Name}=({parameter.Type.ToDisplayString()})ps[{i}];");

            if (parameter.RefKind == RefKind.Ref)
            {
                ps.Add($"ref {parameter.Name}");
            }
            else if (parameter.RefKind == RefKind.Out)
            {
                ps.Add($"out {parameter.Name}");
            }
            else
            {
                ps.Add(parameter.Name);
            }
        }
        if (ps.Count > 0)
        {
            if (method.ReturnsVoid)
            {
                if (method.IsStatic)
                {
                    codeBuilder.AppendLine($"{method.ContainingType.ToDisplayString()}.{method.Name}({string.Join(",", ps)});");
                }
                else
                {
                    codeBuilder.AppendLine($"(({method.ContainingType.ToDisplayString()})instance).{method.Name}({string.Join(",", ps)});");
                }
            }
            else
            {
                if (method.IsStatic)
                {
                    codeBuilder.AppendLine($"var result = {method.ContainingType.ToDisplayString()}.{method.Name}({string.Join(",", ps)});");
                }
                else
                {
                    codeBuilder.AppendLine($"var result = (({method.ContainingType.ToDisplayString()})instance).{method.Name}({string.Join(",", ps)});");
                }
            }
        }
        else
        {
            if (method.ReturnsVoid)
            {
                if (method.IsStatic)
                {
                    codeBuilder.AppendLine($"{method.ContainingType.ToDisplayString()}.{method.Name}();");
                }
                else
                {
                    codeBuilder.AppendLine($"(({method.ContainingType.ToDisplayString()})instance).{method.Name}();");
                }
            }
            else
            {
                if (method.IsStatic)
                {
                    codeBuilder.AppendLine($"var result = {method.ContainingType.ToDisplayString()}.{method.Name}();");
                }
                else
                {
                    codeBuilder.AppendLine($"var result = (({method.ContainingType.ToDisplayString()})instance).{method.Name}();");
                }
            }
        }
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var parameter = method.Parameters[i];

            if (parameter.RefKind == RefKind.Ref || parameter.RefKind == RefKind.Out)
            {
                codeBuilder.AppendLine($"ps[{i}]={parameter.Name};");
            }
        }

        if (method.ReturnsVoid)
        {
            codeBuilder.AppendLine("return default;");
        }
        else
        {
            codeBuilder.AppendLine("return result;");
        }
        codeBuilder.AppendLine("}");
    }

    private void BuildMethodFunc(StringBuilder codeBuilder, IMethodSymbol method)
    {
        codeBuilder.AppendLine($"public static IDynamicMethodInfo {this.GetMethodName(method)}ClassProperty => new {this.GetMethodName(method)}Class();");
    }

    private bool IsTypeAwaitable(ITypeSymbol typeSymbol, out ITypeSymbol returnType)
    {
        returnType = default;
        // 查找无参数的GetAwaiter实例方法
        var getAwaiterMethod = typeSymbol.GetMembers("GetAwaiter")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Parameters.IsEmpty && m.MethodKind == MethodKind.Ordinary);

        if (getAwaiterMethod == null)
        {
            return false; // 无符合条件的实例方法
        }

        var awaiterType = getAwaiterMethod.ReturnType as INamedTypeSymbol;
        if (awaiterType == null)
        {
            return false;
        }

        // 获取INotifyCompletion和ICriticalNotifyCompletion接口符号
        var inotifyCompletion = this.m_compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.INotifyCompletion");
        var icriticalNotifyCompletion = this.m_compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.ICriticalNotifyCompletion");

        if (inotifyCompletion == null || icriticalNotifyCompletion == null)
        {
            return false; // 编译环境中缺少必要接口
        }

        // 检查是否实现任一接口
        var implementsInterface = awaiterType.AllInterfaces.Any(i =>
            SymbolEqualityComparer.Default.Equals(i, inotifyCompletion) ||
            SymbolEqualityComparer.Default.Equals(i, icriticalNotifyCompletion));

        if (!implementsInterface)
        {
            return false;
        }

        // 检查IsCompleted属性是否存在且类型为bool
        var isCompleted = awaiterType.GetMembers("IsCompleted")
            .OfType<IPropertySymbol>()
            .FirstOrDefault(p => p.Type.SpecialType == SpecialType.System_Boolean && p.GetMethod != null);

        if (isCompleted == null)
        {
            return false;
        }

        // 检查GetResult方法是否存在并无参数
        var getResult = awaiterType.GetMembers("GetResult")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Parameters.IsEmpty);
        if (getResult.ReturnType.IsVoid())
        {
            returnType = default;
        }
        else
        {
            returnType = getResult.ReturnType;
        }

        return getResult != null;
    }

    private string GetMethodName(IMethodSymbol method)
    {
        return method.GetDeterminantName();
    }

    private string GetObjectName(IMethodSymbol method)
    {
        foreach (var item1 in new string[] { "obj", "targetObj", "target", "@obj", "@targetObj", "@target" })
        {
            var same = false;
            foreach (var item2 in method.Parameters)
            {
                if (item2.Name == item1)
                {
                    same = true;
                    break;
                }
            }

            if (!same)
            {
                return item1;
            }
        }

        return "@obj";
    }
}