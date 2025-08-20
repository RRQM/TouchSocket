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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using TouchSocket.Core;

namespace TouchSocket;

[Generator]
public class FastSerializeGenerator : IIncrementalGenerator
{
    public const string FastSerializableAttributeString = "TouchSocket.Core.FastSerializableAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 注册FastSerializableAttribute
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("FastSerializableAttribute.g.cs", SourceText.From(fastSerializableAttribute, Encoding.UTF8)));

        // 筛选包含FastSerializableAttribute的类
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is TypeDeclarationSyntax tds
                    && tds.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetClassSymbol(ctx))
            .Where(static m => m is not null);

        var combined = provider.Collect();

        context.RegisterSourceOutput(combined, (spc, source) => Execute(source, spc));
    }

    private static void Execute(ImmutableArray<INamedTypeSymbol> symbols, SourceProductionContext context)
    {
        var processed = new HashSet<string>();

        foreach (var namedTypeSymbol in symbols.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>())
        {
            if (!namedTypeSymbol.HasAttributes(FastSerializableAttributeString, out var atts))
                continue;

            var pairs = ProcessAttributes(namedTypeSymbol, atts, context);
            foreach (var pair in pairs)
            {
                var builder = new FastSerializeCodeBuilder(pair.Key, pair.Value);
                if (processed.Add(builder.Id))
                {
                    context.AddSource(builder);
                }
            }
        }
    }

    private static Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> ProcessAttributes(
        INamedTypeSymbol classSymbol,
        IEnumerable<AttributeData> attributes,
        SourceProductionContext context)
    {
        var pairs = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(SymbolEqualityComparer.Default);
        var types = new List<INamedTypeSymbol>();

        foreach (var item in attributes)
        {
            var args = item.ConstructorArguments;
            if (args.Length == 0) continue;

            var typeSymbol = args[0].Value as INamedTypeSymbol;
            if (typeSymbol == null) continue;

            var typeMode = args.Length > 1 && args[1].Value is int mode
                ? (TypeMode)mode
                : TypeMode.Self;

            CollectTypes(typeSymbol, typeMode, types, context, item);
        }

        pairs[classSymbol] = types.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>().ToList();
        return pairs;
    }

    private static bool CanBeAdd(INamedTypeSymbol namedTypeSymbol, SourceProductionContext context, AttributeData attributeData)
    {
        if (namedTypeSymbol.IsPrimitiveAndString())
        {
            return false;
        }
        if (namedTypeSymbol.IsAbstract || namedTypeSymbol.TypeKind == TypeKind.Interface)
        {
            return false;
        }

        if (!namedTypeSymbol.IsInheritFrom(Utils.IPackageTypeName))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_FastSerialize0001, attributeData.ApplicationSyntaxReference.GetSyntax().GetLocation(), namedTypeSymbol.ToDisplayString()));
            return false;
        }
        return true;
    }

    private static void CollectTypes(
        INamedTypeSymbol namedTypeSymbol,
        TypeMode typeMode,
        List<INamedTypeSymbol> namedTypeSymbols,
        SourceProductionContext context,
        AttributeData attributeData)
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

    private static INamedTypeSymbol GetClassSymbol(GeneratorSyntaxContext context)
    {
        var typeSyntax = (TypeDeclarationSyntax)context.Node;
        return context.SemanticModel.GetDeclaredSymbol(typeSyntax) as INamedTypeSymbol;
    }

    // 保持原有fastSerializableAttribute字符串
    private const string fastSerializableAttribute = @"
/*
此代码由SourceGenerator工具直接生成，非必要请不要修改此处代码
*/

#pragma warning disable

using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 标识源生成Fast序列化相关的实现。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =true)]
    /*GeneratedCode*/
    internal class FastSerializableAttribute : Attribute
    {
        public FastSerializableAttribute(Type type, TypeMode typeMode)
        {
            this.Type = type;
            this.TypeMode = typeMode;
        }

        public Type Type { get; }

        public FastSerializableAttribute(Type type) : this(type, TypeMode.Self)
        {

        }

        public TypeMode TypeMode { get; }
    }

    [Flags]
    /*GeneratedCode*/
    internal enum TypeMode
    {
        All = -1,
        Self = 0,
        Field = 1,
        Property = 2,
        MethodReturn = 4,
        MethodParameter = 8
    }
}

";
}