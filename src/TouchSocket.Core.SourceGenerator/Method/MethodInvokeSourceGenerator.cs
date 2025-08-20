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
using System.Collections.Generic;
using System.Linq;

namespace TouchSocket;

[Generator]
public class MethodInvokeSourceGenerator : IIncrementalGenerator
{
    public const string DynamicMethod = "TouchSocket.Core.DynamicMethodAttribute";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. 注册语法提供器来捕获类型声明
        var typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is TypeDeclarationSyntax,
                transform: static (ctx, _) => (TypeDeclarationSyntax)ctx.Node);

        // 2. 组合编译和类型信息
        var compilationAndTypes = context.CompilationProvider.Combine(typeDeclarations.Collect());

        // 3. 注册生成逻辑
        context.RegisterSourceOutput(compilationAndTypes, (spc, source) =>
            Execute(source.Left, source.Right, spc));
    }

    private static void Execute(
        Compilation compilation,
        IEnumerable<TypeDeclarationSyntax> typeDeclaration,
        SourceProductionContext context)
    {
        var pairs = new Dictionary<INamedTypeSymbol, List<IMethodSymbol>>(SymbolEqualityComparer.Default);

        foreach (var typeSyntax in typeDeclaration.Distinct())
        {
            var model = compilation.GetSemanticModel(typeSyntax.SyntaxTree);
            var typeSymbol = model.GetDeclaredSymbol(typeSyntax) as INamedTypeSymbol;

            if (typeSymbol == null || typeSymbol.IsGenericType)
            {
                continue;
            }

            // 3. 检查类型和方法是否应用了DynamicMethod属性
            var methods = GetDynamicMethods(typeSymbol);
            if (methods.Any())
            {
                pairs[typeSymbol] = methods.ToList();
            }
        }

        // 4. 生成源代码
        foreach (var pair in pairs)
        {
            var builder = new MethodInvokeCodeBuilder(pair.Key, compilation, pair.Value);
            context.AddSource(builder);

            //Debugger.Launch();
            var methodInvokeClassCodeBuilder = new MethodInvokeClassCodeBuilder(pair.Key, compilation, pair.Value);
            context.AddSource(methodInvokeClassCodeBuilder);

            var titleBuilder = new MethodInvokeTitleCodeBuilder(pair.Key);
            context.AddSource(titleBuilder);


            //var returnTypeInvokeCodeBuilder = new ReturnTypeInvokeCodeBuilder(pair.Key, compilation, pair.Value.Select(a => a.ReturnType).Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>());
            //context.AddSource(returnTypeInvokeCodeBuilder);
        }
    }

    private static IEnumerable<IMethodSymbol> GetDynamicMethods(INamedTypeSymbol typeSymbol)
    {
        var hasTypeAttribute = typeSymbol.HasAttribute(MethodInvokeSourceGenerator.DynamicMethod);

        return typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary &&
                      !m.IsGenericMethod &&
                      IsAccessible(m.DeclaredAccessibility) &&
                      (hasTypeAttribute || HasMethodAttribute(m)));
    }

    private static bool IsAccessible(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Internal or
            Accessibility.ProtectedAndInternal or
            Accessibility.Public => true,
            _ => false
        };
    }

    private static bool HasMethodAttribute(IMethodSymbol method) =>
        method.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == DynamicMethod ||
            a.AttributeClass?.GetAttributes().Any(ad =>
                ad.AttributeClass?.ToDisplayString() == DynamicMethod) == true);
}