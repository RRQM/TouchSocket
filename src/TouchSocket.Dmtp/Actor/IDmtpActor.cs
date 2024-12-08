//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 提供Dmtp协议的最基础功能件
    /// </summary>
    public interface IDmtpActor : IDependencyObject, IOnlineClient, IClosableClient, IIdClient
    {
        #region 属性

        /// <summary>
        /// 是否允许支持路由数据。
        /// </summary>
        bool AllowRoute { get; }

        /// <summary>
        /// 包含当前功能件的宿主通讯端。
        /// </summary>
        IDmtpActorObject Client { get; }

        /// <summary>
        /// 是否基于可靠协议构建。例如：基于Tcp则为<see langword="true"/>，基于Udp则为<see langword="false"/>。
        /// </summary>
        bool IsReliable { get; }

        /// <summary>
        /// 最后一次活动时间。
        /// </summary>
        DateTime LastActiveTime { get; }

        /// <summary>
        /// 日志
        /// </summary>
        ILog Logger { get; }

        /// <summary>
        /// 等待返回池
        /// </summary>
        WaitHandlePool<IWaitResult> WaitHandlePool { get; }

        /// <summary>
        /// 关闭标记
        /// </summary>
        CancellationToken ClosedToken { get; }

        #endregion 属性

        #region IDmtpChannel

        /// <summary>
        /// 判断指定Id的通道是否已经存在
        /// </summary>
        /// <param name="id">要判断的通道Id</param>
        /// <returns>如果通道存在返回true，否则返回false</returns>
        bool ChannelExisted(int id);

        /// <summary>
        /// 在当前对点创建一个随机Id的通道
        /// </summary>
        /// <param name="metadata">可选的元数据参数，用于传递额外的信息</param>
        /// <returns>返回一个异步任务，该任务完成后将提供创建的IDmtpChannel对象</returns>
        Task<IDmtpChannel> CreateChannelAsync(Metadata metadata = default);

        /// <summary>
        /// 在当前对点创建一个指定Id的通道
        /// </summary>
        /// <param name="id">要创建的通道的唯一标识符</param>
        /// <param name="metadata">可选参数，提供有关通道的元数据信息</param>
        /// <returns>返回创建的通道对象，类型为IDmtpChannel</returns>
        Task<IDmtpChannel> CreateChannelAsync(int id, Metadata metadata = default);

        /// <summary>
        /// 在指定路由点创建一个指定Id的通道
        /// </summary>
        /// <param name="targetId">目标路由点的标识符</param>
        /// <param name="id">要创建的通道的唯一标识符</param>
        /// <param name="metadata">有关通道的元数据，可选，默认为default(Metadata)</param>
        /// <returns>返回一个异步任务，该任务完成后将包含新创建的IDmtpChannel接口实例</returns>
        Task<IDmtpChannel> CreateChannelAsync(string targetId, int id, Metadata metadata = default);

        /// <summary>
        /// 在指定路由点创建一个随机Id的通道
        /// </summary>
        /// <param name="targetId">目标路由点的标识符</param>
        /// <param name="metadata">可选参数，用于传递附加信息</param>
        /// <returns>返回一个异步任务，该任务完成后将提供创建的IDmtpChannel对象</returns>
        Task<IDmtpChannel> CreateChannelAsync(string targetId, Metadata metadata = default);

        /// <summary>
        /// 尝试订阅已存在的通道。
        /// </summary>
        /// <param name="id">要订阅的通道的标识符。</param>
        /// <param name="channel">订阅的通道对象，成功时返回此参数。</param>
        /// <returns>如果订阅成功则返回true；如果通道不存在或发生错误则返回false。</returns>
        bool TrySubscribeChannel(int id, out IDmtpChannel channel);

        #endregion IDmtpChannel

        #region 方法

        /// <summary>
        /// 向当前对点发送一个Ping报文，并且等待回应。
        /// </summary>
        /// <param name="millisecondsTimeout">超时时间，单位为毫秒，默认为5000毫秒。用于控制等待回应的最大时长。</param>
        /// <returns>一般的，当返回<see langword="true"/>时，则表明对点一定存在。而其他情况则返回<see langword="false"/>。该方法主要用于检测对端点的可达性。</returns>
        Task<bool> PingAsync(int millisecondsTimeout = 5000);

        /// <summary>
        /// 向指定路由点发送一个Ping报文，并且等待回应。
        /// </summary>
        /// <param name="targetId">目标路由点的标识符。</param>
        /// <param name="millisecondsTimeout">等待回应的超时时间，单位为毫秒。默认为5000毫秒。</param>
        /// <returns>一般的，当返回<see langword="true"/>时，则表明对点一定存在。而其他情况则返回<see langword="false"/></returns>
        Task<bool> PingAsync(string targetId, int millisecondsTimeout = 5000);

        /// <summary>
        /// 异步发送数据。
        /// </summary>
        /// <param name="protocol">指定通信协议的标识符。</param>
        /// <param name="memory">待发送的数据，以只读内存形式提供。</param>
        /// <remarks>
        /// 此方法用于异步发送数据，通过指定协议标识符和数据内容，实现数据的异步传输。
        /// </remarks>
        Task SendAsync(ushort protocol, ReadOnlyMemory<byte> memory);

        /// <summary>
        /// 异步发送小（64K）对象的包。接收方可以通过ReadPackage来接收。
        /// </summary>
        /// <param name="protocol">发送包时使用的协议标识。</param>
        /// <param name="package">要发送的包实例。</param>
        /// <returns>返回一个Task对象，表示异步操作的完成。</returns>
        Task SendPackageAsync(ushort protocol, IPackage package);

        /// <summary>
        /// 异步发送以utf-8编码的字符串。
        /// </summary>
        /// <param name="protocol">指定的协议编号。</param>
        /// <param name="value">要发送的字符串内容。</param>
        /// <exception cref="ArgumentNullException">当<paramref name="value"/>为null时抛出异常。</exception>
        Task SendStringAsync(ushort protocol, string value);

        /// <summary>
        /// 尝试获取指定Id的DmtpActor。一般此方法仅在Service下有效。
        /// </summary>
        /// <param name="targetId">要查找的DmtpActor的唯一标识符。</param>
        /// <returns>返回一个包含DmtpActor的任务。如果找不到指定Id的DmtpActor，则返回null。</returns>
        Task<DmtpActor> TryFindDmtpActor(string targetId);

        /// <summary>
        /// 尝试请求路由，触发路由相关插件。并在路由失败时向<see cref="MsgPermitEventArgs.Message"/>中传递消息。
        /// </summary>
        /// <param name="e">包含路由信息的事件参数</param>
        /// <returns>一个Task布尔值，指示路由尝试是否成功</returns>
        Task<bool> TryRouteAsync(PackageRouterEventArgs e);

        #endregion 方法
    }
}