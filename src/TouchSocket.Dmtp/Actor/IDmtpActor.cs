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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 提供Dmtp协议的最基础功能件
    /// </summary>
    public interface IDmtpActor : IDependencyObject, IHandshakeObject
    {
        #region 属性

        /// <summary>
        /// 是否允许支持路由数据。
        /// </summary>
        bool AllowRoute { get; }

        /// <summary>
        /// 是否基于可靠协议构建。例如：基于Tcp则为<see langword="true"/>，基于Udp则为<see langword="false"/>。
        /// </summary>
        bool IsReliable { get; }

        /// <summary>
        /// 包含当前功能件的宿主通讯端。
        /// </summary>
        IDmtpActorObject Client { get; }

        /// <summary>
        /// 本节点Id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 最后一次活动时间。
        /// </summary>
        DateTime LastActiveTime { get; }

        /// <summary>
        /// 日志
        /// </summary>
        ILog Logger { get; }

        /// <summary>
        ///  获取可用于同步对当前的访问对象进行锁同步。
        /// </summary>
        object SyncRoot { get; }

        /// <summary>
        /// 等待返回池
        /// </summary>
        WaitHandlePool<IWaitResult> WaitHandlePool { get; }

        #endregion 属性

        #region IDmtpChannel

        /// <summary>
        /// 判断指定Id的通道是否已经存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool ChannelExisted(int id);

        /// <summary>
        /// 在当前对点创建一个随机Id的通道
        /// </summary>
        /// <returns></returns>
        IDmtpChannel CreateChannel(Metadata metadata = default);

        /// <summary>
        /// 在当前对点创建一个指定Id的通道
        /// </summary>
        /// <param name="id"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        IDmtpChannel CreateChannel(int id, Metadata metadata = default);

        /// <summary>
        /// 在指定路由点创建一个指定Id的通道
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="id"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        IDmtpChannel CreateChannel(string targetId, int id, Metadata metadata = default);

        /// <summary>
        /// 在指定路由点创建一个随机Id的通道
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        IDmtpChannel CreateChannel(string targetId, Metadata metadata = default);

        /// <summary>
        /// 在当前对点创建一个随机Id的通道
        /// </summary>
        /// <returns></returns>
        Task<IDmtpChannel> CreateChannelAsync(Metadata metadata = default);

        /// <summary>
        /// 在当前对点创建一个指定Id的通道
        /// </summary>
        /// <param name="id"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Task<IDmtpChannel> CreateChannelAsync(int id, Metadata metadata = default);

        /// <summary>
        /// 在指定路由点创建一个指定Id的通道
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="id"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Task<IDmtpChannel> CreateChannelAsync(string targetId, int id, Metadata metadata = default);

        /// <summary>
        /// 在指定路由点创建一个随机Id的通道
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Task<IDmtpChannel> CreateChannelAsync(string targetId, Metadata metadata = default);

        /// <summary>
        /// 尝试订阅已存在的通道。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        bool TrySubscribeChannel(int id, out IDmtpChannel channel);

        #endregion IDmtpChannel

        #region 方法

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="message">传递消息</param>
        void Close(string message);

        /// <summary>
        /// 向当前对点发送一个Ping报文，并且等待回应。
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>一般的，当返回<see langword="true"/>时，则表明对点一定存在。而其他情况则返回<see langword="false"/></returns>
        bool Ping(int timeout = 5000);

        /// <summary>
        /// 向指定路由点发送一个Ping报文，并且等待回应。
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="timeout"></param>
        /// <returns>一般的，当返回<see langword="true"/>时，则表明对点一定存在。而其他情况则返回<see langword="false"/></returns>
        bool Ping(string targetId, int timeout = 5000);

        /// <summary>
        /// 向当前对点发送一个Ping报文，并且等待回应。
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>一般的，当返回<see langword="true"/>时，则表明对点一定存在。而其他情况则返回<see langword="false"/></returns>
        Task<bool> PingAsync(int timeout = 5000);

        /// <summary>
        /// 向指定路由点发送一个Ping报文，并且等待回应。
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="timeout"></param>
        /// <returns>一般的，当返回<see langword="true"/>时，则表明对点一定存在。而其他情况则返回<see langword="false"/></returns>
        Task<bool> PingAsync(string targetId, int timeout = 5000);

        /// <summary>
        /// 重新设置Id,并且同步到对端
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        void ResetId(string id);

        /// <summary>
        /// 重新设置Id,并且同步到对端
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TimeoutException"></exception>
        Task ResetIdAsync(string id);

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void Send(ushort protocol, byte[] buffer, int offset, int length);

        /// <summary>
        /// 发送字节块
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        void Send(ushort protocol, ByteBlock byteBlock);

        /// <summary>
        /// 异步发送字节
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        Task SendAsync(ushort protocol, byte[] buffer, int offset, int length);

        /// <summary>
        /// 异步发送字节块
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        Task SendAsync(ushort protocol, ByteBlock byteBlock);

        /// <summary>
        /// 以Fast序列化，发送小（64K）对象。接收方需要使用ReadObject读取对象。
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="obj"></param>
        void SendFastObject<T>(ushort protocol, in T obj);

        /// <summary>
        /// 以包发送小（64K）对象。接收方ReadPackage即可。
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="package"></param>
        void SendPackage(ushort protocol, in IPackage package);

        /// <summary>
        /// 发送以utf-8编码的字符串。
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="value"></param>
        /// <see cref="ArgumentNullException"/>
        void SendString(ushort protocol, string value);

        /// <summary>
        /// 发送以utf-8编码的字符串。
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="value"></param>
        /// <see cref="ArgumentNullException"/>
        Task SendStringAsync(ushort protocol, string value);

        /// <summary>
        /// 尝试获取指定Id的DmtpActor。一般此方法仅在Service下有效。
        /// </summary>
        /// <param name="targetId"></param>
        /// <returns></returns>
        Task<DmtpActor> TryFindDmtpActor(string targetId);

        /// <summary>
        /// 尝试请求路由，触发路由相关插件。并在路由失败时向<see cref="MsgPermitEventArgs.Message"/>中传递消息。
        /// </summary>
        /// <returns></returns>
        Task<bool> TryRoute(PackageRouterEventArgs e);

        /// <summary>
        /// 发送Close请求
        /// </summary>
        /// <param name="msg"></param>
        bool SendClose(string msg);

        #endregion 方法
    }
}