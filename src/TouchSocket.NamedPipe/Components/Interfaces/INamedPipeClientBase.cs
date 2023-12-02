//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.IO.Pipes;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道终端接口
    /// </summary>
    public interface INamedPipeClientBase : IClient, ISender, IDefaultSender, IPluginObject, IRequsetInfoSender, IConfigObject, IAdapterObject, ICloseObject
    {
        /// <summary>
        /// 用于通讯的管道流。
        /// </summary>
        public PipeStream PipeStream { get; }

        /// <summary>
        /// 断开连接
        /// </summary>
        DisconnectEventHandler<INamedPipeClientBase> Disconnected { get; set; }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// </para>
        /// </summary>
        DisconnectEventHandler<INamedPipeClientBase> Disconnecting { get; set; }

        /// <summary>
        /// 表示是否为客户端。
        /// </summary>
        bool IsClient { get; }

        /// <summary>
        /// 判断是否在线
        /// <para>该属性仅表示管道状态是否在线</para>
        /// </summary>
        bool Online { get; }
    }
}