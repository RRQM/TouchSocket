using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace TouchSocket
{
    internal static class Utils
    {
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
    }
}