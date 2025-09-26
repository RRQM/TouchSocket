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
public class DependencyPropertyGenerator : IIncrementalGenerator
{
    public const string GeneratorPropertyAttributeString = "TouchSocket.Core.GeneratorPropertyAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 注册GeneratorPropertyAttribute
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("GeneratorPropertyAttribute.g.cs", SourceText.From(generatorPropertyAttribute, Encoding.UTF8)));

        // 筛选包含GeneratorPropertyAttribute的类
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is TypeDeclarationSyntax,
                transform: static (ctx, _) => GetClassSymbol(ctx))
            .Where(static m => m is not null);

        var combined = provider.Collect();

        context.RegisterSourceOutput(combined, (spc, source) => Execute(source, spc));
    }

    private static void Execute(ImmutableArray<INamedTypeSymbol> symbols, SourceProductionContext context)
    {
        //Debugger.Launch();
        var processed = new HashSet<string>();

        foreach (var namedTypeSymbol in symbols.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>())
        {
            if (namedTypeSymbol == null)
            {
                continue;
            }

            var dependencyProperties = GetDependencyProperties(namedTypeSymbol, context);

            if (dependencyProperties.Count == 0)
            {
                continue;
            }

            var groupedByTargetType = dependencyProperties
                .GroupBy(dp => dp.TargetType, SymbolEqualityComparer.Default)
                .Cast<IGrouping<INamedTypeSymbol, DependencyPropertyInfo>>();

            foreach (var dependencyPropertyInfo in dependencyProperties)
            {
                var builder = new DependencyPropertyCodeBuilder(dependencyPropertyInfo);
                if (processed.Add(builder.Id))
                {
                    context.AddSource(builder);
                }
            }
        }
    }

    private static List<DependencyPropertyInfo> GetDependencyProperties(INamedTypeSymbol classSymbol, SourceProductionContext context)
    {
        var properties = new List<DependencyPropertyInfo>();

        // 检查字段
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is IFieldSymbol field &&
                field.HasAttribute(GeneratorPropertyAttributeString, out var attributeData))
            {
                var propertyInfo = ProcessDependencyPropertyField(field, attributeData, context);
                if (propertyInfo != null)
                {
                    properties.Add(propertyInfo);
                }
            }
            else if (member is IPropertySymbol property &&
                     property.HasAttribute(GeneratorPropertyAttributeString, out attributeData))
            {
                var propertyInfo = ProcessDependencyPropertyProperty(property, attributeData, context);
                if (propertyInfo != null)
                {
                    properties.Add(propertyInfo);
                }
            }
        }

        return properties;
    }

    private static DependencyPropertyInfo ProcessDependencyPropertyField(IFieldSymbol field, AttributeData attributeData, SourceProductionContext context)
    {
        if (!Utils.IsDependencyProperty(field.Type))
        {
            return null;
        }

        var targetType = GetTargetTypeFromAttribute(attributeData);
        // 如果未设置TargetType，则使用当前属性所在的类型
        if (targetType == null)
        {
            targetType = field.ContainingType;
        }

        var dependencyPropertyType = GetDependencyPropertyGenericType(field.Type);

        // 从字段的构造函数参数中获取属性名称
        var propertyName = GetPropertyNameFromField(field, field.Name);

        // 获取生成选项
        var generationOptions = GetGenerationOptionsFromAttribute(attributeData);

        var actionMode = GetActionModeFromAttribute(attributeData);

        return new DependencyPropertyInfo
        {
            Name = propertyName,
            ActionMode = actionMode,
            FieldName = field.Name,
            DependencyPropertyType = dependencyPropertyType,
            TargetType = targetType,
            ContainingType = field.ContainingType,
            GenerationOptions = generationOptions
        };
    }

    private static DependencyPropertyInfo ProcessDependencyPropertyProperty(IPropertySymbol property, AttributeData attributeData, SourceProductionContext context)
    {
        if (!Utils.IsDependencyProperty(property.Type))
        {
            return null;
        }

        var targetType = GetTargetTypeFromAttribute(attributeData);
        // 如果未设置TargetType，则使用当前属性所在的类型
        if (targetType == null)
        {
            targetType = property.ContainingType;
        }

        var dependencyPropertyType = GetDependencyPropertyGenericType(property.Type);

        // 从属性的构造函数参数中获取属性名称
        var propertyName = GetPropertyNameFromProperty(property, property.Name);

        // 获取生成选项
        var generationOptions = GetGenerationOptionsFromAttribute(attributeData);

        var actionMode = GetActionModeFromAttribute(attributeData);

        return new DependencyPropertyInfo
        {
            Name = propertyName,
            ActionMode = actionMode,
            FieldName = property.Name,
            DependencyPropertyType = dependencyPropertyType,
            TargetType = targetType,
            ContainingType = property.ContainingType,
            GenerationOptions = generationOptions
        };
    }

    private static PropertyGenerationOptions GetGenerationOptionsFromAttribute(AttributeData attributeData)
    {
        var namedArguments = attributeData.NamedArguments.ToDictionary(x => x.Key, x => x.Value);
        if (namedArguments.TryGetValue("GenerationOptions", out var generationOptionsValue))
        {
            if (generationOptionsValue.Value is int intValue)
            {
                return (PropertyGenerationOptions)intValue;
            }
        }

        return PropertyGenerationOptions.All;
    }

    private static bool GetActionModeFromAttribute(AttributeData attributeData)
    {
        var namedArguments = attributeData.NamedArguments.ToDictionary(x => x.Key, x => x.Value);
        if (namedArguments.TryGetValue("ActionMode", out var actionModeValue))
        {
            if (actionModeValue.Value is bool boolValue)
            {
                return boolValue;
            }
        }

        return false;
    }

    private static ITypeSymbol GetDependencyPropertyGenericType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.TypeArguments.Length > 0)
        {
            return namedType.TypeArguments[0];
        }
        return null;
    }

    private static INamedTypeSymbol GetTargetTypeFromAttribute(AttributeData attributeData)
    {
        // 然后检查命名参数 TargetType 属性
        var namedArguments = attributeData.NamedArguments.ToDictionary(x => x.Key, x => x.Value);
        if (namedArguments.TryGetValue("TargetType", out var targetTypeValue) &&
            targetTypeValue.Value is INamedTypeSymbol namedTargetType)
        {
            return namedTargetType;
        }



        // 如果都没有设置，返回 null，将在调用处使用当前类型
        return null;
    }

    private static string GetPropertyNameFromField(IFieldSymbol field, string fieldName)
    {
        // 首先尝试从字段的初始化表达式中获取属性名称
        if (field.DeclaringSyntaxReferences.Length > 0)
        {
            var syntaxReference = field.DeclaringSyntaxReferences[0];
            if (syntaxReference.GetSyntax() is VariableDeclaratorSyntax variableDeclarator)
            {
                if (variableDeclarator.Initializer?.Value is ObjectCreationExpressionSyntax objectCreation)
                {
                    if (objectCreation.ArgumentList?.Arguments.Count > 0)
                    {
                        var firstArgument = objectCreation.ArgumentList.Arguments[0];
                        if (firstArgument.Expression is LiteralExpressionSyntax literalExpression)
                        {
                            var value = literalExpression.Token.ValueText;
                            if (!string.IsNullOrEmpty(value))
                            {
                                return value;
                            }
                        }
                    }
                }
            }
        }

        // 如果无法从构造函数参数获取，则从字段名称推断
        return GetPropertyNameFromFieldName(fieldName);
    }

    private static string GetPropertyNameFromProperty(IPropertySymbol property, string propertyName)
    {
        // 首先尝试从属性的初始化表达式中获取属性名称
        if (property.DeclaringSyntaxReferences.Length > 0)
        {
            var syntaxReference = property.DeclaringSyntaxReferences[0];
            if (syntaxReference.GetSyntax() is PropertyDeclarationSyntax propertyDeclaration)
            {
                if (propertyDeclaration.Initializer?.Value is ObjectCreationExpressionSyntax objectCreation)
                {
                    if (objectCreation.ArgumentList?.Arguments.Count > 0)
                    {
                        var firstArgument = objectCreation.ArgumentList.Arguments[0];
                        if (firstArgument.Expression is LiteralExpressionSyntax literalExpression)
                        {
                            var value = literalExpression.Token.ValueText;
                            if (!string.IsNullOrEmpty(value))
                            {
                                return value;
                            }
                        }
                    }
                }
            }
        }

        // 如果无法从构造函数参数获取，则从属性名称推断
        return GetPropertyNameFromFieldName(propertyName);
    }

    private static string GetPropertyNameFromFieldName(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
        {
            return fieldName;
        }

        // 移除Property后缀
        if (fieldName.EndsWith("Property"))
        {
            return fieldName.Substring(0, fieldName.Length - "Property".Length);
        }

        return fieldName;
    }

    private static INamedTypeSymbol GetClassSymbol(GeneratorSyntaxContext context)
    {
        var typeSyntax = (TypeDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(typeSyntax) as INamedTypeSymbol;

        // 只返回包含GeneratorPropertyAttribute的类型
        if (symbol != null && HasGeneratorPropertyAttribute(symbol))
        {
            return symbol;
        }

        return null;
    }

    private static bool HasGeneratorPropertyAttribute(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if ((member is IFieldSymbol || member is IPropertySymbol) &&
                member.HasAttribute(GeneratorPropertyAttributeString))
            {
                return true;
            }
        }
        return false;
    }

    private const string generatorPropertyAttribute = @"

