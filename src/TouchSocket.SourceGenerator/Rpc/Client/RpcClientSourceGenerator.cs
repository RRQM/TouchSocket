using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace TouchSocket
{
    [Generator]
    public class RpcClientSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new RpcClientSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var s = context.Compilation.GetMetadataReference(context.Compilation.Assembly);

            if (context.SyntaxReceiver is RpcClientSyntaxReceiver receiver)
            {
                var builders = receiver
                    .GetRpcApiTypes(context.Compilation)
                    .Select(i => new RpcClientCodeBuilder(i))
                    .Distinct();
                //Debugger.Launch();
                foreach (var builder in builders)
                {
                    var tree = CSharpSyntaxTree.ParseText(builder.ToSourceText());
                    var root = tree.GetRoot().NormalizeWhitespace();
                    var ret = root.ToFullString();
                    context.AddSource($"{builder.GetFileName()}.g.cs", ret);
                }
            }
        }
    }
}