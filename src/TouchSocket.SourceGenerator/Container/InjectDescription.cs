using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace TouchSocket
{
    class InjectDescription
    {
        public INamedTypeSymbol From { get; set; }
        public INamedTypeSymbol To { get; set; }
        public string Key { get; set; }
    }
    class InjectPropertyDescription
    {
        public ITypeSymbol Type { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
    }

    class InjectMethodDescription
    {
        public IEnumerable<InjectPropertyDescription> Types { get; set; }
        public string Name { get; set; }
    }
}