/*
此代码由SourceGenerator工具直接生成，非必要请不要修改此处代码
*/

#pragma warning disable

using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 标识源生成依赖属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    /*GeneratedCode*/
    internal class GeneratorPropertyAttribute : Attribute
    {
        /// <summary>
        /// 要生成依赖属性的目标类型名称。如果未设置，则表示当前属性所在的类型。
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// 生成的属性类型。
        /// </summary>
        public PropertyGenerationOptions GenerationOptions { get; set; } = PropertyGenerationOptions.All;

        /// <summary>
        /// 是否使用Action模式生成Set扩展方法访问器。
        /// </summary>
        public bool ActionMode { get; set; }
    }

    /// <summary>
    /// 依赖属性生成选项。
    /// </summary>
    [Flags]
    enum PropertyGenerationOptions
    {
        /// <summary>
        /// 生成所有访问器。
        /// </summary>
        All = 0,
        /// <summary>
        /// 包含方法形式的 Getter。
        /// </summary>
        IncludeMethodGetter = 1,
        /// <summary>
        /// 包含方法形式的 Setter。
        /// </summary>
        IncludeMethodSetter = 2,
        /// <summary>
        /// 包含属性形式的 Getter。
        /// </summary>
        IncludePropertyGetter = 4,
        /// <summary>
        /// 包含属性形式的 Setter。
        /// </summary>
        IncludePropertySetter = 8
    }
}
";
}

/// <summary>
/// 依赖属性信息
/// </summary>
internal class DependencyPropertyInfo
{
    public string Name { get; set; }
    public bool ActionMode { get; set; }
    public string FieldName { get; set; }
    public ITypeSymbol DependencyPropertyType { get; set; }
    public INamedTypeSymbol TargetType { get; set; }
    public INamedTypeSymbol ContainingType { get; set; }
    public PropertyGenerationOptions GenerationOptions { get; set; } = PropertyGenerationOptions.All;
}