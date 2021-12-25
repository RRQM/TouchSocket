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
using System;
using System.Collections.Generic;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RRQMRPC接口
    /// </summary>
    public interface IRRQMRpcParser
    {
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
        RpcProxyInfo ProxyInfo { get; }

        /// <summary>
        /// 函数库
        /// </summary>
        MethodStore MethodStore { get; }

        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭
        /// </summary>
        string ProxyToken { get; }

        /// <summary>
        /// RPC代理版本
        /// </summary>
        Version RPCVersion { get; }

        /// <summary>
        /// 序列化选择器
        /// </summary>
        SerializationSelector SerializationSelector { get; }

        /// <summary>
        /// 获取代理文件
        /// </summary>
        /// <param name="proxyToken">代理令箭</param>
        /// <param name="caller">调用作用者/></param>
        /// <returns></returns>
        RpcProxyInfo GetProxyInfo(string proxyToken, ICaller caller);

        /// <summary>
        /// 获取注册函数
        /// </summary>
        /// <param name="proxyToken"></param>
        /// <param name="caller">调用作用者/></param>
        /// <returns></returns>
        List<MethodItem> GetRegisteredMethodItems(string proxyToken, ICaller caller);


#if NET45_OR_GREATER

        /// <summary>
        /// 编译代理
        /// </summary>
        /// <param name="targetDic">存放目标文件夹</param>
        void CompilerProxy(string targetDic = "");

#endif
    }
}