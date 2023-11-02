using System;
using System.IO.Pipes;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道终端接口
    /// </summary>
    public interface INamedPipeClientBase : IClient, ISender, IDefaultSender, IPluginObject, IRequsetInfoSender,IConfigObject
    {
        /// <summary>
        /// 是否允许自由调用<see cref="SetDataHandlingAdapter"/>进行赋值。
        /// </summary>
        bool CanSetDataHandlingAdapter { get; }

        /// <summary>
        /// 用于通讯的管道流。
        /// </summary>
        public PipeStream PipeStream { get; }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        SingleStreamDataHandlingAdapter DataHandlingAdapter { get; }

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

        /// <summary>
        /// 关闭客户端。
        /// </summary>
        /// <param name="msg"></param>
        /// <exception cref="Exception"></exception>
        void Close(string msg = TouchSocketCoreUtility.Empty);

        /// <summary>
        /// 设置数据处理适配器
        /// </summary>
        /// <param name="adapter"></param>
        void SetDataHandlingAdapter(SingleStreamDataHandlingAdapter adapter);
    }
}