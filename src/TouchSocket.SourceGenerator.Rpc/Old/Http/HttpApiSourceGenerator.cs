//using Microsoft.CodeAnalysis;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;

//namespace TouchSocket.SourceGenerator.Rpc.Http
//{
//    /// <summary>
//    /// HttpApi代码生成器
//    /// </summary>
//    [Generator]
//    public class HttpApiSourceGenerator : ISourceGenerator
//    {
//        /// <summary>
//        /// 初始化
//        /// </summary>
//        /// <param name="context"></param>
//        public void Initialize(GeneratorInitializationContext context)
//        {
//            context.RegisterForSyntaxNotifications(() => new HttpApiSyntaxReceiver());
//        }

//        /// <summary>
//        /// 执行
//        /// </summary>
//        /// <param name="context"></param>
//        public void Execute(GeneratorExecutionContext context)
//        {
//            //Debugger.Launch();
//            var s = context.Compilation.GetMetadataReference(context.Compilation.Assembly);

//            if (context.SyntaxReceiver is HttpApiSyntaxReceiver receiver)
//            {
//                var builders = receiver
//                    .GetHttpApiTypes(context.Compilation)
//                    .Select(i => new HttpApiCodeBuilder(i))
//                    .Distinct();

//                foreach (var builder in builders)
//                {
//                    context.AddSource(builder.HttpApiTypeName, builder.ToSourceText());
//                }
//            }
//        }
//    }
//}
