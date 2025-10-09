//// ------------------------------------------------------------------------------
//// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//// CSDN博客：https://blog.csdn.net/qq_40374647
//// 哔哩哔哩视频：https://space.bilibili.com/94253567
//// Gitee源代码仓库：https://gitee.com/RRQM_Home
//// Github源代码仓库：https://github.com/RRQM
//// API首页：https://touchsocket.net/
//// 交流QQ群：234762506
//// 感谢您的下载和使用
//// ------------------------------------------------------------------------------

//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using System.Collections.Generic;

//namespace TouchSocket;

//[Generator]
//public class LanguageVersionSourceGenerator : IIncrementalGenerator
//{
//    public void Initialize(IncrementalGeneratorInitializationContext context)
//    {
//        context.RegisterSourceOutput(context.ParseOptionsProvider, (spc, parseOptions) =>
//        {
//            // 检测C#语言版本并生成预编译定义
//            var languageVersionDefines = GenerateLanguageVersionDefines(parseOptions);
//            if (!string.IsNullOrEmpty(languageVersionDefines))
//            {
//                spc.AddSource("GeneratorLanguageVersionDefines.g.cs", languageVersionDefines);
//            }
//        });
//    }

//    /// <summary>
//    /// 根据解析选项生成C#语言版本的预编译定义
//    /// </summary>
//    /// <param name="parseOptions">解析选项</param>
//    /// <returns>预编译定义的源代码</returns>
//    private static string GenerateLanguageVersionDefines(ParseOptions parseOptions)
//    {
//        if (parseOptions is not CSharpParseOptions csharpOptions)
//        {
//            return null;
//        }

//        var languageVersion = csharpOptions.LanguageVersion;
//        var defines = new List<string>();

//        if (languageVersion >= LanguageVersion.CSharp1)
//        {
//            defines.Add("#define CSHARP1_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp2)
//        {
//            defines.Add("#define CSHARP2_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp3)
//        {
//            defines.Add("#define CSHARP3_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp4)
//        {
//            defines.Add("#define CSHARP4_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp5)
//        {
//            defines.Add("#define CSHARP5_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp6)
//        {
//            defines.Add("#define CSHARP6_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp7)
//        {
//            defines.Add("#define CSHARP7_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp7_1)
//        {
//            defines.Add("#define CSHARP7_1_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp7_2)
//        {
//            defines.Add("#define CSHARP7_2_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp7_3)
//        {
//            defines.Add("#define CSHARP7_3_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp8)
//        {
//            defines.Add("#define CSHARP8_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp9)
//        {
//            defines.Add("#define CSHARP9_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp10)
//        {
//            defines.Add("#define CSHARP10_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp11)
//        {
//            defines.Add("#define CSHARP11_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp12)
//        {
//            defines.Add("#define CSHARP12_OR_GREATER");
//        }
//        if (languageVersion >= LanguageVersion.CSharp13)
//        {
//            defines.Add("#define CSHARP13_OR_GREATER");
//        }
//        if (defines.Count == 0)
//        {
//            return null;
//        }

//        return string.Join("\r\n", defines) + "\r\n";
//    }
//}
