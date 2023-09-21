using System;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// RpcParserExtension
    /// </summary>
    public static class RpcParserExtension
    {
        /// <summary>
        /// 配置<see cref="RpcStore"/>
        /// </summary>
        /// <typeparam name="TParser"></typeparam>
        /// <param name="parser"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TParser ConfigureRpcStore<TParser>(this TParser parser, Action<RpcStore> action) where TParser : IRpcParser
        {
            action?.Invoke(parser.RpcStore);
            return parser;
        }
    }
}