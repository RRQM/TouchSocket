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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TouchSocket;

internal static class Utils
{
    public const string GeneratorPackageAttributeTypeName = "TouchSocket.Core.GeneratorPackageAttribute";
    public const string IPackageTypeName = "TouchSocket.Core.IPackage";
    public const string Task = "System.Threading.Tasks.Task";
    public const string DependencyPropertyBase = "TouchSocket.Core.DependencyPropertyBase";

    public static bool EqualsWithFullName(this ISymbol symbol, string fullName)
    {
        return symbol.ToDisplayString().Equals(fullName);
    }

    public static bool IsDependencyProperty(ITypeSymbol typeSymbol)
    {
        return typeSymbol.IsInheritFrom(Utils.DependencyPropertyBase);
    }

    public static string GetAccessModifierString(this Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => string.Empty,
        };
    }

    public static bool IsReadOnlyMemory(this ITypeSymbol typeSymbol, out ITypeSymbol elementType)
    {
        elementType = null;

        // 确保是命名类型符号
        if (typeSymbol is not INamedTypeSymbol fieldType)
            return false;

        // 检查原始定义的元数据名称和命名空间
        var originalDefinition = fieldType.OriginalDefinition;
        if (originalDefinition.MetadataName != "ReadOnlyMemory`1" ||
            originalDefinition.ContainingNamespace?.ToDisplayString() != "System")
        {
            return false;
        }

        // 验证类型参数数量并获取元素类型
        if (fieldType.TypeArguments.Length == 1)
        {
            elementType = fieldType.TypeArguments[0];
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取方法的确定性名称，即使在重载时，也能区分。
    /// <para>计算规则是：方法名_参数类型名称</para>
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    public static string GetDeterminantName(this IMethodSymbol methodInfo)
    {
        return GenerateKey(methodInfo);
    }

    public static string GetGeneratedCodeString()
    {
        return $"[global::System.CodeDom.Compiler.GeneratedCode(\"{Assembly.GetExecutingAssembly().FullName}\", \"{Assembly.GetExecutingAssembly().GetName().Version}\")]";
    }

    //public static ITypeSymbol GetNullableType(this ITypeSymbol typeSymbol)
    //{

    //    //Debugger.Launch();
    //    var namedTypeSymbol = (INamedTypeSymbol)typeSymbol;

    //    if (namedTypeSymbol.SpecialType == SpecialType.System_Nullable_T)
    //    {
    //        return namedTypeSymbol.TypeArguments.First();
    //    }

    //    if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
    //    {
    //        if (namedTypeSymbol.TypeArguments.Any())
    //        {
    //            return namedTypeSymbol.TypeArguments.First();
    //        }
    //        else
    //        {
    //            return namedTypeSymbol.OriginalDefinition;
    //        }
    //    }

    //    return namedTypeSymbol;
    //}

    public static ITypeSymbol GetNullableType(this ITypeSymbol typeSymbol)
    {
        // 如果是可空值类型（Nullable<T>），返回T
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedTypeSymbol.TypeArguments.Length == 1 ? namedTypeSymbol.TypeArguments[0] : typeSymbol;
        }

        // 如果是可空引用类型（T?），去除可空标注，返回T
        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return typeSymbol.WithNullableAnnotation(NullableAnnotation.None);
        }

        // 其他情况直接返回原类型
        return typeSymbol;
    }

    public static string GetTypeofString(this ITypeSymbol typeSymbol)
    {
        var type = typeSymbol.GetNullableType();
        if (type is IDynamicTypeSymbol)
        {
            return typeof(object).ToString();
        }
        return type.ToDisplayString();
    }

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attribute)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass != null && attrClass.ToDisplayString() == attribute.ToDisplayString())
            {
                return true;
            }
        }
        return false;
    }

    public static bool HasAttribute(this ISymbol symbol, string attribute)
    {
        return symbol.HasAttribute(attribute, out _);
    }

    public static bool HasAttribute(this ISymbol symbol, string attribute, out AttributeData attributeData)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass != null && attrClass.ToDisplayString() == attribute)
            {
                attributeData = attr;
                return true;
            }
        }
        attributeData = default;
        return false;
    }

    public static bool HasAttributes(this ISymbol symbol, string attribute, out IEnumerable<AttributeData> attributeDatas)
    {
        var list = new List<AttributeData>();
        foreach (var attr in symbol.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass != null && attrClass.ToDisplayString() == attribute)
            {
                list.Add(attr);
            }
        }

        if (list.Count > 0)
        {
            attributeDatas = list;
            return true;
        }
        attributeDatas = default;
        return false;
    }

    #region 注释
    public static IEnumerable<string> GetXmlSummary(this ISymbol symbol)
    {
        var xmlDoc = symbol.GetDocumentationCommentXml();
        if (string.IsNullOrWhiteSpace(xmlDoc))
        {
            return [];
        }

        return ExtractSummaryFromXml(xmlDoc);
    }

    private static IEnumerable<string> ExtractSummaryFromXml(string xmlDoc)
    {
        try
        {
            var parsed = XElement.Parse(xmlDoc);
            var summaryElement = parsed.Element("summary");

            if (summaryElement != null)
            {
                return summaryElement.Value
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line));
            }
            return [];
        }
        catch
        {
            return [];
        }
    }

    #endregion

    public static bool HasFlags(int value, int flag)
    {
        return (value & flag) == flag;
    }

    public static bool HasReturn(this IMethodSymbol method)
    {
        if (method.ReturnsVoid || method.ReturnType.ToDisplayString() == typeof(Task).FullName || method.ReturnType.ToDisplayString() == typeof(ValueTask).FullName)
        {
            return false;
        }
        return true;
    }

    public static INamedTypeSymbol GetRealReturnType(this IMethodSymbol method)
    {
        if (!method.HasReturn())
        {
            return default;
        }

        if (method.ReturnType is not INamedTypeSymbol returnTypeSymbol)
        {
            return default;
        }

        if (returnTypeSymbol.IsGenericType && returnTypeSymbol.IsInheritFrom(Utils.Task))
        {
            returnTypeSymbol = returnTypeSymbol.TypeArguments[0] as INamedTypeSymbol;
        }

        if (returnTypeSymbol.IsValueType)
        {
            return returnTypeSymbol;
        }
        else
        {
            return returnTypeSymbol.GetNullableType() as INamedTypeSymbol;
        }
    }

    public static bool IsInheritFrom(this ITypeSymbol typeSymbol, string baseType)
    {
        if (typeSymbol.ToDisplayString() == baseType)
        {
            return true;
        }

        if (typeSymbol.BaseType != null)
        {
            var b = IsInheritFrom(typeSymbol.BaseType, baseType);
            if (b)
            {
                return true;
            }
        }

        foreach (var item in typeSymbol.AllInterfaces)
        {
            var b = IsInheritFrom(item, baseType);
            if (b)
            {
                return true;
            }
        }

        return false;
    }

    public static string MakeIdentifier(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // 替换非法字符，允许 Unicode 字母、数字和下划线
        var result = Regex.Replace(input, @"[^\p{L}\p{N}_]", "_");

        // 如果结果以数字开头，则添加前缀 _
        if (char.IsDigit(result.First()))
        {
            result = "_" + result;
        }

        return result;
    }

    public static string RenameCamelCase(this string str)
    {
        var firstChar = str[0];

        if (firstChar == char.ToLowerInvariant(firstChar))
        {
            return str;
        }

        var name = str.ToCharArray();
        name[0] = char.ToLowerInvariant(firstChar);

        return new string(name);
    }

    private static string GenerateKey(IMethodSymbol method)
    {
        var parameterTypes = method.Parameters.Select(p => p.Type).ToArray(); // 使用类型名称
        return GenerateKey(method.Name, parameterTypes);
    }

    private static string GenerateKey(string methodName, ITypeSymbol[] parameterTypes)
    {
        // 将参数类型名称转换为合法的标识符
        var parameterTypeNames = string.Join("_", parameterTypes.Select(t => MakeIdentifier(t.Name)));
        return $"{MakeIdentifier(methodName)}_{parameterTypeNames}";
    }

    public static bool IsUnmanagedType(this ITypeSymbol type)
    {
        // 使用HashSet防止递归循环引用
        var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        return IsUnmanagedTypeCore(type, visited);
    }

    private static bool IsUnmanagedTypeCore(ITypeSymbol type, HashSet<ITypeSymbol> visited)
    {
        // 1. 基本值类型
        if (type.IsPrimitiveType())
            return true;

        // 2. 枚举类型
        if (type.TypeKind == TypeKind.Enum)
            return true;

        // 3. 指针类型
        if (type is IPointerTypeSymbol)
            return true;

        // 4. 类型参数（带unmanaged约束）
        if (type is ITypeParameterSymbol typeParam && typeParam.HasUnmanagedTypeConstraint)
            return true;

        // 5. 结构体（递归检查字段）
        if (type.IsValueType && type.TypeKind == TypeKind.Struct)
        {
            // 防止递归循环
            if (!visited.Add(type))
                return false;

            try
            {
                foreach (var member in type.GetMembers())
                {
                    if (member is IFieldSymbol field && !field.IsStatic)
                    {
                        // 5.1 固定缓冲区处理
                        if (field.IsFixedSizeBuffer)
                        {
                            var bufferType = (IArrayTypeSymbol)field.Type;
                            if (!IsUnmanagedTypeCore(bufferType.ElementType, visited))
                                return false;
                        }
                        // 5.2 常规字段处理
                        else if (!IsUnmanagedTypeCore(field.Type, visited))
                        {
                            return false;
                        }
                    }
                }
                return true; // 所有字段均满足非托管条件
            }
            finally
            {
                visited.Remove(type);
            }
        }

        return false;
    }

    private static bool IsPrimitiveType(this ITypeSymbol type)
    {
        var specialType = type.SpecialType;
        return specialType switch
        {
            SpecialType.System_SByte => true,
            SpecialType.System_Byte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Char => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_Decimal => true,
            SpecialType.System_Boolean => true,
            SpecialType.System_IntPtr => true,
            SpecialType.System_UIntPtr => true,
            _ => false
        };
    }

    #region 类型判断

    public static bool IsDictionary(this INamedTypeSymbol namedTypeSymbol)
    {
        //Debugger.Launch();
        if (!namedTypeSymbol.IsGenericType)
        {
            return false;
        }
        if (namedTypeSymbol.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.Dictionary<TKey, TValue>")
        {
            return true;
        }
        return false;
    }

    public static bool IsArray(this ITypeSymbol namedTypeSymbol)
    {
        return namedTypeSymbol.TypeKind == TypeKind.Array;
    }

    public static bool IsList(this INamedTypeSymbol namedTypeSymbol)
    {
        //Debugger.Launch();
        if (!namedTypeSymbol.IsGenericType)
        {
            return false;
        }
        if (namedTypeSymbol.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.List<T>")
        {
            return true;
        }
        return false;
    }

    public static bool IsPrimitiveAndString(this ITypeSymbol typeSymbol)
    {
        switch (typeSymbol.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Char:
            case SpecialType.System_SByte:

            case SpecialType.System_Byte:

            case SpecialType.System_Int16:

            case SpecialType.System_UInt16:

            case SpecialType.System_Int32:

            case SpecialType.System_UInt32:

            case SpecialType.System_Int64:

            case SpecialType.System_UInt64:

            case SpecialType.System_Decimal:

            case SpecialType.System_Single:

            case SpecialType.System_Double:

            case SpecialType.System_String:
            case SpecialType.System_DateTime:
                return true;

            case SpecialType.System_Nullable_T:
                return typeSymbol.GetNullableType().IsPrimitiveAndString();

            default:
                return false;
        }
    }

    public static bool IsString(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType == SpecialType.System_String;
    }

    public static bool IsVoid(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType == SpecialType.System_Void;
    }

    public static bool IsTimeSpan(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.IsInheritFrom(typeof(TimeSpan).ToString());
    }

    public static bool IsGuid(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.IsInheritFrom(typeof(Guid).ToString());
    }

    #endregion 类型判断
}
internal abstract class TypeCodeBuilder<T> : CodeBuilder where T : class, ITypeSymbol
{
    public TypeCodeBuilder(T typeSymbol)
    {
        this.TypeSymbol = typeSymbol;
    }
    public override string Id => this.TypeSymbol.ToDisplayString();

    public T TypeSymbol { get; }

    public override string GetFileName()
    {
        return $"{this.Id}.g.cs";
    }

    protected CodeSpace CreateNamespace(StringBuilder codeBuilder)
    {
        if (this.TypeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            return new CodeSpace();
        }

        codeBuilder.AppendLine($"namespace {this.TypeSymbol.ContainingNamespace}");
        return new CodeSpace(codeBuilder);
    }

    protected CodeSpace CreateNamespace(StringBuilder codeBuilder, string namespaceString)
    {
        codeBuilder.AppendLine($"namespace {namespaceString}");
        return new CodeSpace(codeBuilder);
    }

    protected CodeSpace CreateNamespaceIfNotGlobalNamespace(StringBuilder codeBuilder, string namespaceString)
    {
        if (this.TypeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            return new CodeSpace();
        }

        codeBuilder.AppendLine($"namespace {namespaceString}");
        return new CodeSpace(codeBuilder);
    }
}
internal abstract class CodeBuilder
{
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

    public abstract string Id { get; }

    public abstract string GetFileName();

    protected abstract bool GeneratorCode(StringBuilder codeBuilder);

    public bool ToSourceText(out string sourceCode)
    {
        var codeBuilder = this.CreateCodeStringBuilder();

        if (this.GeneratorCode(codeBuilder))
        {
            codeBuilder = ReplaceGeneratedCode(codeBuilder);

            var tree = CSharpSyntaxTree.ParseText(codeBuilder.ToString());
            var root = tree.GetRoot().NormalizeWhitespace();
            var ret = root.ToFullString();

            sourceCode = ret;
            return true;
        }

        sourceCode = null;
        return false;
    }

    public static string ReplaceGeneratedCode(string code)
    {
        return code.Replace("/*GeneratedCode*/", $"[global::System.CodeDom.Compiler.GeneratedCode(\"TouchSocket.SourceGenerator\",\"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}\")]");
    }

    public static StringBuilder ReplaceGeneratedCode(StringBuilder codeBuilder)
    {
        return codeBuilder.Replace("/*GeneratedCode*/", $"[global::System.CodeDom.Compiler.GeneratedCode(\"TouchSocket.SourceGenerator\",\"{Assembly.GetExecutingAssembly().GetName().Version.ToString()}\")]");
    }

    protected StringBuilder CreateCodeStringBuilder()
    {
        var codeBuilder = new StringBuilder();
        codeBuilder.AppendLine("/*");
        codeBuilder.AppendLine("此代码由工具直接生成，非必要请不要修改此处代码");
        codeBuilder.AppendLine("*/");
        codeBuilder.AppendLine("#pragma warning disable");

        foreach (var item in this.Usings)
        {
            codeBuilder.AppendLine(item);
        }

        return codeBuilder;
    }

    protected CodeSpace CreateCodeSpace(StringBuilder codeBuilder)
    {
        return new CodeSpace(codeBuilder);
    }
}

public readonly struct CodeSpace : IDisposable
{
    private readonly StringBuilder m_stringBuilder;

    public CodeSpace(StringBuilder stringBuilder)
    {
        this.m_stringBuilder = stringBuilder;
        this.m_stringBuilder.AppendLine("{");
    }
    public void Dispose()
    {
        this.m_stringBuilder?.AppendLine("}");
    }
}

internal class CodeBuilderEqualityComparer<T> : IEqualityComparer<T> where T : CodeBuilder
{
    public static CodeBuilderEqualityComparer<T> Default { get; } = new CodeBuilderEqualityComparer<T>();

    public bool Equals(T x, T y)
    {
        return x.Id.Equals(y.Id);
    }

    public int GetHashCode(T obj)
    {
        return obj.Id.GetHashCode();
    }
}

internal static class SourceProductionContextExtension
{
    public static void AddSource(this SourceProductionContext sourceProductionContext, CodeBuilder builder)
    {
        if (builder.ToSourceText(out var sourceCode))
        {
            sourceProductionContext.AddSource(builder.GetFileName(), sourceCode);
        }
    }
}

public enum EndianType
{
    /// <summary>
    /// 小端模式，即DCBA
    /// </summary>
    Little,

    /// <summary>
    /// 大端模式。即ABCD
    /// </summary>
    Big,

    /// <summary>
    /// 以交换小端格式。即CDAB
    /// </summary>
    LittleSwap,

    /// <summary>
    /// 以交换大端，即：BADC
    /// </summary>
    BigSwap
}