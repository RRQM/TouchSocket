using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RRQMRPC接口
    /// </summary>
    public interface IRRQMRPCParser
    {
        /// <summary>
        /// 收到字节
        /// </summary>
        event RRQMBytesEventHandler Received;

        /// <summary>
        /// 内存池实例
        /// </summary>
        BytePool BytePool { get; }

        /// <summary>
        /// 获取生成的代理代码
        /// </summary>
        CellCode[] Codes { get; }

        /// <summary>
        /// 代理源文件命名空间
        /// </summary>
        string NameSpace { get; }

        /// <summary>
        /// 获取代理文件实例
        /// </summary>
        RPCProxyInfo ProxyInfo { get; }

        /// <summary>
        /// 函数库
        /// </summary>
        MethodStore MethodStore { get; }

        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭
        /// </summary>
        string ProxyToken { get; }

        /// <summary>
        /// RPC编译器
        /// </summary>
        IRPCCompiler RPCCompiler { get; }

        /// <summary>
        /// RPC代理版本
        /// </summary>
        Version RPCVersion { get; }

        /// <summary>
        /// 序列化转换器
        /// </summary>
        SerializeConverter SerializeConverter { get; }
    }
}
