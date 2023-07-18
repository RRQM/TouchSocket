using Microsoft.CodeAnalysis;
using System.Linq;

namespace TouchSocket.SourceGenerator.Rpc
{
    /// <summary>
    /// HttpApi代码生成器
    /// </summary>
    [Generator]
    public class RpcSourceGenerator : ISourceGenerator
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new RpcSyntaxReceiver());
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        public void Execute(GeneratorExecutionContext context)
        {
            var s = context.Compilation.GetMetadataReference(context.Compilation.Assembly);

            if (context.SyntaxReceiver is RpcSyntaxReceiver receiver)
            {
                var builders = receiver
                    .GetRpcApiTypes(context.Compilation)
                    .Select(i => new RpcCodeBuilder(i))
                    .Distinct();
                //Debugger.Launch();
                foreach (var builder in builders)
                {
                    context.AddSource($"{builder.GetFileName()}.g.cs", builder.ToSourceText());
                }
            }
        }
    }
}
