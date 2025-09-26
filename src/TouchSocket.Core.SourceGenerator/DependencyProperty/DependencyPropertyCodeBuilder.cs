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
using System.Text;
using TouchSocket.Core;

namespace TouchSocket;

internal class DependencyPropertyCodeBuilder : TypeCodeBuilder<INamedTypeSymbol>
{
    private readonly DependencyPropertyInfo m_dependencyPropertyInfo;

    public DependencyPropertyCodeBuilder(DependencyPropertyInfo dependencyPropertyInfo) : base(dependencyPropertyInfo.TargetType)
    {
        this.m_dependencyPropertyInfo = dependencyPropertyInfo;
    }

    public override string Id => this.m_dependencyPropertyInfo.TargetType.ToDisplayString() + this.m_dependencyPropertyInfo.Name;

    public override string GetFileName()
    {
        return $"{this.m_dependencyPropertyInfo.TargetType.Name}_{this.m_dependencyPropertyInfo.Name}_DependencyProperty.g.cs";
    }

    protected override bool GeneratorCode(StringBuilder codeBuilder)
    {
        using (this.CreateNamespace(codeBuilder))
        {
            codeBuilder.AppendLine($"public static partial class {this.m_dependencyPropertyInfo.ContainingType.Name}Extensions");
            using (this.CreateCodeSpace(codeBuilder))
            {
                var options = this.m_dependencyPropertyInfo.GenerationOptions;

                // 如果选择生成扩展属性（默认或包含属性访问器）
                if (options == PropertyGenerationOptions.All ||
                    this.HasFlag(options, PropertyGenerationOptions.IncludePropertyGetter) ||
                    this.HasFlag(options, PropertyGenerationOptions.IncludePropertySetter))
                {
                    //生成扩展属性，供依赖对象调用
                    codeBuilder.AppendLine($"extension<TDependencyObject>(TDependencyObject dependencyObject)");
                    codeBuilder.AppendLine($"where TDependencyObject:{this.m_dependencyPropertyInfo.TargetType.Name}");
                    using (this.CreateCodeSpace(codeBuilder))
                    {
                        codeBuilder.AppendLine($"/// <inheritdoc cref=\"{this.GetDependencyPropertyTypeString()}\"/>");
                        var propertyDeclaration = $"public {this.m_dependencyPropertyInfo.DependencyPropertyType.ToDisplayString()} {this.m_dependencyPropertyInfo.Name} {{";

                        // 根据选项生成getter
                        if (options == PropertyGenerationOptions.All || this.HasFlag(options, PropertyGenerationOptions.IncludePropertyGetter))
                        {
                            propertyDeclaration += $" get => dependencyObject.GetValue({this.GetDependencyPropertyTypeString()});";
                        }

                        // 根据选项生成setter
                        if (options == PropertyGenerationOptions.All || this.HasFlag(options, PropertyGenerationOptions.IncludePropertySetter))
                        {
                            propertyDeclaration += $" set => dependencyObject.SetValue({this.GetDependencyPropertyTypeString()}, value);";
                        }

                        propertyDeclaration += " }";
                        codeBuilder.AppendLine(propertyDeclaration);
                    }
                    codeBuilder.AppendLine();
                }

                // 如果选择生成方法形式的Getter（默认或包含方法Getter）
                if (options == PropertyGenerationOptions.All || this.HasFlag(options, PropertyGenerationOptions.IncludeMethodGetter))
                {
                    codeBuilder.AppendLine($"///<inheritdoc cref=\"{this.GetDependencyPropertyTypeString()}\"/>");
                    //生成扩展读取方法，供依赖对象调用
                    codeBuilder.AppendLine($"public static {this.m_dependencyPropertyInfo.DependencyPropertyType.ToDisplayString()} Get{this.m_dependencyPropertyInfo.Name}<TDependencyObject>(this TDependencyObject dependencyObject)");
                    codeBuilder.AppendLine($"where TDependencyObject:{this.m_dependencyPropertyInfo.TargetType.Name}");
                    using (this.CreateCodeSpace(codeBuilder))
                    {
                        codeBuilder.AppendLine($"return dependencyObject.GetValue({this.GetDependencyPropertyTypeString()});");
                    }
                    codeBuilder.AppendLine();
                }

                // 如果选择生成方法形式的Setter（默认或包含方法Setter）
                if (options == PropertyGenerationOptions.All || this.HasFlag(options, PropertyGenerationOptions.IncludeMethodSetter))
                {
                    //生成扩展设置方法，供依赖对象调用
                    codeBuilder.AppendLine($"///<inheritdoc cref=\"{this.GetDependencyPropertyTypeString()}\"/>");

                    if (this.m_dependencyPropertyInfo.ActionMode)
                    {
                        codeBuilder.AppendLine($"public static TDependencyObject Set{this.m_dependencyPropertyInfo.Name}<TDependencyObject>(this TDependencyObject dependencyObject, Action<{this.m_dependencyPropertyInfo.DependencyPropertyType.ToDisplayString()}> action)");

                        codeBuilder.AppendLine($"where TDependencyObject:{this.m_dependencyPropertyInfo.TargetType.Name}");

                        using (this.CreateCodeSpace(codeBuilder))
                        {
                            codeBuilder.AppendLine($"var value = new {this.m_dependencyPropertyInfo.DependencyPropertyType.ToDisplayString()}();");
                            codeBuilder.AppendLine("action(value);");
                            codeBuilder.AppendLine($"dependencyObject.SetValue({this.GetDependencyPropertyTypeString()}, value);");
                            codeBuilder.AppendLine("return dependencyObject;");
                        }
                    }
                    else
                    {
                        if (this.m_dependencyPropertyInfo.DependencyPropertyType is IArrayTypeSymbol)
                        {
                            codeBuilder.AppendLine($"public static TDependencyObject Set{this.m_dependencyPropertyInfo.Name}<TDependencyObject>(this TDependencyObject dependencyObject, params {this.m_dependencyPropertyInfo.DependencyPropertyType.ToDisplayString()} value)");
                        }
                        else
                        {
                            codeBuilder.AppendLine($"public static TDependencyObject Set{this.m_dependencyPropertyInfo.Name}<TDependencyObject>(this TDependencyObject dependencyObject, {this.m_dependencyPropertyInfo.DependencyPropertyType.ToDisplayString()} value)");
                        }


                        codeBuilder.AppendLine($"where TDependencyObject:{this.m_dependencyPropertyInfo.TargetType.Name}");

                        using (this.CreateCodeSpace(codeBuilder))
                        {
                            codeBuilder.AppendLine($"dependencyObject.SetValue({this.GetDependencyPropertyTypeString()}, value);");
                            codeBuilder.AppendLine("return dependencyObject;");
                        }
                    }

                }
            }
        }

        return true;
    }

    private bool HasFlag(PropertyGenerationOptions options, PropertyGenerationOptions flag)
    {
        return (options & flag) == flag;
    }

    private string GetDependencyPropertyTypeString()
    {
        return this.m_dependencyPropertyInfo.ContainingType.ToDisplayString() + "." + this.m_dependencyPropertyInfo.FieldName;
    }
}