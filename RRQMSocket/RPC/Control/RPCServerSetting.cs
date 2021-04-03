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

namespace RRQMSocket.RPC
{
    /*
    若汝棋茗
    */

    /// <summary>
    /// RPC服务器相关设置
    /// </summary>
    public class RPCServerSetting
    {
        /// <summary>
        /// 命名空间（都会以RRQMRPC开头）
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// RPC版本，该值为null时，版本生成号会递增。
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// 代理令箭，当客户端获取代理文件时需验证令箭
        /// </summary>
        public string ProxyToken { get; set; }

        /// <summary>
        /// 代理源代码是否可见
        /// </summary>
        public bool ProxySourceCodeVisible { get; set; }
    }
}