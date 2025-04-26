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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using TouchSocket.Core;

namespace TouchSocket;

internal sealed class FastSerializeSyntaxReceiver : ISyntaxReceiver
{
    private readonly List<TypeDeclarationSyntax> m_syntaxList = new List<TypeDeclarationSyntax>();

    public Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> GetFastSerializeContexts(GeneratorExecutionContext context)
    {
        var compilation = context.Compilation;

        var pairs = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>();
        //Debugger.Launch();
        foreach (var syntax in this.m_syntaxList)
        {
            var namedTypeSymbol = compilation.GetSemanticModel(syntax.SyntaxTree).GetDeclaredSymbol(syntax);
            if (!namedTypeSymbol.HasAttributes(FastSerializeGenerator.FastSerializableAttributeString, out var atts))
            {
                continue;
            }

            var types = new List<INamedTypeSymbol>();

            foreach (var item in atts)
            {
                if (item.ConstructorArguments.Length == 1)
                {
                    if (item.ConstructorArguments[0].Value is not INamedTypeSymbol typeSymbol)
                    {
                        continue;
                    }

                    this.GetINamedTypeSymbols(typeSymbol, TypeMode.Self, ref types, context, item);
                }
                else if (item.ConstructorArguments.Length == 2)
                {
                    if (item.ConstructorArguments[0].Value is not INamedTypeSymbol typeSymbol)
                    {
                        continue;
                    }

                    if (item.ConstructorArguments[1].Value is not int value)
                    {
                        continue;
                    }

                    var typeMode = (TypeMode)value;
                    this.GetINamedTypeSymbols(typeSymbol, typeMode, ref types, context, item);
                }
            }

            types = types.Distinct().ToList();
            pairs.Add(namedTypeSymbol, types);
        }

        return pairs;
    }

    public void GetINamedTypeSymbols(INamedTypeSymbol namedTypeSymbol, TypeMode typeMode, ref List<INamedTypeSymbol> namedTypeSymbols, GeneratorExecutionContext context, AttributeData attributeData)
    {
        if (typeMode == TypeMode.Self)
        {
            //Debugger.Launch();
            if (CanBeAdd(namedTypeSymbol, context, attributeData))
            {
                namedTypeSymbols.Add(namedTypeSymbol);
            }
            return;
        }

        foreach (var symbol in namedTypeSymbol.GetMembers())
        {
            switch (symbol)
            {
                case IFieldSymbol targetSymbol:
                    {
                        if (typeMode == TypeMode.All || typeMode.HasFlag(TypeMode.Field))
                        {
                            if (targetSymbol.Type is INamedTypeSymbol namedType && CanBeAdd(namedType, context, attributeData))
                            {
                                namedTypeSymbols.Add(namedType);
                            }
                        }
                        break;
                    }
                case IPropertySymbol targetSymbol:
                    {
                        if (typeMode == TypeMode.All || typeMode.HasFlag(TypeMode.Property))
                        {
                            if (targetSymbol.Type is INamedTypeSymbol namedType && CanBeAdd(namedType, context, attributeData))
                            {
                                namedTypeSymbols.Add(namedType);
                            }
                        }
                        break;
                    }
                case IMethodSymbol targetSymbol:
                    {
                        if (typeMode == TypeMode.All || typeMode.HasFlag(TypeMode.MethodReturn))
                        {
                            if (targetSymbol.HasReturn())
                            {
                                if (targetSymbol.GetRealReturnType() is INamedTypeSymbol namedType && CanBeAdd(namedType, context, attributeData))
                                {
                                    namedTypeSymbols.Add(namedType);
                                }
                            }
                        }

                        if (typeMode == TypeMode.All || typeMode.HasFlag(TypeMode.MethodParameter))
                        {
                            foreach (var parameterSymbol in targetSymbol.Parameters)
                            {
                                if (parameterSymbol.Type is INamedTypeSymbol namedType && CanBeAdd(namedType, context, attributeData))
                                {
                                    namedTypeSymbols.Add(namedType);
                                }
                            }
                        }
                        break;
                    }
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 访问语法树
    /// </summary>
    /// <param name="syntaxNode"></param>
    void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is TypeDeclarationSyntax syntax)
        {
            this.m_syntaxList.Add(syntax);
        }
    }

    private static bool CanBeAdd(INamedTypeSymbol namedTypeSymbol, GeneratorExecutionContext context, AttributeData attributeData)
    {
        if (namedTypeSymbol.IsPrimitive())
        {
            return false;
        }
        if (namedTypeSymbol.IsAbstract || namedTypeSymbol.TypeKind == TypeKind.Interface)
        {
            return false;
        }

        if (!namedTypeSymbol.IsInheritFrom(PackageSyntaxReceiver.IPackageTypeName))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_FastSerialize0001, attributeData.ApplicationSyntaxReference.GetSyntax().GetLocation(), namedTypeSymbol.ToDisplayString()));
            return false;
        }
        return true;
    }
}