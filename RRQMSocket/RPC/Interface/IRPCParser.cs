//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC解析器接口
    /// </summary>
    public interface IRPCParser
    {
        /// <summary>
        /// 调用方法
        /// </summary>
        event Action<IRPCParser, RPCContext> InvokeMethod;

        /// <summary>
        /// 获取代理文件
        /// </summary>
        Func<string, RPCProxyInfo> GetProxyInfo { get; set; }

        /// <summary>
        /// 初始化服务
        /// </summary>
        Func<List<MethodItem>> InitMethodServer { get; set; }

        /// <summary>
        /// 调用方法结束后
        /// </summary>
        /// <param name="context"></param>
        void EndInvokeMethod(RPCContext context);

        ///// <summary>
        ///// 初始化函数映射
        ///// </summary>
        ///// <param name="serverMethodStore"></param>
        ///// <param name="clientMethodStore"></param>
        //void InitMethodStore(MethodStore serverMethodStore, MethodStore clientMethodStore);

        /// <summary>
        /// 序列化转换器
        /// </summary>
        SerializeConverter SerializeConverter { get; set; }
    }
}