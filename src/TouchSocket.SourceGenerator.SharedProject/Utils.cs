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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TouchSocket;

internal static class Utils
{
    public const string Task = "System.Threading.Tasks.Task";

    public static bool EqualsWithFullName(this ISymbol symbol, string fullName)
    {
        return symbol.ToDisplayString().Equals(fullName);
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

    public static ITypeSymbol GetNullableType(this ITypeSymbol typeSymbol)
    {
        //Debugger.Launch();
        var namedTypeSymbol = (INamedTypeSymbol)typeSymbol;

        if (namedTypeSymbol.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedTypeSymbol.TypeArguments.First();
        }

        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            if (namedTypeSymbol.TypeArguments.Any())
            {
                return namedTypeSymbol.TypeArguments.First();
            }
            else
            {
                return namedTypeSymbol.OriginalDefinition;
            }
        }

        return namedTypeSymbol;
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


    public static bool HasFlags(int value, int flag)
    {
        return (value & flag) == flag;
    }

    public static bool HasReturn(this IMethodSymbol method)
    {
        if (method.ReturnsVoid || method.ReturnType.ToDisplayString() == typeof(Task).FullName)
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

    public static bool IsPrimitive(this ITypeSymbol typeSymbol)
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
                return typeSymbol.GetNullableType().IsPrimitive();

            default:
                return false;
        }
    }

    public static bool IsString(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType == SpecialType.System_String;
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

internal abstract class CodeBuilder
{
    public abstract string Id { get; }

    public abstract string GetFileName();

    public string ToSourceText()
    {
        var code = this.ToString();
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot().NormalizeWhitespace();
        var ret = root.ToFullString();
        return ret;
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