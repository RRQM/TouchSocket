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
    /// 定义一个静态类，用于扩展Dmtp文件传输的功能
    /// </summary>
    public static class DmtpFileTransferActorExtension
    {
        #region 插件扩展

        /// <summary>
        /// 使用DmtpFileTransfer插件
        /// </summary>
        /// <param name="pluginManager">插件管理器实例，用于管理插件的加载和执行</param>
        /// <returns>返回DmtpFileTransferFeature实例，以便进一步操作或配置</returns>
        public static DmtpFileTransferFeature UseDmtpFileTransfer(this IPluginManager pluginManager)
        {
            // 通过插件管理器添加DmtpFileTransfer插件，并返回插件实例
            return pluginManager.Add<DmtpFileTransferFeature>();
        }

        #endregion 插件扩展

        #region DependencyProperty

        /// <summary>
        /// DmtpFileTransferActor
        /// </summary>
        public static readonly DependencyProperty<IDmtpFileTransferActor> DmtpFileTransferActorProperty =
            new("DmtpFileTransferActor", default);

        #endregion DependencyProperty

        /// <summary>
        /// 从<see cref="DmtpActor"/>中获取<see cref="IDmtpFileTransferActor"/>
        /// </summary>
        /// <param name="dmtpActor">要从中获取文件传输演员的<see cref="IDmtpActor"/>实例。</param>
        /// <returns>返回一个<see cref="IDmtpFileTransferActor"/>实例。</returns>
        public static IDmtpFileTransferActor GetDmtpFileTransferActor(this IDmtpActor dmtpActor)
        {
            // 通过DmtpFileTransferActorProperty属性从dmtpActor中获取IDmtpFileTransferActor实例
            return dmtpActor.GetValue(DmtpFileTransferActorProperty);
        }

        /// <summary>
        /// 向<see cref="DmtpActor"/>中设置<see cref="DmtpFileTransferActor"/>
        /// </summary>
        /// <param name="dmtpActor">要设置<see cref="DmtpFileTransferActor"/>的<see cref="IDmtpActor"/>对象</param>
        /// <param name="dmtpRpcActor">要设置的<see cref="DmtpFileTransferActor"/>实例</param>
        internal static void SetDmtpFileTransferActor(this IDmtpActor dmtpActor, DmtpFileTransferActor dmtpRpcActor)
        {
            // 使用DependencyProperty进行设置，确保可以在WPF等框架中作为属性绑定使用
            dmtpActor.SetValue(DmtpFileTransferActorProperty, dmtpRpcActor);
        }

        /// <summary>
        /// 从<see cref="IDmtpActorObject"/>中获取能实现文件传输的<see cref="IDmtpFileTransferActor"/>
        /// </summary>
        /// <param name="client">实现文件传输功能的对象</param>
        /// <returns>返回一个可进行文件传输的演员对象</returns>
        /// <exception cref="ArgumentNullException">如果内部演员对象为空，则抛出此异常</exception>
        public static IDmtpFileTransferActor GetDmtpFileTransferActor(this IDmtpActorObject client)
        {
            // 从client中获取Dmtp文件传输演员
            var actor = client.DmtpActor.GetDmtpFileTransferActor();
            // 检查获取的演员是否为空
            if (actor is null)
            {
                // 如果为空，则抛出ArgumentNullException异常
                throw new ArgumentNullException(nameof(actor), TouchSocketDmtpResource.DmtpFileTransferActorNull);
            }
            else
            {
                // 返回获取的演员对象
                return actor;
            }
        }

        #region Id文件传输


        /// <summary>
        /// 同步拉取文件操作的同步封装方法。
        /// </summary>
        /// <param name="actor">参与文件传输操作的角色。</param>
        /// <param name="targetId">目标文件的标识。</param>
        /// <param name="fileOperator">文件操作对象，用于处理文件传输过程中的具体操作。</param>
        /// <returns>返回文件拉取操作的结果。</returns>
        public static Result PullFile(this IDmtpFileTransferActor actor, string targetId, FileOperator fileOperator)
        {
            return PullFileAsync(actor, targetId, fileOperator).GetFalseAwaitResult();
        }


        /// <summary>
        /// 异步拉取文件
        /// </summary>
        /// <param name="actor">文件传输行为者</param>
        /// <param name="targetId">目标ID，标识需要拉取的文件</param>
        /// <param name="fileOperator">文件操作器，用于处理文件操作</param>
        /// <returns>返回一个异步任务，包含操作结果</returns>
        /// <remarks>
        /// 该方法为文件传输行为者提供了一种异步拉取文件的方式，通过指定目标ID和文件操作器来完成文件的拉取操作。
        /// </remarks>
        public static async Task<Result> PullFileAsync(this IDmtpFileTransferActor actor, string targetId, FileOperator fileOperator)
        {
            if (fileOperator is null)
            {
                return fileOperator.SetResult(new Result(ResultCode.Error, TouchSocketCoreResource.ArgumentIsNull.Format(nameof(fileOperator))));
            }
            if (fileOperator.IsEnd)
            {
                fileOperator.SetResult(Result.Default);
            }

            try
            {
                var resourceInfoResult = await actor.PullFileResourceInfoAsync(targetId, fileOperator.ResourcePath, fileOperator.Metadata, fileOperator.FileSectionSize, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (!resourceInfoResult.IsSuccess)
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
                await Task.Run(async () =>
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
                             using (var result = await actor.PullFileSectionAsync(targetId, fileSection, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                             {
                                 if (result.IsSuccess)
                                 {
                                     for (var j = 0; j < fileOperator.TryCount; j++)
                                     {
                                         var res = locator.WriteFileSection(result);
                                         if (res.IsSuccess)
                                         {
                                             await fileOperator.AddFlowAsync(fileSection.Length).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                                     await Task.Delay(500).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                                 }
                             }
                         }
                         catch (Exception ex)
                         {
                             failed++;
                             failResult = new Result(ex);
                             await Task.Delay(500).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                         }
                     }
                 }).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (fileOperator.Token.IsCancellationRequested)
                {
                    if (actor.DmtpActor.Online)
                    {
                        await actor.FinishedFileResourceInfoAsync(targetId, resourceInfo, ResultCode.Canceled, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                    return fileOperator.SetResult(Result.Canceled);
                }
                var result1 = locator.TryFinished();
                if (actor.DmtpActor.Online)
                {
                    await actor.FinishedFileResourceInfoAsync(targetId, resourceInfo, ResultCode.Success, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }

                return result1.IsSuccess ? fileOperator.SetResult(Result.Success) : fileOperator.SetResult(failResult);
            }
            catch (Exception ex)
            {
                return fileOperator.SetResult(new Result(ex));
            }
        }


        /// <summary>
        /// 异步推送文件到指定目标
        /// </summary>
        /// <param name="actor">执行文件推送操作的演员</param>
        /// <param name="targetId">目标标识符，标识文件推送的目的地</param>
        /// <param name="fileOperator">文件操作对象，包含待推送的文件信息和操作方法</param>
        /// <returns>返回文件推送操作的结果</returns>
        public static Result PushFile(this IDmtpFileTransferActor actor, string targetId, FileOperator fileOperator)
        {
            return PushFileAsync(actor, targetId, fileOperator).GetFalseAwaitResult();
        }

        /// <summary>
        /// 异步推送文件到指定目标。
        /// </summary>
        /// <param name="actor">参与文件传输的行为者。</param>
        /// <param name="targetId">目标设备的唯一标识符。</param>
        /// <param name="fileOperator">用于操作文件的对象。</param>
        /// <returns>返回一个异步任务，该任务完成后会提供推送操作的结果。</returns>
        public static async Task<Result> PushFileAsync(this IDmtpFileTransferActor actor, string targetId, FileOperator fileOperator)
        {
            if (fileOperator is null)
            {
                return fileOperator.SetResult(new Result(ResultCode.Error, TouchSocketCoreResource.ArgumentIsNull.Format(nameof(fileOperator))));
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
                var resultInfo = await actor.PushFileResourceInfoAsync(targetId, fileOperator.SavePath, locator, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                if (!resultInfo.IsSuccess)
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
                await Task.Run(async () =>
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
                            var result = await actor.PushFileSectionAsync(targetId, locator, fileSection, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                            if (result.IsSuccess)
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

                if (fileOperator.Token.IsCancellationRequested)
                {
                    await actor.FinishedFileResourceInfoAsync(targetId, fileOperator.ResourceInfo, ResultCode.Canceled, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                    return fileOperator.SetResult(Result.Canceled);
                }
                else
                {
                    var res = await actor.FinishedFileResourceInfoAsync(targetId, fileOperator.ResourceInfo, ResultCode.Success, fileOperator.Metadata, (int)fileOperator.Timeout.TotalMilliseconds, fileOperator.Token);
                    return fileOperator.SetResult(res.ToResult());
                }
            }
            catch (Exception ex)
            {
                return fileOperator.SetResult(new Result(ex));
            }
        }

        #endregion Id文件传输

        #region 文件传输

        /// <summary>
        /// 从远程服务器拉取文件。
        /// </summary>
        /// <param name="actor">提供文件传输功能的actor。</param>
        /// <param name="fileOperator">用于处理文件的文件操作器。</param>
        /// <returns>返回一个<see cref="Result"/>类型的值，包含操作的结果信息。</returns>
        public static Result PullFile(this IDmtpFileTransferActor actor, FileOperator fileOperator)
        {
            return PullFile(actor, default, fileOperator);
        }


        /// <summary>
        /// 异步拉取文件
        /// </summary>
        /// <param name="actor">文件传输行为者</param>
        /// <param name="fileOperator">文件操作对象，用于处理文件操作</param>
        /// <returns>返回一个异步任务，任务完成后返回操作结果</returns>
        public static Task<Result> PullFileAsync(this IDmtpFileTransferActor actor, FileOperator fileOperator)
        {
            // 调用重载的PullFileAsync方法，使用默认值进行异步文件拉取操作
            return PullFileAsync(actor, default, fileOperator);
        }

        /// <summary>
        /// 扩展方法，用于推送文件。
        /// </summary>
        /// <param name="actor">实现文件传输操作的演员。</param>
        /// <param name="fileOperator">文件操作对象，用于指定文件操作。</param>
        /// <returns>返回文件推送操作的结果。</returns>
        public static Result PushFile(this IDmtpFileTransferActor actor, FileOperator fileOperator)
        {
            // 调用重载的PushFile方法，使用默认值进行文件推送操作
            return PushFile(actor, default, fileOperator);
        }


        /// <summary>
        /// 异步推送文件方法
        /// </summary>
        /// <param name="actor">文件传输行为者</param>
        /// <param name="fileOperator">文件操作器，用于处理文件操作</param>
        /// <returns>返回一个异步任务，该任务表示文件推送操作的结果</returns>
        public static Task<Result> PushFileAsync(this IDmtpFileTransferActor actor, FileOperator fileOperator)
        {
            // 调用重载的PushFileAsync方法，使用默认值进行文件推送
            return PushFileAsync(actor, default, fileOperator);
        }

        #endregion 文件传输
    }
}