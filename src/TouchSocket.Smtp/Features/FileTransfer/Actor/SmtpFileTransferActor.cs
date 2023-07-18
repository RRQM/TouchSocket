using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Smtp.FileTransfer
{
    /// <summary>
    /// 能够基于SMTP协议提供文件传输功能
    /// </summary>
    public class SmtpFileTransferActor : ISmtpFileTransferActor
    {
        /// <summary>
        /// 创建一个<see cref="SmtpFileTransferActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        public SmtpFileTransferActor(ISmtpActor smtpActor)
        {
            this.SmtpActor = smtpActor;
        }

        #region 委托

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        public Action<ISmtpActor, FileTransferStatusEventArgs> OnFileTransfered { get; set; }

        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        public Action<ISmtpActor, FileOperationEventArgs> OnFileTransfering { get; set; }

        #endregion 委托

        #region 字段

        private ushort m_finishedFileResourceInfo_Request;
        private ushort m_finishedFileResourceInfo_Response;
        private ushort m_pullFileResourceInfo_Request;
        private ushort m_pullFileResourceInfo_Response;
        private ushort m_pullFileSection_Request;
        private ushort m_pullFileSection_Response;
        private ushort m_pullSmallFile_Request;
        private ushort m_pullSmallFile_Response;
        private ushort m_pushFileResourceInfo_Request;
        private ushort m_pushFileResourceInfo_Response;
        private ushort m_pushFileSection_Request;
        private ushort m_pushFileSection_Response;
        private ushort m_pushSmallFile_Request;
        private ushort m_pushSmallFile_Response;

        #endregion 字段

        /// <summary>
        /// 处理收到的消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool InputReceivedData(SmtpMessage message)
        {
            var byteBlock = message.BodyByteBlock;
            if (message.ProtocolFlags == this.m_pullFileResourceInfo_Request)
            {
                try
                {
                    var waitFileResource = new WaitFileResource();
                    waitFileResource.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitFileResource.Route)
                    {
                        if (this.SmtpActor.TryRoute(RouteType.PullFile, waitFileResource))
                        {
                            if (this.SmtpActor.TryFindSmtpActor(waitFileResource.TargetId, out var actor))
                            {
                                actor.Send(this.m_pullFileResourceInfo_Request, byteBlock);
                                return true;
                            }
                            else
                            {
                                waitFileResource.UnpackageBody(byteBlock);
                                waitFileResource.Status = TouchSocketSmtpStatus.ClientNotFind.ToValue();
                            }
                        }
                        else
                        {
                            waitFileResource.UnpackageBody(byteBlock);
                            waitFileResource.Status = TouchSocketSmtpStatus.RoutingNotAllowed.ToValue();
                        }
                        waitFileResource.SwitchId();
                        byteBlock.Reset();
                        waitFileResource.Package(byteBlock);
                        this.SmtpActor.Send(this.m_pullFileResourceInfo_Response, byteBlock);
                    }
                    else
                    {
                        waitFileResource.UnpackageBody(byteBlock);
                        Task.Factory.StartNew(this.RequestPullFileResourceInfo, waitFileResource);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pullFileResourceInfo_Response)
            {
                try
                {
                    var waitFileResource = new WaitFileResource();
                    waitFileResource.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitFileResource.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(waitFileResource.TargetId, out var actor))
                        {
                            actor.Send(this.m_pullFileResourceInfo_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileResource.UnpackageBody(byteBlock);
                        this.SmtpActor.WaitHandlePool.SetRun(waitFileResource);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pullFileSection_Request)
            {
                try
                {
                    var waitFileSection = new WaitFileSection();
                    waitFileSection.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitFileSection.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(waitFileSection.TargetId, out var actor))
                        {
                            actor.Send(this.m_pullFileSection_Request, byteBlock);
                        }
                        else
                        {
                            waitFileSection.Status = TouchSocketSmtpStatus.ClientNotFind.ToValue();
                            waitFileSection.SwitchId();
                            byteBlock.Reset();
                            waitFileSection.Package(byteBlock);
                            this.SmtpActor.Send(this.m_pullFileSection_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileSection.UnpackageBody(byteBlock);
                        this.RequestPullFileSection(waitFileSection);
                        //Task.Factory.StartNew(this.RequestPullFileSection, waitFileSection);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pullFileSection_Response)
            {
                try
                {
                    var waitFileSection = new WaitFileSection();
                    waitFileSection.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitFileSection.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(waitFileSection.TargetId, out var actor))
                        {
                            actor.Send(this.m_pullFileSection_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileSection.UnpackageBody(byteBlock);
                        this.SmtpActor.WaitHandlePool.SetRun(waitFileSection);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushFileResourceInfo_Request)
            {
                try
                {
                    var waitFileResource = new WaitFileResource();
                    waitFileResource.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitFileResource.Route)
                    {
                        if (this.SmtpActor.TryRoute(RouteType.PullFile, waitFileResource))
                        {
                            if (this.SmtpActor.TryFindSmtpActor(waitFileResource.TargetId, out var actor))
                            {
                                actor.Send(this.m_pushFileResourceInfo_Request, byteBlock);
                                return true;
                            }
                            else
                            {
                                waitFileResource.UnpackageBody(byteBlock);
                                waitFileResource.Status = TouchSocketSmtpStatus.ClientNotFind.ToValue();
                            }
                        }
                        else
                        {
                            waitFileResource.UnpackageBody(byteBlock);
                            waitFileResource.Status = TouchSocketSmtpStatus.RoutingNotAllowed.ToValue();
                        }

                        byteBlock.Reset();
                        waitFileResource.SwitchId();
                        waitFileResource.Package(byteBlock);
                        this.SmtpActor.Send(this.m_pushFileResourceInfo_Response, byteBlock);
                    }
                    else
                    {
                        waitFileResource.UnpackageBody(byteBlock);
                        Task.Factory.StartNew(this.RequestPushFileResourceInfo, waitFileResource);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushFileResourceInfo_Response)
            {
                try
                {
                    var waitFileResource = new WaitFileResource();
                    waitFileResource.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitFileResource.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(waitFileResource.TargetId, out var actor))
                        {
                            actor.Send(this.m_pushFileResourceInfo_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileResource.UnpackageBody(byteBlock);
                        this.SmtpActor.WaitHandlePool.SetRun(waitFileResource);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushFileSection_Request)
            {
                try
                {
                    var waitFileSection = new WaitFileSection();
                    waitFileSection.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitFileSection.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(waitFileSection.TargetId, out var actor))
                        {
                            actor.Send(this.m_pushFileSection_Request, byteBlock);
                        }
                        else
                        {
                            waitFileSection.Status = TouchSocketSmtpStatus.ClientNotFind.ToValue();
                            waitFileSection.SwitchId();
                            byteBlock.Reset();
                            waitFileSection.Package(byteBlock);
                            this.SmtpActor.Send(this.m_pushFileSection_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileSection.UnpackageBody(byteBlock);
                        this.RequestPushFileSection(waitFileSection);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushFileSection_Response)
            {
                try
                {
                    var waitFileSection = new WaitFileSection();
                    waitFileSection.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitFileSection.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(waitFileSection.TargetId, out var actor))
                        {
                            actor.Send(this.m_pushFileSection_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileSection.UnpackageBody(byteBlock);
                        this.SmtpActor.WaitHandlePool.SetRun(waitFileSection);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_finishedFileResourceInfo_Request)
            {
                try
                {
                    var waitFinishedPackage = new WaitFinishedPackage();
                    waitFinishedPackage.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitFinishedPackage.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(waitFinishedPackage.TargetId, out var actor))
                        {
                            actor.Send(this.m_finishedFileResourceInfo_Request, byteBlock);
                        }
                        else
                        {
                            waitFinishedPackage.Status = TouchSocketSmtpStatus.ClientNotFind.ToValue();
                            waitFinishedPackage.SwitchId();
                            byteBlock.Reset();
                            waitFinishedPackage.Package(byteBlock);
                            this.SmtpActor.Send(this.m_finishedFileResourceInfo_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFinishedPackage.UnpackageBody(byteBlock);
                        Task.Factory.StartNew(this.RequestFinishedFileResourceInfo, waitFinishedPackage);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_finishedFileResourceInfo_Response)
            {
                try
                {
                    var waitFinishedPackage = new WaitFinishedPackage();
                    waitFinishedPackage.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitFinishedPackage.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(waitFinishedPackage.TargetId, out var actor))
                        {
                            actor.Send(this.m_finishedFileResourceInfo_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFinishedPackage.UnpackageBody(byteBlock);
                        this.SmtpActor.WaitHandlePool.SetRun(waitFinishedPackage);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pullSmallFile_Request)
            {
                try
                {
                    var waitSmallFilePackage = new WaitSmallFilePackage();
                    waitSmallFilePackage.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitSmallFilePackage.Route)
                    {
                        if (this.SmtpActor.TryRoute(RouteType.PullFile, waitSmallFilePackage))
                        {
                            if (this.SmtpActor.TryFindSmtpActor(waitSmallFilePackage.TargetId, out var actor))
                            {
                                actor.Send(this.m_pullSmallFile_Request, byteBlock);
                                return true;
                            }
                            else
                            {
                                waitSmallFilePackage.UnpackageBody(byteBlock);
                                waitSmallFilePackage.Status = TouchSocketSmtpStatus.ClientNotFind.ToValue();
                            }
                        }
                        else
                        {
                            waitSmallFilePackage.UnpackageBody(byteBlock);
                            waitSmallFilePackage.Status = TouchSocketSmtpStatus.RoutingNotAllowed.ToValue();
                        }
                        byteBlock.Reset();
                        waitSmallFilePackage.SwitchId();
                        waitSmallFilePackage.Package(byteBlock);
                        this.SmtpActor.Send(this.m_pullSmallFile_Response, byteBlock);
                    }
                    else
                    {
                        waitSmallFilePackage.UnpackageBody(byteBlock);
                        Task.Factory.StartNew(this.RequestPullSmallFile, waitSmallFilePackage);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pullSmallFile_Response)
            {
                try
                {
                    var waitSmallFilePackage = new WaitSmallFilePackage();
                    waitSmallFilePackage.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitSmallFilePackage.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(waitSmallFilePackage.TargetId, out var actor))
                        {
                            actor.Send(this.m_pullSmallFile_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitSmallFilePackage.UnpackageBody(byteBlock);
                        this.SmtpActor.WaitHandlePool.SetRun(waitSmallFilePackage);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushSmallFile_Request)
            {
                try
                {
                    var waitSmallFilePackage = new WaitSmallFilePackage();
                    waitSmallFilePackage.UnpackageRouter(byteBlock);
                    if (this.SmtpActor.AllowRoute && waitSmallFilePackage.Route)
                    {
                        if (this.SmtpActor.TryRoute(RouteType.PullFile, waitSmallFilePackage))
                        {
                            if (this.SmtpActor.TryFindSmtpActor(waitSmallFilePackage.TargetId, out var actor))
                            {
                                actor.Send(this.m_pushSmallFile_Request, byteBlock);
                                return true;
                            }
                            else
                            {
                                waitSmallFilePackage.UnpackageBody(byteBlock);
                                waitSmallFilePackage.Status = TouchSocketSmtpStatus.ClientNotFind.ToValue();
                            }
                        }
                        else
                        {
                            waitSmallFilePackage.UnpackageBody(byteBlock);
                            waitSmallFilePackage.Status = TouchSocketSmtpStatus.RoutingNotAllowed.ToValue();
                        }
                        byteBlock.Reset();
                        waitSmallFilePackage.SwitchId();
                        waitSmallFilePackage.Package(byteBlock);
                        this.SmtpActor.Send(this.m_pushSmallFile_Response, byteBlock);
                    }
                    else
                    {
                        waitSmallFilePackage.UnpackageBody(byteBlock);
                        Task.Factory.StartNew(this.RequestPushSmallFile, waitSmallFilePackage);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushSmallFile_Response)
            {
                try
                {
                    var waitSmallFilePackage = new WaitSmallFilePackage();
                    waitSmallFilePackage.UnpackageRouter(byteBlock);

                    if (this.SmtpActor.AllowRoute && waitSmallFilePackage.Route)
                    {
                        if (this.SmtpActor.TryFindSmtpActor(waitSmallFilePackage.TargetId, out var actor))
                        {
                            actor.Send(this.m_pushSmallFile_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitSmallFilePackage.UnpackageBody(byteBlock);
                        this.SmtpActor.WaitHandlePool.SetRun(waitSmallFilePackage);
                    }
                }
                catch (Exception ex)
                {
                    this.SmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置处理协议标识的起始标识。默认为30；
        /// </summary>
        /// <param name="start"></param>
        public void SetProtocolFlags(ushort start)
        {
            this.m_pullFileResourceInfo_Request = start++;
            this.m_pullFileResourceInfo_Response = start++;
            this.m_pullFileSection_Request = start++;
            this.m_pullFileSection_Response = start++;
            this.m_pushFileResourceInfo_Request = start++;
            this.m_pushFileResourceInfo_Response = start++;
            this.m_pushFileSection_Request = start++;
            this.m_pushFileSection_Response = start++;
            this.m_finishedFileResourceInfo_Request = start++;
            this.m_finishedFileResourceInfo_Response = start++;
            this.m_pullSmallFile_Request = start++;
            this.m_pullSmallFile_Response = start++;
            this.m_pushSmallFile_Request = start++;
            this.m_pushSmallFile_Response = start++;
        }

        #region 属性

        private string m_rootPath = string.Empty;

        /// <summary>
        /// 文件资源访问接口。
        /// </summary>
        public IFileResourceController FileController { get; set; }

        /// <inheritdoc/>
        public string RootPath
        {
            get => this.m_rootPath;
            set
            {
                value ??= string.Empty;
                this.m_rootPath = value;
            }
        }

        /// <inheritdoc/>
        public ISmtpActor SmtpActor { get; }

        #endregion 属性

        private bool TryFindSmtpFileTransferActor(string targetId, out SmtpFileTransferActor rpcActor)
        {
            if (targetId == this.SmtpActor.Id)
            {
                rpcActor = this;
                return true;
            }
            if (this.SmtpActor.TryFindSmtpActor(targetId, out var smtpActor))
            {
                if (smtpActor.GetSmtpFileTransferActor() is SmtpFileTransferActor newActor)
                {
                    rpcActor = newActor;
                    return true;
                }
            }

            rpcActor = default;
            return false;
        }

        #region Id传输

        /// <inheritdoc/>

        public FinishedResult FinishedFileResourceInfo(string targetId, FileResourceInfo fileResourceInfo, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                return new FinishedResult(ResultCode.Error, TouchSocketCoreResource.ArgumentNull.GetDescription(targetId), fileResourceInfo.ResourceHandle, default);
            }

            if (this.SmtpActor.AllowRoute && this.TryFindSmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.FinishedFileResourceInfo(fileResourceInfo, metadata, timeout, token);
            }
            else
            {
                return this.PrivateFinishedFileResourceInfo(targetId, fileResourceInfo, metadata, timeout, token);
            }
        }

        /// <inheritdoc/>
        public Task<FinishedResult> FinishedFileResourceInfoAsync(string targetId, FileResourceInfo fileResourceInfo, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.FinishedFileResourceInfo(targetId, fileResourceInfo, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>

        public FileResourceInfoResult PullFileResourceInfo(string targetId, string path, Metadata metadata = default, int fileSectionSize = 1024 * 512, int timeout = 5000, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                return new FileResourceInfoResult(TouchSocketCoreResource.ArgumentNull.GetDescription(targetId));
            }

            if (this.SmtpActor.AllowRoute && this.TryFindSmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.PullFileResourceInfo(path, metadata, fileSectionSize, timeout, token);
            }
            else
            {
                return this.PrivatePullFileResourceInfo(targetId, path, metadata, fileSectionSize, timeout, token);
            }
        }

        /// <inheritdoc/>

        public Task<FileResourceInfoResult> PullFileResourceInfoAsync(string targetId, string path, Metadata metadata = default, int fileSectionSize = 1024 * 512, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullFileResourceInfo(targetId, path, metadata, timeout, fileSectionSize, token);
            });
        }

        /// <inheritdoc/>

        public FileSectionResult PullFileSection(string targetId, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                return new FileSectionResult(ResultCode.Error, TouchSocketCoreResource.ArgumentNull.GetDescription(targetId), default, fileSection);
            }

            if (this.SmtpActor.AllowRoute && this.TryFindSmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.PullFileSection(fileSection, timeout, token);
            }
            else
            {
                return this.PrivatePullFileSection(targetId, fileSection, timeout, token);
            }
        }

        /// <inheritdoc/>

        public Task<FileSectionResult> PullFileSectionAsync(string targetId, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullFileSection(targetId, fileSection, timeout, token);
            });
        }

        /// <inheritdoc/>

        public Result PushFileResourceInfo(string targetId, string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                return new Result(ResultCode.Error, TouchSocketCoreResource.ArgumentNull.GetDescription(targetId));
            }

            if (this.SmtpActor.AllowRoute && this.TryFindSmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.PushFileResourceInfo(savePath, fileResourceLocator, metadata, timeout, token);
            }
            else
            {
                return this.PrivatePushFileResourceInfo(targetId, savePath, fileResourceLocator, metadata, timeout, token);
            }
        }

        /// <inheritdoc/>

        public Task<Result> PushFileResourceInfoAsync(string targetId, string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushFileResourceInfo(targetId, savePath, fileResourceLocator, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>

        public Task<Result> PushFileSectionAsync(string targetId, FileResourceLocator fileResourceLocator, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushFileSection(targetId, fileResourceLocator, fileSection, timeout, token);
            });
        }

        #endregion Id传输

        #region 传输

        /// <inheritdoc/>

        public FinishedResult FinishedFileResourceInfo(FileResourceInfo fileResourceInfo, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivateFinishedFileResourceInfo(default, fileResourceInfo, metadata, timeout, token);
        }

        /// <inheritdoc/>
        public Task<FinishedResult> FinishedFileResourceInfoAsync(FileResourceInfo fileResourceInfo, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.FinishedFileResourceInfo(fileResourceInfo, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public FileResourceInfoResult PullFileResourceInfo(string path, Metadata metadata = default, int fileSectionSize = 1024 * 512, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePullFileResourceInfo(default, path, metadata, fileSectionSize, timeout, token);
        }

        /// <inheritdoc/>
        public Task<FileResourceInfoResult> PullFileResourceInfoAsync(string path, Metadata metadata = default, int fileSectionSize = 1024 * 512, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullFileResourceInfo(path, metadata, fileSectionSize, timeout, token);
            });
        }

        /// <inheritdoc/>
        public FileSectionResult PullFileSection(FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePullFileSection(default, fileSection, timeout, token);
        }

        /// <inheritdoc/>
        public Task<FileSectionResult> PullFileSectionAsync(FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullFileSection(fileSection, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Result PushFileResourceInfo(string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePushFileResourceInfo(default, savePath, fileResourceLocator, metadata, timeout, token);
        }

        /// <inheritdoc/>
        public Task<Result> PushFileResourceInfoAsync(string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushFileResourceInfo(savePath, fileResourceLocator, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Result PushFileSection(string targetId, FileResourceLocator fileResourceLocator, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                return new Result(ResultCode.Error, TouchSocketCoreResource.ArgumentNull.GetDescription(targetId));
            }

            if (this.SmtpActor.AllowRoute && this.TryFindSmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.PushFileSection(fileResourceLocator, fileSection, timeout, token);
            }
            else
            {
                return this.PrivatePushFileSection(targetId, fileResourceLocator, fileSection, timeout, token);
            }
        }

        /// <inheritdoc/>
        public Result PushFileSection(FileResourceLocator fileResourceLocator, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePushFileSection(default, fileResourceLocator, fileSection, timeout, token);
        }

        /// <inheritdoc/>
        public Task<Result> PushFileSectionAsync(FileResourceLocator fileResourceLocator, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushFileSection(fileResourceLocator, fileSection, timeout, token);
            });
        }

        private FinishedResult PrivateFinishedFileResourceInfo(string targetId, FileResourceInfo fileResourceInfo, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            var waitFinishedPackage = new WaitFinishedPackage()
            {
                SourceId = this.SmtpActor.Id,
                TargetId = targetId,
                Route = targetId.HasValue(),
                ResourceHandle = fileResourceInfo.ResourceHandle,
                Metadata = metadata,
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetWaitData(waitFinishedPackage);

            var byteBlock = new ByteBlock();
            try
            {
                waitFinishedPackage.Package(byteBlock);
                this.SmtpActor.Send(this.m_finishedFileResourceInfo_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitFinishedPackage)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketSmtpStatus.Success:
                                    {
                                        return new FinishedResult(ResultCode.Success, waitFile.ResourceHandle, waitFile.UnFinishedIndexs);
                                    }
                                case TouchSocketSmtpStatus.ResourceHandleNotFind:
                                    {
                                        return new FinishedResult(ResultCode.Error, TouchSocketSmtpStatus.ResourceHandleNotFind.GetDescription(waitFile.ResourceHandle), waitFile.ResourceHandle, waitFile.UnFinishedIndexs);
                                    }
                                case TouchSocketSmtpStatus.HasUnFinished:
                                    {
                                        return new FinishedResult(ResultCode.Fail, TouchSocketSmtpStatus.HasUnFinished.GetDescription(waitFile.UnFinishedIndexs.Length), waitFile.ResourceHandle, waitFile.UnFinishedIndexs);
                                    }
                                default:
                                    {
                                        return new FinishedResult(ResultCode.Error, waitFile.Message, waitFile.ResourceHandle, waitFile.UnFinishedIndexs);
                                    }
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return new FinishedResult(Result.Overtime, fileResourceInfo.ResourceHandle);
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return new FinishedResult(Result.Canceled, fileResourceInfo.ResourceHandle);
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return new FinishedResult(Result.UnknownFail, fileResourceInfo.ResourceHandle);
                        }
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private FileResourceInfoResult PrivatePullFileResourceInfo(string targetId, string path, Metadata metadata = default, int fileSectionSize = 1024 * 512, int timeout = 5000, CancellationToken token = default)
        {
            var waitFileResource = new WaitFileResource()
            {
                Path = path,
                TargetId = targetId,
                Route = targetId.HasValue(),
                SourceId = this.SmtpActor.Id,
                Metadata = metadata,
                FileSectionSize = fileSectionSize
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetWaitData(waitFileResource);

            var byteBlock = new ByteBlock();
            try
            {
                waitFileResource.Package(byteBlock);
                this.SmtpActor.Send(this.m_pullFileResourceInfo_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitFileResource)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketSmtpStatus.Success:
                                    {
                                        var resourceInfo = new FileResourceInfo(waitFile.FileInfo, fileSectionSize);
                                        resourceInfo.ResetResourceHandle(waitFile.ResourceHandle);
                                        return new FileResourceInfoResult(resourceInfo);
                                    }
                                case TouchSocketSmtpStatus.FileNotExists:
                                    {
                                        return new FileResourceInfoResult(TouchSocketSmtpStatus.FileNotExists.GetDescription(waitFile.Path));
                                    }
                                case TouchSocketSmtpStatus.RemoteRefuse:
                                default:
                                    {
                                        return new FileResourceInfoResult(waitFile.Status.ToStatus().GetDescription());
                                    }
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return new FileResourceInfoResult(Result.Overtime);
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return new FileResourceInfoResult(Result.Canceled);
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return new FileResourceInfoResult(Result.UnknownFail);
                        }
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private FileSectionResult PrivatePullFileSection(string targetId, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            fileSection.Status = FileSectionStatus.Transfering;
            var waitFileSection = new WaitFileSection()
            {
                SourceId = this.SmtpActor.Id,
                TargetId = targetId,
                Route = targetId.HasValue(),
                FileSection = fileSection
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetWaitData(waitFileSection);

            var byteBlock = new ByteBlock();
            try
            {
                waitFileSection.Package(byteBlock);
                this.SmtpActor.Send(this.m_pullFileSection_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitFileSection)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketSmtpStatus.Success:
                                    {
                                        return new FileSectionResult(ResultCode.Success, waitFile.Value.Array, fileSection);
                                    }
                                case TouchSocketSmtpStatus.ResourceHandleNotFind:
                                    {
                                        return new FileSectionResult(ResultCode.Error, TouchSocketSmtpStatus.ResourceHandleNotFind.GetDescription(waitFile.FileSection.ResourceHandle), default, fileSection);
                                    }
                                case TouchSocketSmtpStatus.ClientNotFind:
                                    {
                                        return new FileSectionResult(ResultCode.Error, TouchSocketSmtpStatus.ClientNotFind.GetDescription(targetId), default, fileSection);
                                    }
                                case TouchSocketSmtpStatus.LengthErrorWhenRead:
                                    {
                                        return new FileSectionResult(ResultCode.Error, TouchSocketSmtpStatus.LengthErrorWhenRead.GetDescription(), default, fileSection);
                                    }
                                default:
                                    return new FileSectionResult(ResultCode.Exception, waitFile.Message, default, fileSection);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return new FileSectionResult(ResultCode.Overtime, default, fileSection);
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return new FileSectionResult(ResultCode.Canceled, default, fileSection);
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return new FileSectionResult(ResultCode.Fail, default, fileSection);
                        }
                }
            }
            finally
            {
                fileSection.Status = FileSectionStatus.Transfered;
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private Result PrivatePushFileResourceInfo(string targetId, string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            var waitFileResource = new WaitFileResource()
            {
                Path = savePath,
                TargetId = targetId,
                Route = targetId.HasValue(),
                SourceId = this.SmtpActor.Id,
                FileInfo = fileResourceLocator.FileResourceInfo.FileInfo,
                Metadata = metadata,
                FileSectionSize = fileResourceLocator.FileResourceInfo.FileSectionSize
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetWaitData(waitFileResource);

            var byteBlock = new ByteBlock();
            try
            {
                waitFileResource.Package(byteBlock);
                this.SmtpActor.Send(this.m_pushFileResourceInfo_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitFileResource)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketSmtpStatus.Success:
                                    {
                                        fileResourceLocator.FileResourceInfo.ResetResourceHandle(waitFile.ResourceHandle);
                                        return Result.Success;
                                    }
                                case TouchSocketSmtpStatus.RemoteRefuse:
                                    {
                                        return new Result(ResultCode.Error, waitFile.Status.ToStatus().GetDescription());
                                    }
                                case TouchSocketSmtpStatus.ClientNotFind:
                                    {
                                        return new Result(ResultCode.Error, waitFile.Status.ToStatus().GetDescription(targetId));
                                    }
                                default:
                                    return new Result(ResultCode.Error, waitFile.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return Result.Overtime;
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return Result.Canceled;
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return Result.UnknownFail;
                        }
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private Result PrivatePushFileSection(string targetId, FileResourceLocator fileResourceLocator, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            fileSection.Status = FileSectionStatus.Transfering;
            var fileSectionResult = fileResourceLocator.ReadFileSection(fileSection);
            if (!fileSectionResult.IsSuccess())
            {
                fileSection.Status = FileSectionStatus.Fail;
                return new Result(fileSectionResult);
            }
            var waitFileSection = new WaitFileSection()
            {
                SourceId = this.SmtpActor.Id,
                TargetId = targetId,
                Route = targetId.HasValue(),
                FileSection = fileSection,
                Value = new ArraySegment<byte>(fileSectionResult.Value)
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetWaitData(waitFileSection);

            var byteBlock = new ByteBlock(fileSectionResult.Value.Length + 1024);
            try
            {
                waitFileSection.Package(byteBlock);
                this.SmtpActor.Send(this.m_pushFileSection_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitFileSection)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketSmtpStatus.Success:
                                    {
                                        fileSection.Status = FileSectionStatus.Finished;
                                        return Result.Success;
                                    }
                                case TouchSocketSmtpStatus.ResourceHandleNotFind:
                                    {
                                        fileSection.Status = FileSectionStatus.Fail;
                                        return new Result(ResultCode.Error, TouchSocketSmtpStatus.ResourceHandleNotFind.GetDescription(waitFile.FileSection.ResourceHandle));
                                    }
                                default:
                                    fileSection.Status = FileSectionStatus.Fail;
                                    return new Result(ResultCode.Error, waitFile.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            fileSection.Status = FileSectionStatus.Fail;
                            return Result.Overtime;
                        }
                    case WaitDataStatus.Canceled:
                        {
                            fileSection.Status = FileSectionStatus.Fail;
                            return Result.Canceled;
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            fileSection.Status = FileSectionStatus.Fail;
                            return Result.UnknownFail;
                        }
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private void RequestFinishedFileResourceInfo(object o)//请求完成
        {
            //2.未找到
            //3.未完成
            try
            {
                var waitFinishedPackage = (WaitFinishedPackage)o;
                var transferType = TransferType.SectionPull;
                string resourcePath = default;
                string savePath = default;
                var resultThis = Result.Success;
                FileResourceInfo resourceInfo = default;
                try
                {
                    if (this.FileController.TryGetFileResourceLocator(waitFinishedPackage.ResourceHandle, out var locator))
                    {
                        if (locator.FileAccess == FileAccess.Read)
                        {
                            transferType = TransferType.SectionPull;
                            if (this.FileController.TryRelaseFileResourceLocator(waitFinishedPackage.ResourceHandle, out _))
                            {
                                waitFinishedPackage.Status = TouchSocketSmtpStatus.Success.ToValue();
                                resourcePath = locator.LocatorPath;
                                resourceInfo = locator.FileResourceInfo;
                            }
                            else
                            {
                                waitFinishedPackage.Status = TouchSocketSmtpStatus.ResourceHandleNotFind.ToValue();
                                resultThis = new Result(ResultCode.Fail, TouchSocketSmtpStatus.ResourceHandleNotFind.GetDescription(waitFinishedPackage.ResourceHandle));
                            }
                        }
                        else
                        {
                            transferType = TransferType.SectionPush;
                            if (this.FileController.TryGetFileResourceLocator(waitFinishedPackage.ResourceHandle, out _))
                            {
                                resourceInfo = locator.FileResourceInfo;
                                savePath = locator.LocatorPath;
                                var sections = locator.GetUnfinishedFileSection();
                                if (sections.Length == 0)
                                {
                                    var result = locator.TryFinished();
                                    if (result.IsSuccess())
                                    {
                                        waitFinishedPackage.Status = TouchSocketSmtpStatus.Success.ToValue();
                                    }
                                    else
                                    {
                                        waitFinishedPackage.Status = TouchSocketSmtpStatus.Exception.ToValue();
                                        waitFinishedPackage.Message = result.Message;
                                        resultThis = result;
                                    }
                                }
                                else
                                {
                                    waitFinishedPackage.UnFinishedIndexs = sections.Select(a => a.Index).ToArray();
                                    waitFinishedPackage.Status = TouchSocketSmtpStatus.HasUnFinished.ToValue();

                                    resultThis = new Result(ResultCode.Fail, TouchSocketSmtpStatus.HasUnFinished.GetDescription(sections.Length));
                                }
                            }
                            else
                            {
                                waitFinishedPackage.Status = TouchSocketSmtpStatus.ResourceHandleNotFind.ToValue();
                                resultThis = new Result(ResultCode.Fail, TouchSocketSmtpStatus.ResourceHandleNotFind.GetDescription(waitFinishedPackage.ResourceHandle));
                            }
                        }
                    }
                    else
                    {
                        waitFinishedPackage.Status = TouchSocketSmtpStatus.ResourceHandleNotFind.ToValue();
                        resultThis = new Result(ResultCode.Fail, TouchSocketSmtpStatus.ResourceHandleNotFind.GetDescription(waitFinishedPackage.ResourceHandle));
                    }
                }
                catch (Exception ex)
                {
                    waitFinishedPackage.Status = TouchSocketSmtpStatus.Exception.ToValue();
                    waitFinishedPackage.Message = ex.Message;
                }

                using (var byteBlock = new ByteBlock())
                {
                    waitFinishedPackage.SwitchId();
                    waitFinishedPackage.Package(byteBlock);
                    this.SmtpActor.Send(this.m_finishedFileResourceInfo_Response, byteBlock);
                }

                var args = new FileTransferStatusEventArgs(
                    transferType, waitFinishedPackage?.Metadata, resourceInfo?.FileInfo, resultThis)
                {
                    ResourcePath = resourcePath,
                    SavePath = savePath
                };
                this.OnFileTransfered?.Invoke(this.SmtpActor, args);
            }
            catch
            {
            }
        }

        private void RequestPullFileResourceInfo(object o)//请求拉取
        {
            //2.不响应
            //3.不允许
            //4.文件不存在
            //5.other
            try
            {
                var waitFileResource = (WaitFileResource)o;

                try
                {
                    var args = new FileOperationEventArgs(TransferType.SectionPull, waitFileResource.Metadata, default)
                    {
                        ResourcePath = waitFileResource.Path
                    };

                    this.OnFileTransfering?.Invoke(this.SmtpActor, args);

                    var fullPath = this.FileController.GetFullPath(this.RootPath, args.ResourcePath);

                    if (args.IsPermitOperation)
                    {
                        if (File.Exists(fullPath))
                        {
                            var locator = this.FileController.LoadFileResourceLocatorForRead(fullPath, waitFileResource.FileSectionSize);
                            waitFileResource.ResourceHandle = locator.FileResourceInfo.ResourceHandle;
                            waitFileResource.FileInfo = locator.FileResourceInfo.FileInfo;
                            waitFileResource.Status = TouchSocketSmtpStatus.Success.ToValue();
                        }
                        else
                        {
                            waitFileResource.Path = fullPath;
                            waitFileResource.Status = TouchSocketSmtpStatus.FileNotExists.ToValue();
                        }
                    }
                    else
                    {
                        waitFileResource.Status = TouchSocketSmtpStatus.RemoteRefuse.ToValue();
                        waitFileResource.Message = args.Message;
                    }
                }
                catch (Exception ex)
                {
                    waitFileResource.Message = ex.Message;
                    waitFileResource.Status = TouchSocketSmtpStatus.Exception.ToValue();
                }
                using (var byteBlock = new ByteBlock())
                {
                    waitFileResource.SwitchId();
                    waitFileResource.Package(byteBlock);
                    this.SmtpActor.Send(this.m_pullFileResourceInfo_Response, byteBlock);
                }
            }
            catch
            {
            }
        }

        private void RequestPullFileSection(object o)
        {
            try
            {
                //2.没找到
                //3.读取文件长度不一致
                var waitFileSection = (WaitFileSection)o;
                var length = waitFileSection.FileSection.Length;
                var buffer = BytePool.Default.Rent(length);
                try
                {
                    if (this.FileController.TryGetFileResourceLocator(waitFileSection.FileSection.ResourceHandle,
                   out var locator))
                    {
                        var r = locator.ReadBytes(waitFileSection.FileSection.Offset, buffer, 0, length);
                        if (r == length)
                        {
                            waitFileSection.Status = TouchSocketSmtpStatus.Success.ToValue();
                            waitFileSection.Value = new ArraySegment<byte>(buffer, 0, length);
                        }
                        else
                        {
                            waitFileSection.Status = TouchSocketSmtpStatus.LengthErrorWhenRead.ToValue();
                        }
                    }
                    else
                    {
                        waitFileSection.Status = TouchSocketSmtpStatus.ResourceHandleNotFind.ToValue();
                    }

                    using (var byteBlock = new ByteBlock(length + 1024))
                    {
                        waitFileSection.SwitchId();
                        waitFileSection.Package(byteBlock);
                        this.SmtpActor.Send(this.m_pullFileSection_Response, byteBlock);
                    }
                }
                finally
                {
                    waitFileSection.Value = default;
                    BytePool.Default.Return(buffer);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void RequestPushFileResourceInfo(object o)//请求推送
        {
            //2.不响应
            //3.不支持
            //4.其他
            try
            {
                var waitFileResource = (WaitFileResource)o;
                try
                {
                    var args = new FileOperationEventArgs(TransferType.SectionPush,
                        waitFileResource.Metadata, waitFileResource.FileInfo)
                    {
                        SavePath = waitFileResource.Path
                    };

                    this.OnFileTransfering?.Invoke(this.SmtpActor, args);

                    var savePath = this.FileController.GetFullPath(this.RootPath, args.SavePath);

                    if (args.IsPermitOperation)
                    {
                        var locator = this.FileController.LoadFileResourceLocatorForWrite(savePath, new FileResourceInfo(waitFileResource.FileInfo, waitFileResource.FileSectionSize));
                        waitFileResource.ResourceHandle = locator.FileResourceInfo.ResourceHandle;
                        waitFileResource.Status = TouchSocketSmtpStatus.Success.ToValue();
                    }
                    else
                    {
                        waitFileResource.Status = TouchSocketSmtpStatus.RemoteRefuse.ToValue();
                        waitFileResource.Message = args.Message;
                    }
                }
                catch (Exception ex)
                {
                    waitFileResource.Status = TouchSocketSmtpStatus.Exception.ToValue();
                    waitFileResource.Message = ex.Message;
                }
                using (var byteBlock = new ByteBlock())
                {
                    waitFileResource.SwitchId();
                    waitFileResource.Package(byteBlock);
                    this.SmtpActor.Send(this.m_pushFileResourceInfo_Response, byteBlock);
                }
            }
            catch
            {
            }
        }

        private void RequestPushFileSection(object o)
        {
            //2.没找到
            //3.其他
            try
            {
                var waitFileSection = (WaitFileSection)o;
                try
                {
                    if (this.FileController.TryGetFileResourceLocator(waitFileSection.FileSection.ResourceHandle, out var locator))
                    {
                        var result = Result.UnknownFail;
                        for (var i = 0; i < 10; i++)
                        {
                            result = locator.WriteFileSection(waitFileSection.FileSection, waitFileSection.Value.Array);
                            if (result.IsSuccess())
                            {
                                break;
                            }
                        }
                        if (result.IsSuccess())
                        {
                            waitFileSection.Status = TouchSocketSmtpStatus.Success.ToValue();
                        }
                        else
                        {
                            waitFileSection.Status = TouchSocketSmtpStatus.Exception.ToValue();
                            waitFileSection.Message = result.Message;
                        }
                    }
                    else
                    {
                        waitFileSection.Status = TouchSocketSmtpStatus.ResourceHandleNotFind.ToValue();
                    }
                }
                catch (Exception ex)
                {
                    waitFileSection.Status = TouchSocketSmtpStatus.Exception.ToValue();
                    waitFileSection.Message = ex.Message;
                }

                waitFileSection.Value = default;
                using (var byteBlock = new ByteBlock())
                {
                    waitFileSection.SwitchId();
                    waitFileSection.Package(byteBlock);
                    this.SmtpActor.Send(this.m_pushFileSection_Response, byteBlock);
                }
            }
            catch
            {
            }
        }

        #endregion 传输

        #region 小文件

        /// <summary>
        /// 允许传输的小文件的最大长度。默认1024*1024字节。
        /// <para>注意，当调整该值时，应该和对端保持一致。</para>
        /// </summary>
        public int MaxSmallFileLength { get; set; } = 1024 * 1024;

        /// <inheritdoc/>
        public PullSmallFileResult PullSmallFile(string targetId, string path, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            if (this.SmtpActor.AllowRoute && this.TryFindSmtpFileTransferActor(targetId, out var actor))
            {
                return actor.PullSmallFile(path, metadata, timeout, token);
            }
            else
            {
                return this.PrivatePullSmallFile(targetId, path, metadata, timeout, token);
            }
        }

        /// <inheritdoc/>
        public PullSmallFileResult PullSmallFile(string path, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePullSmallFile(default, path, metadata, timeout, token);
        }

        /// <inheritdoc/>
        public Task<PullSmallFileResult> PullSmallFileAsync(string targetId, string path, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullSmallFile(targetId, path, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Task<PullSmallFileResult> PullSmallFileAsync(string path, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullSmallFile(path, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Result PushSmallFile(string targetId, string savePath, FileInfo fileInfo, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            if (this.SmtpActor.AllowRoute && this.TryFindSmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.PushSmallFile(savePath, fileInfo, metadata, timeout, token);
            }
            else
            {
                return this.PrivatePushSmallFile(targetId, savePath, fileInfo, metadata, timeout, token);
            }
        }

        /// <inheritdoc/>
        public Result PushSmallFile(string savePath, FileInfo fileInfo, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePushSmallFile(default, savePath, fileInfo, metadata, timeout, token);
        }

        /// <inheritdoc/>
        public Task<Result> PushSmallFileAsync(string targetId, string savePath, FileInfo fileInfo, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushSmallFile(targetId, savePath, fileInfo, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Task<Result> PushSmallFileAsync(string savePath, FileInfo fileInfo, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushSmallFile(savePath, fileInfo, metadata, timeout, token);
            });
        }

        private PullSmallFileResult PrivatePullSmallFile(string targetId, string path, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            var waitSmallFilePackage = new WaitSmallFilePackage()
            {
                Path = path,
                SourceId = this.SmtpActor.Id,
                TargetId = targetId,
                Route = targetId.HasValue(),
                Metadata = metadata
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetWaitData(waitSmallFilePackage);

            var byteBlock = new ByteBlock();
            try
            {
                waitSmallFilePackage.Package(byteBlock);
                this.SmtpActor.Send(this.m_pullSmallFile_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitSmallFilePackage)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketSmtpStatus.Success:
                                    {
                                        return new PullSmallFileResult(waitFile.Data);
                                    }
                                case TouchSocketSmtpStatus.FileNotExists:
                                    {
                                        return new PullSmallFileResult(ResultCode.Error, TouchSocketSmtpStatus.FileNotExists.GetDescription(waitFile.Path));
                                    }
                                case TouchSocketSmtpStatus.RemoteRefuse:
                                case TouchSocketSmtpStatus.LengthErrorWhenRead:
                                case TouchSocketSmtpStatus.FileLengthTooLong:
                                case TouchSocketSmtpStatus.ClientNotFind:
                                default:
                                    {
                                        return new PullSmallFileResult(ResultCode.Error, TouchSocketSmtpStatus.RemoteRefuse.GetDescription(waitFile.Message));
                                    }
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return new PullSmallFileResult(ResultCode.Overtime);
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return new PullSmallFileResult(ResultCode.Canceled);
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return new PullSmallFileResult(ResultCode.Error, TouchSocketSmtpStatus.UnknownError.GetDescription());
                        }
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private Result PrivatePushSmallFile(string targetId, string savePath, FileInfo fileInfo, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            if (!File.Exists(fileInfo.FullName))
            {
                return new Result(ResultCode.Error, TouchSocketSmtpStatus.FileNotExists.GetDescription(fileInfo.FullName));
            }
            if (fileInfo.Length > this.MaxSmallFileLength)
            {
                return new Result(ResultCode.Error, TouchSocketSmtpStatus.FileLengthTooLong.GetDescription());
            }
            var waitSmallFilePackage = new WaitSmallFilePackage()
            {
                Path = savePath,
                SourceId = this.SmtpActor.Id,
                Metadata = metadata,
                Route = targetId.HasValue(),
                TargetId = targetId,
                FileInfo = new RemoteFileInfo(fileInfo)
            };

            var waitData = this.SmtpActor.WaitHandlePool.GetWaitData(waitSmallFilePackage);

            var byteBlock = new ByteBlock();
            var buffer = BytePool.Default.Rent((int)fileInfo.Length);
            try
            {
                var r = this.FileController.ReadAllBytes(fileInfo, buffer);
                if (r <= 0)
                {
                    return new Result(ResultCode.Error, TouchSocketSmtpStatus.LengthErrorWhenRead.GetDescription());
                }
                waitSmallFilePackage.Data = buffer;
                waitSmallFilePackage.Len = r;

                waitSmallFilePackage.Package(byteBlock);
                this.SmtpActor.Send(this.m_pushSmallFile_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitSmallFilePackage)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketSmtpStatus.Success:
                                    {
                                        return Result.Success;
                                    }
                                case TouchSocketSmtpStatus.RemoteRefuse:
                                    {
                                        return new Result(ResultCode.Error, TouchSocketSmtpStatus.RemoteRefuse.GetDescription(waitFile.Message));
                                    }
                                case TouchSocketSmtpStatus.ClientNotFind:
                                    {
                                        return new Result(ResultCode.Error, TouchSocketSmtpStatus.ClientNotFind.GetDescription());
                                    }
                                default:
                                    return new Result(ResultCode.Exception, waitFile.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return Result.Overtime;
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return Result.Canceled;
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return Result.UnknownFail;
                        }
                }
            }
            finally
            {
                this.SmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
                BytePool.Default.Return(buffer);
            }
        }

        private void RequestPullSmallFile(object o)
        {
            //2.不响应
            //3.不允许
            //4.不存在
            //5.读取文件长度异常

            var buffer = BytePool.Default.Rent(this.MaxSmallFileLength);
            try
            {
                var waitSmallFilePackage = (WaitSmallFilePackage)o;
                var resultThis = Result.Success;
                try
                {
                    var args = new FileOperationEventArgs(TransferType.SmallPull,
                        waitSmallFilePackage.Metadata, default)
                    {
                        ResourcePath = waitSmallFilePackage.Path
                    };

                    this.OnFileTransfering?.Invoke(this.SmtpActor, args);

                    var fullPath = this.FileController.GetFullPath(this.RootPath, args.ResourcePath);
                    waitSmallFilePackage.Path = fullPath;
                    if (args.IsPermitOperation)
                    {
                        if (File.Exists(fullPath))
                        {
                            var fileInfo = new FileInfo(fullPath);
                            if (fileInfo.Length > this.MaxSmallFileLength)
                            {
                                waitSmallFilePackage.Status = TouchSocketSmtpStatus.FileLengthTooLong.ToValue();
                                resultThis = new Result(ResultCode.Error, TouchSocketSmtpStatus.FileLengthTooLong.GetDescription());
                            }
                            else
                            {
                                var r = this.FileController.ReadAllBytes(fileInfo, buffer);
                                if (r > 0)
                                {
                                    waitSmallFilePackage.Data = buffer;
                                    waitSmallFilePackage.Len = r;
                                    waitSmallFilePackage.FileInfo = new RemoteFileInfo(fileInfo);
                                    waitSmallFilePackage.Status = TouchSocketSmtpStatus.Success.ToValue();
                                }
                                else
                                {
                                    waitSmallFilePackage.Status = TouchSocketSmtpStatus.LengthErrorWhenRead.ToValue();
                                    resultThis = new Result(ResultCode.Error, TouchSocketSmtpStatus.LengthErrorWhenRead.GetDescription());
                                }
                            }
                        }
                        else
                        {
                            waitSmallFilePackage.Status = TouchSocketSmtpStatus.FileNotExists.ToValue();
                            resultThis = new Result(ResultCode.Error, TouchSocketSmtpStatus.FileNotExists.GetDescription(fullPath));
                        }
                    }
                    else
                    {
                        waitSmallFilePackage.Status = TouchSocketSmtpStatus.RemoteRefuse.ToValue();
                        waitSmallFilePackage.Message = args.Message;
                        resultThis = new Result(ResultCode.Error, TouchSocketSmtpStatus.RemoteRefuse.GetDescription(args.Message));
                    }
                }
                catch (Exception ex)
                {
                    waitSmallFilePackage.Status = TouchSocketSmtpStatus.Exception.ToValue();
                    waitSmallFilePackage.Message = ex.Message;
                }

                using (var byteBlock = new ByteBlock())
                {
                    waitSmallFilePackage.SwitchId();
                    waitSmallFilePackage.Package(byteBlock);
                    this.SmtpActor.Send(this.m_pullSmallFile_Response, byteBlock);
                }

                var resultArgs = new FileTransferStatusEventArgs(
                    TransferType.SmallPull, waitSmallFilePackage.Metadata, waitSmallFilePackage.FileInfo, resultThis)
                {
                    ResourcePath = waitSmallFilePackage.Path
                };
                this.OnFileTransfered?.Invoke(this.SmtpActor, resultArgs);
            }
            catch
            {
            }
            finally
            {
                BytePool.Default.Return(buffer);
            }
        }

        private void RequestPushSmallFile(object o)
        {
            //2.不响应
            //3.不允许

            try
            {
                var waitSmallFilePackage = (WaitSmallFilePackage)o;
                var resultThis = Result.Success;
                try
                {
                    var args = new FileOperationEventArgs(TransferType.SmallPush,
                        waitSmallFilePackage.Metadata, waitSmallFilePackage.FileInfo)
                    {
                        SavePath = waitSmallFilePackage.Path
                    };

                    this.OnFileTransfering?.Invoke(this.SmtpActor, args);

                    var fullPath = this.FileController.GetFullPath(this.RootPath, args.SavePath);
                    waitSmallFilePackage.Path = fullPath;
                    if (args.IsPermitOperation)
                    {
                        this.FileController.WriteAllBytes(fullPath, waitSmallFilePackage.Data, 0, waitSmallFilePackage.Data.Length);
                        waitSmallFilePackage.Status = TouchSocketSmtpStatus.Success.ToValue();
                    }
                    else
                    {
                        waitSmallFilePackage.Status = TouchSocketSmtpStatus.RemoteRefuse.ToValue();
                        waitSmallFilePackage.Message = args.Message;
                        resultThis = new Result(ResultCode.Error, TouchSocketSmtpStatus.RemoteRefuse.GetDescription(args.Message));
                    }
                }
                catch (Exception ex)
                {
                    waitSmallFilePackage.Status = TouchSocketSmtpStatus.Exception.ToValue();
                    waitSmallFilePackage.Message = ex.Message;
                }
                waitSmallFilePackage.FileInfo = default;
                waitSmallFilePackage.Data = default;
                waitSmallFilePackage.SwitchId();
                using (var byteBlock = new ByteBlock())
                {
                    waitSmallFilePackage.Package(byteBlock);
                    this.SmtpActor.Send(this.m_pushSmallFile_Response, byteBlock);
                }

                var resultArgs = new FileTransferStatusEventArgs(
                    TransferType.SmallPush, waitSmallFilePackage.Metadata, waitSmallFilePackage.FileInfo, resultThis)
                {
                    SavePath = waitSmallFilePackage.Path
                };
                this.OnFileTransfered?.Invoke(this.SmtpActor, resultArgs);
            }
            catch
            {
            }
        }

        #endregion 小文件

        #region Id文件传输

        /// <inheritdoc/>
        public Result PullFile(string targetId, FileOperator fileOperator)
        {
            if (fileOperator is null)
            {
                return fileOperator.SetResult(new Result(ResultCode.Error, TouchSocketCoreResource.ArgumentNull.GetDescription(nameof(fileOperator))));
            }
            if (fileOperator.IsEnd)
            {
                fileOperator.SetResult(Result.Default);
            }

            try
            {
                var resourceInfoResult = this.PullFileResourceInfo(targetId, fileOperator.ResourcePath, fileOperator.Metadata, fileOperator.FileSectionSize, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                if (resourceInfoResult.NotSuccess())
                {
                    return new Result(resourceInfoResult);
                }
                var resourceInfo = resourceInfoResult.FileResourceInfo;
                if (fileOperator.ResourceInfo == null)
                {
                    fileOperator.SetLength(resourceInfo.FileInfo.Length);
                    fileOperator.ResourceInfo = resourceInfo;
                }
                else
                {
                    fileOperator.ResourceInfo.ResetResourceHandle(resourceInfo.ResourceHandle);
                }

                var locator = new FileResourceLocator(fileOperator.ResourceInfo, fileOperator.SavePath);
                var sections = new ConcurrentQueue<FileSection>();
                foreach (var item in locator.FileResourceInfo.FileSections)
                {
                    if (item.Status != FileSectionStatus.Finished)
                    {
                        sections.Enqueue(item);
                    }
                }

                var failResult = Result.UnknownFail;
                var task = Task.Run(async () =>
                {
                    var failed = 0;
                    while (true)
                    {
                        if (failed > fileOperator.TryCount)
                        {
                            break;
                        }
                        if (!sections.TryDequeue(out var fileSection))
                        {
                            break;
                        }
                        try
                        {
                            var result = this.PullFileSection(targetId, fileSection, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                            if (result.IsSuccess())
                            {
                                for (var j = 0; j < fileOperator.TryCount; j++)
                                {
                                    var res = locator.WriteFileSection(result);
                                    if (res.IsSuccess())
                                    {
                                        fileOperator.AddFlow(fileSection.Length);
                                        failed = 0;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                failResult = new Result(result);
                                if (result.ResultCode == ResultCode.Canceled)
                                {
                                    return;
                                }
                                failed++;
                                sections.Enqueue(fileSection);
                                await Task.Delay(500);
                            }
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            failResult = new Result(ex);
                            await Task.Delay(500);
                        }
                    }
                });
                task.Wait();
                if (fileOperator.Token.IsCancellationRequested)
                {
                    return fileOperator.SetResult(Result.Canceled);
                }
                var result1 = locator.TryFinished();
                if (this.SmtpActor.IsHandshaked)
                {
                    this.FinishedFileResourceInfo(targetId, resourceInfo);
                }

                return result1.IsSuccess() ? fileOperator.SetResult(Result.Success) : fileOperator.SetResult(failResult);
            }
            catch (Exception ex)
            {
                return fileOperator.SetResult(new Result(ex));
            }
        }

        /// <inheritdoc/>
        public Task<Result> PullFileAsync(string targetId, FileOperator fileOperator)
        {
            return Task.Run(() =>
            {
                return PullFile(targetId, fileOperator);
            });
        }

        /// <inheritdoc/>
        public Result PushFile(string targetId, FileOperator fileOperator)
        {
            if (fileOperator is null)
            {
                return fileOperator.SetResult(new Result(ResultCode.Error, TouchSocketCoreResource.ArgumentNull.GetDescription(nameof(fileOperator))));
            }
            if (fileOperator.IsEnd)
            {
                fileOperator.SetResult(Result.Default);
            }

            try
            {
                if (fileOperator.ResourceInfo == null)
                {
                    fileOperator.ResourceInfo = new FileResourceInfo(fileOperator.ResourcePath, fileOperator.FileSectionSize);
                    fileOperator.SetLength(fileOperator.ResourceInfo.FileInfo.Length);
                }
                using var locator = new FileResourceLocator(fileOperator.ResourceInfo);
                var resultInfo = this.PushFileResourceInfo(targetId, fileOperator.SavePath, locator, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                if (resultInfo.NotSuccess())
                {
                    return fileOperator.SetResult(resultInfo);
                }
                var sections = new ConcurrentQueue<FileSection>();
                foreach (var item in locator.FileResourceInfo.FileSections)
                {
                    if (item.Status != FileSectionStatus.Finished)
                    {
                        sections.Enqueue(item);
                    }
                }

                var failResult = Result.UnknownFail;
                var task = Task.Run(async () =>
                {
                    var failed = 0;
                    while (true)
                    {
                        if (failed > fileOperator.TryCount)
                        {
                            break;
                        }
                        if (!sections.TryDequeue(out var fileSection))
                        {
                            break;
                        }
                        try
                        {
                            var result = this.PushFileSection(targetId, locator, fileSection, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                            if (result.IsSuccess())
                            {
                                fileOperator.AddFlow(fileSection.Length);
                            }
                            else
                            {
                                failResult = new Result(result);
                                if (result.ResultCode == ResultCode.Canceled)
                                {
                                    return;
                                }
                                failed++;
                                sections.Enqueue(fileSection);
                                await Task.Delay(500);
                            }
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            failResult = new Result(ex);
                            await Task.Delay(500);
                        }
                    }
                });
                task.Wait();
                var res = this.FinishedFileResourceInfo(targetId, fileOperator.ResourceInfo);
                return fileOperator.Token.IsCancellationRequested ? fileOperator.SetResult(Result.Canceled) : fileOperator.SetResult(res.ToResult());
            }
            catch (Exception ex)
            {
                return fileOperator.SetResult(new Result(ex));
            }
        }

        /// <inheritdoc/>
        public Task<Result> PushFileAsync(string targetId, FileOperator fileOperator)
        {
            return Task.Run(() =>
            {
                return PushFile(targetId, fileOperator);
            });
        }

        #endregion Id文件传输

        #region 文件传输

        /// <inheritdoc/>
        public Result PullFile(FileOperator fileOperator)
        {
            if (fileOperator is null)
            {
                return fileOperator.SetResult(new Result(ResultCode.Error, TouchSocketCoreResource.ArgumentNull.GetDescription(nameof(fileOperator))));
            }
            if (fileOperator.IsEnd)
            {
                fileOperator.SetResult(Result.Default);
            }

            try
            {
                var resourceInfoResult = this.PullFileResourceInfo(fileOperator.ResourcePath, fileOperator.Metadata, fileOperator.FileSectionSize, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                if (resourceInfoResult.NotSuccess())
                {
                    return new Result(resourceInfoResult);
                }
                var resourceInfo = resourceInfoResult.FileResourceInfo;
                if (fileOperator.ResourceInfo == null)
                {
                    fileOperator.SetLength(resourceInfo.FileInfo.Length);
                    fileOperator.ResourceInfo = resourceInfo;
                }
                else
                {
                    fileOperator.ResourceInfo.ResetResourceHandle(resourceInfo.ResourceHandle);
                }

                var locator = new FileResourceLocator(fileOperator.ResourceInfo, fileOperator.SavePath);
                var sections = new ConcurrentQueue<FileSection>();
                foreach (var item in locator.FileResourceInfo.FileSections)
                {
                    if (item.Status != FileSectionStatus.Finished)
                    {
                        sections.Enqueue(item);
                    }
                }

                var failResult = Result.UnknownFail;

                var task = Task.Run(async () =>
                {
                    var failed = 0;
                    while (true)
                    {
                        if (failed > fileOperator.TryCount)
                        {
                            break;
                        }
                        if (!sections.TryDequeue(out var fileSection))
                        {
                            break;
                        }
                        try
                        {
                            var result = this.PullFileSection(fileSection, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                            if (result.IsSuccess())
                            {
                                for (var j = 0; j < fileOperator.TryCount; j++)
                                {
                                    var res = locator.WriteFileSection(result);
                                    if (res.IsSuccess())
                                    {
                                        fileOperator.AddFlow(fileSection.Length);
                                        failed = 0;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                failResult = new Result(result);
                                if (result.ResultCode == ResultCode.Canceled)
                                {
                                    return;
                                }
                                failed++;
                                sections.Enqueue(fileSection);
                                await Task.Delay(500);
                            }
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            failResult = new Result(ex);
                            await Task.Delay(500);
                        }
                    }
                });
                task.Wait();
                if (fileOperator.Token.IsCancellationRequested)
                {
                    return fileOperator.SetResult(Result.Canceled);
                }
                var result1 = locator.TryFinished();
                if (this.SmtpActor.IsHandshaked)
                {
                    this.FinishedFileResourceInfo(resourceInfo);
                }

                return result1.IsSuccess() ? fileOperator.SetResult(Result.Success) : fileOperator.SetResult(failResult);
            }
            catch (Exception ex)
            {
                return fileOperator.SetResult(new Result(ex));
            }
        }

        /// <inheritdoc/>
        public Task<Result> PullFileAsync(FileOperator fileOperator)
        {
            return Task.Run(() =>
            {
                return PullFile(fileOperator);
            });
        }

        /// <inheritdoc/>
        public Result PushFile(FileOperator fileOperator)
        {
            if (fileOperator is null)
            {
                return fileOperator.SetResult(new Result(ResultCode.Error, TouchSocketCoreResource.ArgumentNull.GetDescription(nameof(fileOperator))));
            }
            if (fileOperator.IsEnd)
            {
                fileOperator.SetResult(Result.Default);
            }

            try
            {
                if (fileOperator.ResourceInfo == null)
                {
                    fileOperator.ResourceInfo = new FileResourceInfo(fileOperator.ResourcePath, fileOperator.FileSectionSize);
                    fileOperator.SetLength(fileOperator.ResourceInfo.FileInfo.Length);
                }
                using var locator = new FileResourceLocator(fileOperator.ResourceInfo);
                var resultInfo = this.PushFileResourceInfo(fileOperator.SavePath, locator, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                if (resultInfo.NotSuccess())
                {
                    return fileOperator.SetResult(resultInfo);
                }
                var sections = new ConcurrentQueue<FileSection>();
                foreach (var item in locator.FileResourceInfo.FileSections)
                {
                    if (item.Status != FileSectionStatus.Finished)
                    {
                        sections.Enqueue(item);
                    }
                }

                var failResult = Result.UnknownFail;
                var task = Task.Run(async () =>
                {
                    var failed = 0;
                    while (true)
                    {
                        if (failed > fileOperator.TryCount)
                        {
                            break;
                        }
                        if (!sections.TryDequeue(out var fileSection))
                        {
                            break;
                        }
                        try
                        {
                            var result = this.PushFileSection(locator, fileSection, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                            if (result.IsSuccess())
                            {
                                fileOperator.AddFlow(fileSection.Length);
                            }
                            else
                            {
                                failResult = new Result(result);
                                if (result.ResultCode == ResultCode.Canceled)
                                {
                                    return;
                                }
                                failed++;
                                sections.Enqueue(fileSection);
                                await Task.Delay(500);
                            }
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            failResult = new Result(ex);
                            await Task.Delay(500);
                        }
                    }
                });
                task.Wait();

                var res = this.FinishedFileResourceInfo(fileOperator.ResourceInfo);
                return fileOperator.Token.IsCancellationRequested ? fileOperator.SetResult(Result.Canceled) : fileOperator.SetResult(res.ToResult());
            }
            catch (Exception ex)
            {
                return fileOperator.SetResult(new Result(ex));
            }
        }

        /// <inheritdoc/>
        public Task<Result> PushFileAsync(FileOperator fileOperator)
        {
            return Task.Run(() =>
            {
                return PushFile(fileOperator);
            });
        }

        #endregion 文件传输
    }
}