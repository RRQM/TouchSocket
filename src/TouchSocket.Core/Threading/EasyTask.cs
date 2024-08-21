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

using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{

        /// <summary>
        /// EasyTask 类简化了对异步任务的处理，提供了简便的静态方法来创建和操作任务。
        /// </summary>
        public static class EasyTask
        {
                /// <summary>
                /// EasyTask 类的静态构造函数，在类加载时初始化 CompletedTask 属性。
                /// </summary>
                static EasyTask()
                {
#if NET45_OR_GREATER
                        // 在 .NET 4.5 或更高版本中，直接使用 Task.FromResult 方法创建已完成的任务。
                        CompletedTask = Task.FromResult(0);
#else
            // 在 .NET 4.5 以下版本中，使用 Task.CompletedTask 属性获取已完成的任务。
            CompletedTask = Task.CompletedTask;
#endif
                }

                /// <summary>
                /// 获取一个已成功完成的空 Task 对象。
                /// </summary>
                public static Task CompletedTask { get; }

                /// <summary>
                /// 根据提供的取消令牌创建一个已取消的 Task。
                /// </summary>
                /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
                /// <returns>一个表示已取消任务的 Task 对象。</returns>
                public static Task FromCanceled(CancellationToken cancellationToken)
                {
#if NET45
            // 在 .NET 4.5 版本中，使用 TaskCompletionSource 来创建并标记一个任务为已取消。
            var tcs = new TaskCompletionSource<bool>();
            tcs.TrySetCanceled();
            return tcs.Task;
#else
                        // 在 .NET 4.5 以上版本中，直接使用 Task.FromCanceled 方法创建已取消的任务。
                        return Task.FromCanceled(cancellationToken);
#endif
                }

                /// <summary>
                /// 根据提供的取消令牌创建一个已取消的 Task，该任务返回指定类型的结果。
                /// </summary>
                /// <typeparam name="T">任务返回的结果类型。</typeparam>
                /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
                /// <returns>一个表示已取消任务的 Task 对象，带有指定类型的结果。</returns>
                public static Task<T> FromCanceled<T>(CancellationToken cancellationToken)
                {
#if NET45
            // 在 .NET 4.5 版本中，使用 TaskCompletionSource 来创建并标记一个带有结果类型的已取消任务。
            var tcs = new TaskCompletionSource<T>();
            tcs.TrySetCanceled();
            return tcs.Task;
#else
                        // 在 .NET 4.5 以上版本中，直接使用 Task.FromCanceled 方法创建带有结果类型的已取消任务。
                        return Task.FromCanceled<T>(cancellationToken);
#endif
                }
        }
}