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

using TouchSocket.Core;

namespace XUnitTestProject.Core
{
    
    public class TestQueue
    {
        [Theory]
        [InlineData(1000, 100)]
        [InlineData(100, 1000)]
        [InlineData(100, 10000)]
        [InlineData(100, 100000)]
        public void TriggerQueueShouldBeOk(int time, int count)
        {
            for (var j = 0; j < time; j++)
            {
                var ints = new List<int>();
                var queue = new TriggerQueue<int>();
                EventWaitHandle waitHandle = new AutoResetEvent(false);
                queue.OnDequeue = (data) =>
                {
                    ints.Add(data);
                    if (ints.Count == count)
                    {
                        waitHandle.Set();
                    }
                };

                for (var i = 0; i < count; i++)
                {
                    queue.Enqueue(i);
                }
                if (!waitHandle.WaitOne(2000))
                {
                    throw new TimeoutException();
                }

                Assert.True(ints.Count == count);
                for (var i = 0; i < count; i++)
                {
                    Assert.True(ints[i] == i);
                }
                GC.Collect();
            }
        }
    }
}