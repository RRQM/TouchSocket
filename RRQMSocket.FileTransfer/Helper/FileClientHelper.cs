//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.Exceptions;
using RRQMCore.Run;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer.Helper
{
    /// <summary>
    /// 文件客户端辅助扩展
    /// </summary>
    public static class FileClientHelper
    {
        /// <summary>
        /// 批量推送文件
        /// </summary>
        /// <param name="client">终端</param>
        /// <param name="multipleCount">并行数量</param>
        /// <param name="fileRequests">批量请求头</param>
        /// <param name="fileOperators">批量操作器</param>
        /// <param name="metadatas">批量元数据</param>
        public static void PushFiles(this IFileClient client, int multipleCount, FileRequest[] fileRequests, FileOperator[] fileOperators, Metadata[] metadatas)
        {
            if (multipleCount < 1)
            {
                throw new RRQMException("并行数量不能小于1。");
            }

            if (fileRequests is null)
            {
                throw new ArgumentNullException(nameof(fileRequests));
            }

            if (fileOperators is null)
            {
                throw new ArgumentNullException(nameof(fileOperators));
            }

            if (!(fileRequests.Length == fileOperators.Length && metadatas.Length == fileOperators.Length))
            {
                throw new RRQMException("FileRequest、FileOperator和Metadata数量必须一致。");
            }

            int index = 0;
            int t = 0;
            int complatedLen = 0;
            List<Task<Result>> results = new List<Task<Result>>();

            LoopAction loopAction = LoopAction.CreateLoopAction(-1, 100, (loop) =>
            {
                int st = multipleCount - t;
                for (int i = 0; i < st; i++)
                {
                    if (index == fileRequests.Length)
                    {
                        break;
                    }
                    Task<Result> result = client.PushFileAsync(fileRequests[index], fileOperators[index], metadatas[index]);
                    results.Add(result);
                    index++;
                    t++;
                }

                List<Task<Result>> cr = new List<Task<Result>>();

                foreach (var item in results)
                {
                    if (item.IsCompleted)
                    {
                        cr.Add(item);
                        t--;
                        complatedLen++;
                        if (complatedLen == fileRequests.Length)
                        {
                            loop.Dispose();
                        }
                    }
                }

                foreach (var item in cr)
                {
                    results.Remove(item);
                }
            });

            loopAction.Run();
        }

        /// <summary>
        /// 批量拉取文件
        /// </summary>
        /// <param name="client">终端</param>
        /// <param name="multipleCount">并行数量</param>
        /// <param name="fileRequests">批量请求头</param>
        /// <param name="fileOperators">批量操作器</param>
        /// <param name="metadatas">批量元数据</param>
        public static void PullFiles(this IFileClient client, int multipleCount, FileRequest[] fileRequests, FileOperator[] fileOperators, Metadata[] metadatas)
        {
            if (multipleCount < 1)
            {
                throw new RRQMException("并行数量不能小于1。");
            }

            if (fileRequests is null)
            {
                throw new ArgumentNullException(nameof(fileRequests));
            }

            if (fileOperators is null)
            {
                throw new ArgumentNullException(nameof(fileOperators));
            }

            if (!(fileRequests.Length == fileOperators.Length && metadatas.Length == fileOperators.Length))
            {
                throw new RRQMException("FileRequest、FileOperator和Metadata数量必须一致。");
            }

            int index = 0;
            int t = 0;
            int complatedLen = 0;
            List<Task<Result>> results = new List<Task<Result>>();

            LoopAction loopAction = LoopAction.CreateLoopAction(-1, 100, (loop) =>
            {
                int st = multipleCount - t;
                for (int i = 0; i < st; i++)
                {
                    if (index == fileRequests.Length)
                    {
                        break;
                    }
                    Task<Result> result = client.PullFileAsync(fileRequests[index], fileOperators[index], metadatas[index]);
                    results.Add(result);
                    index++;
                    t++;
                }

                List<Task<Result>> cr = new List<Task<Result>>();

                foreach (var item in results)
                {
                    if (item.IsCompleted)
                    {
                        cr.Add(item);
                        t--;
                        complatedLen++;
                        if (complatedLen == fileRequests.Length)
                        {
                            loop.Dispose();
                        }
                    }
                }

                foreach (var item in cr)
                {
                    results.Remove(item);
                }
            });

            loopAction.Run();
        }

        /// <summary>
        /// 异步批量推送文件
        /// </summary>
        /// <param name="client">终端</param>
        /// <param name="multipleCount">并行数量</param>
        /// <param name="fileRequests">批量请求头</param>
        /// <param name="fileOperators">批量操作器</param>
        /// <param name="metadatas">批量元数据</param>
        public static void PushFilesAsync(this IFileClient client, int multipleCount, FileRequest[] fileRequests, FileOperator[] fileOperators, Metadata[] metadatas)
        {
            if (multipleCount < 1)
            {
                throw new RRQMException("并行数量不能小于1。");
            }

            if (fileRequests is null)
            {
                throw new ArgumentNullException(nameof(fileRequests));
            }

            if (fileOperators is null)
            {
                throw new ArgumentNullException(nameof(fileOperators));
            }

            if (!(fileRequests.Length == fileOperators.Length && metadatas.Length == fileOperators.Length))
            {
                throw new RRQMException("FileRequest、FileOperator和Metadata数量必须一致。");
            }

            int index = 0;
            int t = 0;
            int complatedLen = 0;
            List<Task<Result>> results = new List<Task<Result>>();

            LoopAction loopAction = LoopAction.CreateLoopAction(-1, 100, (loop) =>
            {
                int st = multipleCount - t;
                for (int i = 0; i < st; i++)
                {
                    if (index == fileRequests.Length)
                    {
                        break;
                    }
                    Task<Result> result = client.PushFileAsync(fileRequests[index], fileOperators[index], metadatas[index]);
                    results.Add(result);
                    index++;
                    t++;
                }

                List<Task<Result>> cr = new List<Task<Result>>();

                foreach (var item in results)
                {
                    if (item.IsCompleted)
                    {
                        cr.Add(item);
                        t--;
                        complatedLen++;
                        if (complatedLen == fileRequests.Length)
                        {
                            loop.Dispose();
                        }
                    }
                }

                foreach (var item in cr)
                {
                    results.Remove(item);
                }
            });

            loopAction.RunAsync();
        }

        /// <summary>
        /// 异步批量拉取文件
        /// </summary>
        /// <param name="client">终端</param>
        /// <param name="multipleCount">并行数量</param>
        /// <param name="fileRequests">批量请求头</param>
        /// <param name="fileOperators">批量操作器</param>
        /// <param name="metadatas">批量元数据</param>
        public static void PullFilesAsync(this IFileClient client, int multipleCount, FileRequest[] fileRequests, FileOperator[] fileOperators, Metadata[] metadatas)
        {
            if (multipleCount < 1)
            {
                throw new RRQMException("并行数量不能小于1。");
            }

            if (fileRequests is null)
            {
                throw new ArgumentNullException(nameof(fileRequests));
            }

            if (fileOperators is null)
            {
                throw new ArgumentNullException(nameof(fileOperators));
            }

            if (!(fileRequests.Length == fileOperators.Length && metadatas.Length == fileOperators.Length))
            {
                throw new RRQMException("FileRequest、FileOperator和Metadata数量必须一致。");
            }

            int index = 0;
            int t = 0;
            int complatedLen = 0;
            List<Task<Result>> results = new List<Task<Result>>();

            LoopAction loopAction = LoopAction.CreateLoopAction(-1, 100, (loop) =>
            {
                int st = multipleCount - t;
                for (int i = 0; i < st; i++)
                {
                    if (index == fileRequests.Length)
                    {
                        break;
                    }
                    Task<Result> result = client.PullFileAsync(fileRequests[index], fileOperators[index], metadatas[index]);
                    results.Add(result);
                    index++;
                    t++;
                }

                List<Task<Result>> cr = new List<Task<Result>>();

                foreach (var item in results)
                {
                    if (item.IsCompleted)
                    {
                        cr.Add(item);
                        t--;
                        complatedLen++;
                        if (complatedLen == fileRequests.Length)
                        {
                            loop.Dispose();
                        }
                    }
                }

                foreach (var item in cr)
                {
                    results.Remove(item);
                }
            });

            loopAction.RunAsync();
        }
    }
}