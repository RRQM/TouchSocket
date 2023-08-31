using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace TouchSocket
{
    internal static class Utils
    {
        public static bool IsInheritFrom(this ITypeSymbol typeSymbol,string baseType)
        {
            if (typeSymbol.BaseType==null)
            {
                return false;
            }

            if (typeSymbol.BaseType.ToDisplayString()==baseType)
            {
                return true;
            }

            return IsInheritFrom(typeSymbol.BaseType,baseType);
        }
    }
}
