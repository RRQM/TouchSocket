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
    
    public class TestResetEvent
    {
        [Fact]
        public async Task AsyncAutoResetEventShouldBeOkForOvertime()
        {
            var dateTime = DateTime.Now;

            var asyncAutoResetEvent = new AsyncAutoResetEvent();
            var b = await asyncAutoResetEvent.WaitOneAsync(TimeSpan.FromSeconds(2));
            Assert.True(DateTime.Now - dateTime < TimeSpan.FromSeconds(1 + 2));
            Assert.False(b);
        }

        [Fact]
        public async Task AsyncAutoResetEventShouldBeOkForSet()
        {
            var dateTime = DateTime.Now;

            var asyncAutoResetEvent = new AsyncAutoResetEvent();
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                asyncAutoResetEvent.Set();
            });
            var b = await asyncAutoResetEvent.WaitOneAsync(TimeSpan.FromSeconds(10));
            Assert.True(DateTime.Now - dateTime < TimeSpan.FromSeconds(1 + 2));
            Assert.True(b);
        }

        [Fact]
        public async Task AsyncAutoResetEventShouldBeOkForDispose()
        {
            var dateTime = DateTime.Now;

            var asyncAutoResetEvent = new AsyncAutoResetEvent();
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                asyncAutoResetEvent.Dispose();
            });
            var b = await asyncAutoResetEvent.WaitOneAsync(TimeSpan.FromSeconds(10));
            Assert.True(DateTime.Now - dateTime < TimeSpan.FromSeconds(1 + 2));
            Assert.True(b);
        }

        [Fact]
        public async Task AsyncAutoResetEventShouldBeOkForMultipleSet()
        {
            var dateTime = DateTime.Now;

            var asyncAutoResetEvent = new AsyncAutoResetEvent();
            var sets = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                sets.Add(Task.Run(async () =>
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(2000);
                        asyncAutoResetEvent.Set();
                    });
                    var b = await asyncAutoResetEvent.WaitOneAsync(TimeSpan.FromSeconds(10));
                    Assert.True(DateTime.Now - dateTime < TimeSpan.FromSeconds(1 + 2));
                    Assert.True(b);
                }));
            }

            await Task.WhenAll(sets.ToArray());
        }

        [Fact]
        public async Task AsyncAutoResetEventShouldBeOkForMultipleDispose()
        {
            var dateTime = DateTime.Now;

            var asyncAutoResetEvent = new AsyncAutoResetEvent();

            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                asyncAutoResetEvent.Dispose();
            });

            var sets = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                sets.Add(Task.Run(async () =>
                {
                    var b = await asyncAutoResetEvent.WaitOneAsync(TimeSpan.FromSeconds(10));
                    Assert.True(DateTime.Now - dateTime < TimeSpan.FromSeconds(1 + 2));
                    Assert.True(b);
                }));
            }

            await Task.WhenAll(sets.ToArray());
        }
    }
}