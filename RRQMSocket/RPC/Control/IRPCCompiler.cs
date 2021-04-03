using System.Collections.Generic;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC编译器
    /// </summary>
    public interface IRPCCompiler
    {
        /// <summary>
        /// 编译代码
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="codes"></param>
        /// <param name="refStrings"></param>
        /// <returns></returns>
        byte[] CompileCode(string assemblyName, string[] codes, List<string> refStrings);
    }
}