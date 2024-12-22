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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 能够基于Dmtp协议提供文件传输功能的接口
    /// </summary>
    public interface IDmtpFileTransferActor : IActor
    {
        /// <summary>
        /// 文件传输的根路径
        /// </summary>
        string RootPath { get; set; }

        /// <summary>
        /// 允许传输的小文件的最大长度。默认1024*1024个字节。
        /// <para>注意，当调整该值时，应该和对端保持一致。</para>
        /// </summary>
        int MaxSmallFileLength { get; set; }

        /// <summary>
        /// 文件资源访问接口。
        /// </summary>
        IFileResourceController FileController { get; }

        #region Id小文件


        /// <summary>
        /// 拉取小文件。默认设置1024*1024字节大小。
        /// </summary>
        /// <param name="targetId">目标客户端Id</param>
        /// <param name="path">请求路径</param>
        /// <param name="metadata">元数据</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<PullSmallFileResult> PullSmallFileAsync(string targetId, string path, Metadata metadata = null, int millisecondsTimeout = 5000, CancellationToken token = default);

        /// <summary>
        /// 推送小文件。默认设置1024*1024字节大小。
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="savePath">保存路径</param>
        /// <param name="fileInfo">推送的文件信息</param>
        /// <param name="metadata">元数据</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<Result> PushSmallFileAsync(string targetId, string savePath, FileInfo fileInfo, Metadata metadata = null, int millisecondsTimeout = 5000, CancellationToken token = default);

        #endregion Id小文件

        #region 小文件


        /// <summary>
        /// 拉取小文件。默认设置1024*1024字节大小。
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <param name="metadata">元数据</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<PullSmallFileResult> PullSmallFileAsync(string path, Metadata metadata = null, int millisecondsTimeout = 5000, CancellationToken token = default);


        /// <summary>
        /// 推送小文件。默认设置1024*1024字节大小。
        /// </summary>
        /// <param name="savePath">保存路径</param>
        /// <param name="fileInfo">推送的文件信息</param>
        /// <param name="metadata">元数据</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<Result> PushSmallFileAsync(string savePath, FileInfo fileInfo, Metadata metadata = null, int millisecondsTimeout = 5000, CancellationToken token = default);

        #endregion 小文件

        #region Id


        /// <summary>
        /// 请求完成一个资源。
        /// <para>如果是Push，当正常返回时，则说明整个过程已完成。</para>
        /// <para>如果是Pull，当正常返回时，则说明服务器对于这个过程已完成，后续还需要再<see cref="FileResourceLocator.TryFinished"/>。</para>
        /// </summary>
        /// <param name="targetId">目标客户端Id</param>
        /// <param name="fileResourceInfo">文件资源信息</param>
        /// <param name="code">状态代码</param>
        /// <param name="metadata">元数据</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<FinishedResult> FinishedFileResourceInfoAsync(string targetId, FileResourceInfo fileResourceInfo, ResultCode code, Metadata metadata = null, int millisecondsTimeout = 5000, CancellationToken token = default);


        /// <summary>
        /// 拉取文件信息。
        /// <para>注意：</para>
        /// <list type="number">
        /// <item>完成该操作后，必须在设定时间（60秒）内至少完成一次文件块访问，不然该信息将变得无效，</item>
        /// <item>每次该操作，都应该对应一次<see cref="FinishedFileResourceInfoAsync(string,TouchSocket.Dmtp.FileTransfer.FileResourceInfo,TouchSocket.Core.ResultCode,TouchSocket.Core.Metadata,int,System.Threading.CancellationToken)"/></item>
        /// </list>
        /// </summary>
        /// <param name="targetId">目标客户端Id</param>
        /// <param name="path">资源路径</param>
        /// <param name="metadata">元数据</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="fileSectionSize">文件分块尺寸。</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<FileResourceInfoResult> PullFileResourceInfoAsync(string targetId, string path, Metadata metadata = null, int fileSectionSize = 1024 * 512, int millisecondsTimeout = 5000, CancellationToken token = default);


        /// <summary>
        /// 拉取文件块。
        /// <para>注意：拉取文件块时，两个成功块之间的时间应该在设定时间（60秒）内完成。</para>
        /// </summary>
        /// <param name="targetId">目标客户端Id</param>
        /// <param name="fileSection">文件块</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<FileSectionResult> PullFileSectionAsync(string targetId, FileSection fileSection, int millisecondsTimeout = 5000, CancellationToken token = default);


        /// <summary>
        /// 推送文件信息。
        /// <para>注意：</para>
        /// <list type="number">
        /// <item>完成该操作后，必须在设定时间（60秒）内至少完成一次文件块访问，不然该信息将变得无效，</item>
        /// <item>每次该操作，都必须对应一次<see cref="FinishedFileResourceInfoAsync(string,TouchSocket.Dmtp.FileTransfer.FileResourceInfo,TouchSocket.Core.ResultCode,TouchSocket.Core.Metadata,int,System.Threading.CancellationToken)"/></item>
        /// </list>
        /// </summary>
        /// <param name="targetId">目标客户端Id</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="fileResourceLocator">文件资源定位器</param>
        /// <param name="metadata">元数据</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<Result> PushFileResourceInfoAsync(string targetId, string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = null, int millisecondsTimeout = 5000, CancellationToken token = default);


        /// <summary>
        /// 推送文件块。
        /// <para>注意：推送文件块时，两个成功块之间的时间应该在设定时间（60秒）内完成。</para>
        /// </summary>
        /// <param name="targetId">目标客户端Id</param>
        /// <param name="fileResourceLocator">文件资源定位器</param>
        /// <param name="fileSection">文件块</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<Result> PushFileSectionAsync(string targetId, FileResourceLocator fileResourceLocator, FileSection fileSection, int millisecondsTimeout = 5000, CancellationToken token = default);

        #endregion Id

        #region 文件传输


        /// <summary>
        /// 请求完成一个资源。
        /// <para>如果是Push，当正常返回时，则说明整个过程已完成。</para>
        /// <para>如果是Pull，当正常返回时，则说明服务器对于这个过程已完成，后续还需要再<see cref="FileResourceLocator.TryFinished"/>。</para>
        /// </summary>
        /// <param name="fileResourceInfo">文件资源信息</param>
        /// <param name="code">状态代码</param>
        /// <param name="metadata">元数据</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<FinishedResult> FinishedFileResourceInfoAsync(FileResourceInfo fileResourceInfo, ResultCode code, Metadata metadata = null, int millisecondsTimeout = 5000, CancellationToken token = default);


        /// <summary>
        /// 拉取文件信息。
        /// <para>注意：</para>
        /// <list type="number">
        /// <item>完成该操作后，必须在设定时间（60秒）内至少完成一次文件块访问，不然该信息将变得无效，</item>
        /// <item>每次该操作，都应该对应一次<see cref="FinishedFileResourceInfoAsync(string,TouchSocket.Dmtp.FileTransfer.FileResourceInfo,TouchSocket.Core.ResultCode,TouchSocket.Core.Metadata,int,System.Threading.CancellationToken)"/></item>
        /// </list>
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="metadata">元数据</param>
        /// <param name="fileSectionSize">文件分块尺寸</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<FileResourceInfoResult> PullFileResourceInfoAsync(string path, Metadata metadata = null, int fileSectionSize = 1024 * 512, int millisecondsTimeout = 5000, CancellationToken token = default);


        /// <summary>
        /// 拉取文件块。
        /// <para>注意：拉取文件块时，两个成功块之间的时间应该在设定时间（60秒）内完成。</para>
        /// </summary>
        /// <param name="fileSection">文件块</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<FileSectionResult> PullFileSectionAsync(FileSection fileSection, int millisecondsTimeout = 5000, CancellationToken token = default);


        /// <summary>
        /// 推送文件信息。
        /// <para>注意：</para>
        /// <list type="number">
        /// <item>完成该操作后，必须在设定时间（60秒）内至少完成一次文件块访问，不然该信息将变得无效，</item>
        /// <item>每次该操作，都必须对应一次<see cref="FinishedFileResourceInfoAsync(string,TouchSocket.Dmtp.FileTransfer.FileResourceInfo,TouchSocket.Core.ResultCode,TouchSocket.Core.Metadata,int,System.Threading.CancellationToken)"/></item>
        /// </list>
        /// </summary>
        /// <param name="savePath">保存路径</param>
        /// <param name="fileResourceLocator">文件资源定位器</param>
        /// <param name="metadata">元数据</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<Result> PushFileResourceInfoAsync(string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = null, int millisecondsTimeout = 5000, CancellationToken token = default);

        /// <summary>
        /// 推送文件块。
        /// <para>注意：推送文件块时，两个成功块之间的时间应该在设定时间（60秒）内完成。</para>
        /// </summary>
        /// <param name="fileResourceLocator">文件资源定位器</param>
        /// <param name="fileSection">文件块</param>
        /// <param name="millisecondsTimeout">超时设置</param>
        /// <param name="token">可取消令箭</param>
        /// <returns></returns>
        Task<Result> PushFileSectionAsync(FileResourceLocator fileResourceLocator, FileSection fileSection, int millisecondsTimeout = 5000, CancellationToken token = default);

        #endregion 文件传输
    }
}