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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// DmtpFileTransferActorExtension
    /// </summary>
    public static class DmtpFileTransferActorExtension
    {
        #region 插件扩展

        /// <summary>
        /// 使用DmtpFileTransfer插件
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public static DmtpFileTransferFeature UseDmtpFileTransfer(this IPluginManager pluginManager)
        {
            return pluginManager.Add<DmtpFileTransferFeature>();
        }

        #endregion 插件扩展

        #region DependencyProperty

        /// <summary>
        /// DmtpFileTransferActor
        /// </summary>
        public static readonly DependencyProperty<IDmtpFileTransferActor> DmtpFileTransferActorProperty =
            DependencyProperty<IDmtpFileTransferActor>.Register("DmtpFileTransferActor", default);

        #endregion DependencyProperty

        /// <summary>
        /// 从<see cref="DmtpActor"/>中获取<see cref="IDmtpFileTransferActor"/>
        /// </summary>
        /// <param name="dmtpActor"></param>
        /// <returns></returns>
        public static IDmtpFileTransferActor GetDmtpFileTransferActor(this IDmtpActor dmtpActor)
        {
            return dmtpActor.GetValue(DmtpFileTransferActorProperty);
        }

        /// <summary>
        /// 向<see cref="DmtpActor"/>中设置<see cref="DmtpFileTransferActor"/>
        /// </summary>
        /// <param name="dmtpActor"></param>
        /// <param name="dmtpRpcActor"></param>
        internal static void SetDmtpFileTransferActor(this IDmtpActor dmtpActor, DmtpFileTransferActor dmtpRpcActor)
        {
            dmtpActor.SetValue(DmtpFileTransferActorProperty, dmtpRpcActor);
        }

        /// <summary>
        /// 从<see cref="IDmtpActorObject"/>中获取能实现文件传输的<see cref="IDmtpFileTransferActor"/>
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IDmtpFileTransferActor GetDmtpFileTransferActor(this IDmtpActorObject client)
        {
            var actor = client.DmtpActor.GetDmtpFileTransferActor();
            if (actor is null)
            {
                throw new ArgumentNullException(nameof(actor), TouchSocketDmtpResource.DmtpFileTransferActorNull.GetDescription());
            }
            else
            {
                return actor;
            }
        }

        #region Id文件传输

        /// <inheritdoc/>
        public static Result PullFile(this IDmtpFileTransferActor actor, string targetId, FileOperator fileOperator)
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
                var resourceInfoResult = actor.PullFileResourceInfo(targetId, fileOperator.ResourcePath, fileOperator.Metadata, fileOperator.FileSectionSize, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                if (resourceInfoResult.NotSuccess())
                {
                    return fileOperator.SetResult(new Result(resourceInfoResult));
                }
                var resourceInfo = resourceInfoResult.FileResourceInfo;
                if (fileOperator.ResourceInfo == null)
                {
                    fileOperator.ResourceInfo = resourceInfo;
                }
                else
                {
                    fileOperator.ResourceInfo.ResetResourceHandle(resourceInfo.ResourceHandle);
                }

                fileOperator.SetLength(resourceInfo.FileInfo.Length);

                var locator = new FileResourceLocator(fileOperator.ResourceInfo, actor.FileController.GetFullPath(actor.RootPath, fileOperator.SavePath));
                var sections = new ConcurrentStack<FileSection>();

                var fileSections = locator.FileResourceInfo.FileSections;

                for (var i = fileSections.Length - 1; i >= 0; i--)
                {
                    var fileSection = fileSections[i];
                    if (fileSection.Status != FileSectionStatus.Finished)
                    {
                        sections.Push(fileSection);
                    }
                    else
                    {
                        fileOperator.AddCompletedLength(fileSection.Length);
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
                        if (!sections.TryPop(out var fileSection))
                        {
                            break;
                        }
                        try
                        {
                            using (var result = actor.PullFileSection(targetId, fileSection, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token))
                            {
                                if (result.IsSuccess())
                                {
                                    for (var j = 0; j < fileOperator.TryCount; j++)
                                    {
                                        var res = locator.WriteFileSection(result);
                                        if (res.IsSuccess())
                                        {
                                            await fileOperator.AddFlowAsync(fileSection.Length);
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
                                    sections.Push(fileSection);
                                    await Task.Delay(500);
                                }
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
                task.ConfigureAwait(false).GetAwaiter().GetResult();
                if (fileOperator.Token.IsCancellationRequested)
                {
                    if (actor.DmtpActor.IsHandshaked)
                    {
                        actor.FinishedFileResourceInfo(targetId, resourceInfo, ResultCode.Canceled, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                    }
                    return fileOperator.SetResult(Result.Canceled);
                }
                var result1 = locator.TryFinished();
                if (actor.DmtpActor.IsHandshaked)
                {
                    actor.FinishedFileResourceInfo(targetId, resourceInfo, ResultCode.Success, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                }

                return result1.IsSuccess() ? fileOperator.SetResult(Result.Success) : fileOperator.SetResult(failResult);
            }
            catch (Exception ex)
            {
                return fileOperator.SetResult(new Result(ex));
            }
        }

        /// <inheritdoc/>
        public static Task<Result> PullFileAsync(this IDmtpFileTransferActor actor, string targetId, FileOperator fileOperator)
        {
            return Task.Run(() =>
            {
                return actor.PullFile(targetId, fileOperator);
            });
        }

        /// <inheritdoc/>
        public static Result PushFile(this IDmtpFileTransferActor actor, string targetId, FileOperator fileOperator)
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
                fileOperator.ResourceInfo ??= new FileResourceInfo(actor.FileController.GetFullPath(actor.RootPath, fileOperator.ResourcePath), fileOperator.FileSectionSize);

                fileOperator.SetLength(fileOperator.ResourceInfo.FileInfo.Length);

                using var locator = new FileResourceLocator(fileOperator.ResourceInfo);
                var resultInfo = actor.PushFileResourceInfo(targetId, fileOperator.SavePath, locator, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                if (resultInfo.NotSuccess())
                {
                    return fileOperator.SetResult(resultInfo);
                }
                var sections = new ConcurrentStack<FileSection>();

                var fileSections = locator.FileResourceInfo.FileSections;

                for (var i = fileSections.Length - 1; i >= 0; i--)
                {
                    var fileSection = fileSections[i];
                    if (fileSection.Status != FileSectionStatus.Finished)
                    {
                        sections.Push(fileSection);
                    }
                    else
                    {
                        fileOperator.AddCompletedLength(fileSection.Length);
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
                        if (!sections.TryPop(out var fileSection))
                        {
                            break;
                        }
                        try
                        {
                            var result = actor.PushFileSection(targetId, locator, fileSection, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                            if (result.IsSuccess())
                            {
                                await fileOperator.AddFlowAsync(fileSection.Length);
                            }
                            else
                            {
                                failResult = new Result(result);
                                if (result.ResultCode == ResultCode.Canceled)
                                {
                                    return;
                                }
                                failed++;
                                sections.Push(fileSection);
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
                task.ConfigureAwait(false).GetAwaiter().GetResult();

                if (fileOperator.Token.IsCancellationRequested)
                {
                    actor.FinishedFileResourceInfo(targetId, fileOperator.ResourceInfo, ResultCode.Canceled, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                    return fileOperator.SetResult(Result.Canceled);
                }
                else
                {
                    var res = actor.FinishedFileResourceInfo(targetId, fileOperator.ResourceInfo, ResultCode.Success, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                    return fileOperator.SetResult(res.ToResult());
                }
            }
            catch (Exception ex)
            {
                return fileOperator.SetResult(new Result(ex));
            }
        }

        /// <inheritdoc/>
        public static Task<Result> PushFileAsync(this IDmtpFileTransferActor actor, string targetId, FileOperator fileOperator)
        {
            return Task.Run(() =>
            {
                return actor.PushFile(targetId, fileOperator);
            });
        }

        #endregion Id文件传输

        #region 文件传输

        /// <inheritdoc/>
        public static Result PullFile(this IDmtpFileTransferActor actor, FileOperator fileOperator)
        {
            return PullFile(actor, default, fileOperator);
        }

        /// <inheritdoc/>
        public static Task<Result> PullFileAsync(this IDmtpFileTransferActor actor, FileOperator fileOperator)
        {
            return PullFileAsync(actor, default, fileOperator);
        }

        /// <inheritdoc/>
        public static Result PushFile(this IDmtpFileTransferActor actor, FileOperator fileOperator)
        {
            return PushFile(actor, default, fileOperator);
        }

        /// <inheritdoc/>
        public static Task<Result> PushFileAsync(this IDmtpFileTransferActor actor, FileOperator fileOperator)
        {
            return PushFileAsync(actor, default, fileOperator);
        }

        #endregion 文件传输
    }
}