// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace TouchSocket.Core;

internal static class InternalUtilities
{
    /// <summary>
    /// Removes an element from the middle of a queue without disrupting the other elements.
    /// </summary>
    /// <typeparam name="T">The element to remove.</typeparam>
    /// <param name="queue">The queue to modify.</param>
    /// <param name="valueToRemove">The value to remove.</param>
    /// <remarks>
    /// If a value appears multiple times in the queue, only its first entry is removed.
    /// </remarks>
    internal static bool RemoveMidQueue<T>(this Queue<T> queue, T valueToRemove)
        where T : class
    {
        var originalCount = queue.Count;
        var dequeueCounter = 0;
        var found = false;
        while (dequeueCounter < originalCount)
        {
            dequeueCounter++;
            var dequeued = queue.Dequeue();
            if (!found && dequeued == valueToRemove)
            { // only find 1 match
                found = true;
            }
            else
            {
                queue.Enqueue(dequeued);
            }
        }

        return found;
    }
}
