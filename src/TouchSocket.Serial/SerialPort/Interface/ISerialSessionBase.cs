
using System;
using System.IO.Ports;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Serial
{

    /// <summary>
    /// 串口连接接口。
    /// </summary>
    public interface ISerialSessionBase : IClient, ISender, IDefaultSender, IPluginObject, IRequsetInfoSender, IConfigObject, IOnlineClient
    {
        /// <summary>
        /// 是否允许自由调用<see cref="SetDataHandlingAdapter"/>进行赋值。
        /// </summary>
        bool CanSetDataHandlingAdapter { get; }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        SingleStreamDataHandlingAdapter DataHandlingAdapter { get; }

        /// <summary>
        /// 断开连接
        /// </summary>
        DisconnectEventHandler<ISerialSessionBase> Disconnected { get; set; }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// </para>
        /// </summary>
        DisconnectEventHandler<ISerialSessionBase> Disconnecting { get; set; }

        /// <summary>
        /// 主通信器
        /// </summary>
        SerialPort MainSerialPort { get; }


        /// <summary>
        /// 串口描述
        /// </summary>
        SerialProperty SerialProperty { get; }
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