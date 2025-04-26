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
using System.Text;

namespace TouchSocket;

internal class MethodInvokeCodeBuilder : CodeBuilder
{
    private readonly INamedTypeSymbol m_namedTypeSymbol;

    public MethodInvokeCodeBuilder(INamedTypeSymbol type, List<IMethodSymbol> methodSymbols)
    {
        this.m_namedTypeSymbol = type;
        this.MethodSymbols = methodSymbols;
    }

    public INamedTypeSymbol NamedTypeSymbol => this.m_namedTypeSymbol;

    public virtual IEnumerable<string> Usings
    {
        get
        {
            yield return "using System;";
            yield return "using System.Diagnostics;";
            yield return "using TouchSocket.Core;";
            yield return "using System.Threading.Tasks;";
        }
    }

    protected virtual string GeneratorTypeNamespace => "TouchSocket.Core.__Internals";

    public override string Id => this.m_namedTypeSymbol.ToDisplayString();

    public List<IMethodSymbol> MethodSymbols { get; }

    public override string GetFileName()
    {
        return this.GeneratorTypeNamespace + this.GetGeneratorTypeName() + "Generator";
    }

    private string GetGeneratorTypeName()
    {
        var typeName = $"__{Utils.MakeIdentifier(this.m_namedTypeSymbol.ToDisplayString())}MethodExtension";

        return typeName;
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
        if (!this.NamedTypeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            codeString.AppendLine($"namespace {this.GeneratorTypeNamespace}");
            codeString.AppendLine("{");
        }

        codeString.AppendLine($"partial class {this.GetGeneratorTypeName()}");
        codeString.AppendLine("{");
        var methods = this.MethodSymbols;

        foreach (var item in methods)
        {
            this.BuildMethodFunc(codeString, item);
            this.BuildMethod(codeString, item);
        }
        codeString.AppendLine("}");

        if (!this.NamedTypeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            codeString.AppendLine("}");
        }

        // System.Diagnostics.Debugger.Launch();
        return codeString.ToString();
    }

    private void BuildMethod(StringBuilder codeString, IMethodSymbol method)
    {
        var objectName = this.GetObjectName(method);

        codeString.AppendLine($"private static object {this.GetMethodName(method)}(object {objectName}, object[] ps)");
        codeString.AppendLine("{");

        var ps = new List<string>();
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var parameter = method.Parameters[i];
            codeString.AppendLine($"var {parameter.Name}=({parameter.Type.ToDisplayString()})ps[{i}];");

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
                    codeString.AppendLine($"{method.ContainingType.ToDisplayString()}.{method.Name}({string.Join(",", ps)});");
                }
                else
                {
                    codeString.AppendLine($"(({method.ContainingType.ToDisplayString()}){objectName}).{method.Name}({string.Join(",", ps)});");
                }
            }
            else
            {
                if (method.IsStatic)
                {
                    codeString.AppendLine($"var result = {method.ContainingType.ToDisplayString()}.{method.Name}({string.Join(",", ps)});");
                }
                else
                {
                    codeString.AppendLine($"var result = (({method.ContainingType.ToDisplayString()}){objectName}).{method.Name}({string.Join(",", ps)});");
                }

            }
        }
        else
        {
            if (method.ReturnsVoid)
            {
                if (method.IsStatic)
                {
                    codeString.AppendLine($"{method.ContainingType.ToDisplayString()}.{method.Name}();");
                }
                else
                {
                    codeString.AppendLine($"(({method.ContainingType.ToDisplayString()}){objectName}).{method.Name}();");
                }

            }
            else
            {
                if (method.IsStatic)
                {
                    codeString.AppendLine($"var result = {method.ContainingType.ToDisplayString()}.{method.Name}();");
                }
                else
                {
                    codeString.AppendLine($"var result = (({method.ContainingType.ToDisplayString()}){objectName}).{method.Name}();");
                }
            }
        }
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var parameter = method.Parameters[i];

            if (parameter.RefKind == RefKind.Ref || parameter.RefKind == RefKind.Out)
            {
                codeString.AppendLine($"ps[{i}]={parameter.Name};");
            }
        }

        if (method.ReturnsVoid)
        {
            codeString.AppendLine("return default;");
        }
        else
        {
            codeString.AppendLine("return result;");
        }
        codeString.AppendLine("}");
    }

    private void BuildMethodFunc(StringBuilder codeString, IMethodSymbol method)
    {
        codeString.AppendLine($"public static Func<object, object[], object> {this.GetMethodName(method)}Func => {this.GetMethodName(method)};");
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