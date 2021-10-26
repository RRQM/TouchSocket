//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
#if NET45_OR_GREATER

using Microsoft.CSharp;
using RRQMCore.Log;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC编译
    /// </summary>
    public static class RpcCompiler
    {
        private static List<string> RefStrings = new List<string>();

        /// <summary>
        /// 添加编译引用
        /// </summary>
        /// <param name="refPath"></param>
        public static void AddRef(string refPath)
        {
            if (!RefStrings.Contains(refPath))
            {
                RefStrings.Add(refPath);
            }
        }

        /// <summary>
        ///  添加编译引用
        /// </summary>
        /// <param name="type"></param>
        public static void AddRef(Type type)
        {
            AddRef(type.Assembly.FullName);
        }

        /// <summary>
        /// 清除引用
        /// </summary>
        public static void ClearRef()
        {
            RefStrings.Clear();
        }

        /// <summary>
        /// 编译代码
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="codes"></param>
        /// <returns></returns>
        public static void CompileCode(string fullPath, string[] codes)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters compilerParameters = new CompilerParameters();

            compilerParameters.OutputAssembly = fullPath;

            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = false;
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParameters.ReferencedAssemblies.Add(typeof(ILog).Assembly.Location);
            compilerParameters.ReferencedAssemblies.Add(typeof(RPCService).Assembly.Location);

            foreach (var item in RefStrings)
            {
                compilerParameters.ReferencedAssemblies.Add(item);
            }

            CompilerResults cr = codeProvider.CompileAssemblyFromSource(compilerParameters, codes);
            if (cr.Errors.HasErrors)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (CompilerError err in cr.Errors)
                {
                    stringBuilder.AppendLine(err.ErrorText);
                }

                throw new RRQMRPCException(stringBuilder.ToString());
            }
        }
    }
}

#endif