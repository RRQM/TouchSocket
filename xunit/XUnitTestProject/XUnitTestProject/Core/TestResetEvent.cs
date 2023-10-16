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