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
using TouchSocket.Rpc;

namespace TouchSocket;

internal class RegisterRpcServerCodeBuilder : CodeBuilder
{
    private readonly IAssemblySymbol m_assemblySymbol;
    private readonly Dictionary<string, TypedConstant> m_attributeDataDic;
    private readonly INamedTypeSymbol[] m_rpcApis;

    public RegisterRpcServerCodeBuilder(IAssemblySymbol assemblySymbol, AttributeData attributeData, INamedTypeSymbol[] rpcApis)
    {
        this.m_assemblySymbol = assemblySymbol;
        this.m_attributeDataDic = attributeData.NamedArguments.ToDictionary(a => a.Key, a => a.Value);
        this.m_rpcApis = rpcApis;
    }

    public override string Id => this.m_assemblySymbol.ToDisplayString();
    public INamedTypeSymbol[] RpcApis => this.m_rpcApis;

    public override string GetFileName()
    {
        return $"Register{this.GetAssemblyName()}RpcServerGenerator";
    }



    protected override bool GeneratorCode(StringBuilder codeBuilder)
    {
        codeBuilder.AppendLine($"namespace TouchSocket.Rpc");
        codeBuilder.AppendLine("{");
        codeBuilder.AppendLine("/// <summary>");
        codeBuilder.AppendLine($"/// {this.GetClassName()}");
        codeBuilder.AppendLine("/// </summary>");
        codeBuilder.AppendLine($"public static class {this.GetClassName()}");
        codeBuilder.AppendLine("{");

        var registers = this.RpcApis.Select(a => this.GetRegister(a));

        switch (this.GetAccessibility())
        {
            case Rpc.Accessibility.Both:
                this.BuildInternal(codeBuilder, registers);
                this.BuildPublic(codeBuilder, registers.Where(a => a.Item2.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public));
                break;

            case Rpc.Accessibility.Internal:
                this.BuildInternal(codeBuilder, registers);
                break;

            case Rpc.Accessibility.Public:
                this.BuildPublic(codeBuilder, registers.Where(a => a.Item2.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public));
                break;

            default:
                break;
        }
        codeBuilder.AppendLine("}");
        codeBuilder.AppendLine("}");

        return true;
    }

    private void BuildInternal(StringBuilder codeBuilder, IEnumerable<(INamedTypeSymbol, INamedTypeSymbol)> values)
    {
        if (values.Count() > 0)
        {
            codeBuilder.AppendLine("/// <summary>");
            codeBuilder.AppendLine($"/// 注册程序集{this.m_assemblySymbol.Name}中的所有Rpc服务。包括：");
            codeBuilder.AppendLine("/// <list type=\"number\">");
            foreach (var item in values)
            {
                codeBuilder.AppendLine($"/// <item><see cref=\"{item.Item1.ToDisplayString()}\"/>:<see cref=\"{item.Item2.ToDisplayString()}\"/></item>");
            }
            codeBuilder.AppendLine("/// </list>");
            codeBuilder.AppendLine("/// </summary>");
            codeBuilder.AppendLine("/// <param name=\"rpcStore\"></param>");
            codeBuilder.AppendLine($"internal static void Internal{this.GetMethodName()}(this RpcStore rpcStore)");
            codeBuilder.AppendLine("{");
            foreach (var item in values)
            {
                codeBuilder.AppendLine($"rpcStore.RegisterServer<{item.Item1.ToDisplayString()},{item.Item2.ToDisplayString()}>();");
            }
            codeBuilder.AppendLine("}");
        }
    }

    private void BuildPublic(StringBuilder codeBuilder, IEnumerable<(INamedTypeSymbol, INamedTypeSymbol)> values)
    {
        if (values.Count() > 0)
        {
            codeBuilder.AppendLine("/// <summary>");
            codeBuilder.AppendLine($"/// 注册程序集{this.m_assemblySymbol.Name}中的所有公共Rpc服务。包括：");
            codeBuilder.AppendLine("/// <list type=\"number\">");
            foreach (var item in values)
            {
                codeBuilder.AppendLine($"/// <item><see cref=\"{item.Item1.ToDisplayString()}\"/>:<see cref=\"{item.Item2.ToDisplayString()}\"/></item>");
            }
            codeBuilder.AppendLine("/// </list>");
            codeBuilder.AppendLine("/// </summary>");
            codeBuilder.AppendLine("/// <param name=\"rpcStore\"></param>");
            codeBuilder.AppendLine($"public static void {this.GetMethodName()}(this RpcStore rpcStore)");
            codeBuilder.AppendLine("{");
            foreach (var item in values)
            {
                codeBuilder.AppendLine($"rpcStore.RegisterServer<{item.Item1.ToDisplayString()},{item.Item2.ToDisplayString()}>();");
            }
            codeBuilder.AppendLine("}");
        }
    }

    private Rpc.Accessibility GetAccessibility()
    {
        if (this.m_attributeDataDic.TryGetValue(nameof(GeneratorRpcServerRegisterAttribute.Accessibility), out var typedConstant))
        {
            return (Rpc.Accessibility)typedConstant.Value;
        }

        return Rpc.Accessibility.Both;
    }

    private string GetAssemblyName()
    {
        return System.Text.RegularExpressions.Regex.Replace(this.m_assemblySymbol.Name, @"\W", "_");
    }

    private string GetClassName()
    {
        if (this.m_attributeDataDic.TryGetValue(nameof(GeneratorRpcServerRegisterAttribute.ClassName), out var typedConstant))
        {
            return typedConstant.Value.ToString();
        }

        return $"RegisterRpcServerFrom{this.GetAssemblyName()}Extension";
    }

    private string GetMethodName()
    {
        if (this.m_attributeDataDic.TryGetValue(nameof(GeneratorRpcServerRegisterAttribute.MethodName), out var typedConstant))
        {
            return typedConstant.Value.ToString();
        }

        return $"RegisterAllFrom{this.GetAssemblyName()}";
    }

    private (INamedTypeSymbol, INamedTypeSymbol) GetRegister(INamedTypeSymbol namedTypeSymbol)
    {
        var symbolsInterface = namedTypeSymbol.Interfaces.Where(a => a.IsInheritFrom(RpcServerSourceGenerator.IRpcServerTypeName) && a.ToDisplayString() != RpcServerSourceGenerator.IRpcServerTypeName).FirstOrDefault();

        if (symbolsInterface == null)
        {
            return (namedTypeSymbol, namedTypeSymbol);
        }

        return (symbolsInterface, namedTypeSymbol);
    }
}