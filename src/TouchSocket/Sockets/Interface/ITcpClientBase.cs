//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using TouchSocket.Core.Config;
using TouchSocket.Core.Plugins;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// TCP终端基础接口
    /// </summary>
    public interface ITcpClientBase : IClient, ISender, IDefaultSender, IPluginObject, IRequsetInfoSender
    {
        /// <summary>
        /// 断开连接
        /// </summary>
        ClientDisconnectedEventHandler<ITcpClientBase> Disconnected { get; set; }

        /// <summary>
        /// 缓存池大小
        /// </summary>
        int BufferLength { get; }

        /// <summary>
        /// 是否允许自由调用<see cref="SetDataHandlingAdapter"/>进行赋值。
        /// </summary>
        bool CanSetDataHandlingAdapter { get; }

        /// <summary>
        /// 客户端配置
        /// </summary>
        TouchSocketConfig Config { get; }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        DataHandlingAdapter DataHandlingAdapter { get; }

        /// <summary>
        /// IP地址
        /// </summary>
        string IP { get; }

        /// <summary>
        /// 主通信器
        /// </summary>
        Socket MainSocket { get; }

        /// <summary>
        /// 适配器能接收的最大数据包长度。
        /// </summary>
        int MaxPackageSize { get; }

        /// <summary>
        /// 判断是否在线
        /// <para>该属性仅表示TCP状态是否在线</para>
        /// </summary>
        bool Online { get; }

        /// <summary>
        /// 端口号
        /// </summary>
        int Port { get; }

        /// <summary>
        /// 接收模式
        /// </summary>
        public ReceiveType ReceiveType { get; }

        /// <summary>
        /// 使用Ssl加密
        /// </summary>
        bool UseSsl { get; }

        /// <summary>
        /// 中断终端
        /// </summary>
        void Close();

        /// <summary>
        /// 中断终端，传递中断消息
        /// </summary>
        /// <param name="msg"></param>
        void Close(string msg);

        /// <summary>
        /// 获取流，在正常模式下为<see cref="NetworkStream"/>，在Ssl模式下为<see cref="SslStream"/>。
        /// </summary>
        /// <returns></returns>
        Stream GetStream();

        /// <summary>
        /// 设置数据处理适配器
        /// </summary>
        /// <param name="adapter"></param>
        void SetDataHandlingAdapter(DataHandlingAdapter adapter);

        /// <summary>
        /// 禁用发送或接收
        /// </summary>
        /// <param name="how"></param>
        void Shutdown(SocketShutdown how);
    }
}