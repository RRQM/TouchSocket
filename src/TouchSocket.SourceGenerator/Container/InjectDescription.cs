using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace TouchSocket
{
    internal class InjectDescription
    {
        public INamedTypeSymbol From { get; set; }
        public INamedTypeSymbol To { get; set; }
        public string Key { get; set; }
    }

    internal class InjectPropertyDescription
    {
        public ITypeSymbol Type { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
    }

    internal class InjectMethodDescription
    {
        public IEnumerable<InjectPropertyDescription> Types { get; set; }
        public string Name { get; set; }
    }
}