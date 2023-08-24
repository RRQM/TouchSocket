//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Core;

namespace XUnitTestProject.Core
{
    public class TestFlowGate
    {
        [Theory]
        [InlineData(1000, 10)]
        [InlineData(1024 * 1024, 10)]
        public void FlowShouldBeOk(int sum, int step)
        {
            var speed = (long)((double)sum / 10);
            var flowGate = new FlowGate();
            flowGate.Maximum = speed;
            flowGate.MaximumWaitTime = TimeSpan.FromSeconds(1);

            var timeSpan = TimeSpan.FromSeconds((double)sum / speed);

            var time = TimeMeasurer.Run(() =>
              {
                  while (sum > 0)
                  {
                      flowGate.AddCheckWait(step);
                      sum -= step;
                  }
              });

            Assert.True(Math.Abs(timeSpan.Seconds - time.Seconds) < 3);
        }

        [Theory]
        [InlineData(1000, 10)]
        [InlineData(1024 * 1024, 10)]
        public async Task FlowAsyncShouldBeOk(int sum, int step)
        {
            var speed = (long)((double)sum / 10);
            var flowGate = new FlowGate();
            flowGate.Maximum = speed;
            flowGate.MaximumWaitTime = TimeSpan.FromSeconds(1);

            var timeSpan = TimeSpan.FromSeconds((double)sum / speed);

            var time = await TimeMeasurer.RunAsync(async () =>
            {
                while (sum > 0)
                {
                    await flowGate.AddCheckWaitAsync(step);
                    sum -= step;
                }
            });

            Assert.True(Math.Abs(timeSpan.Seconds - time.Seconds) < 3);
        }

        [Theory]
        [InlineData(1000, 200)]
        [InlineData(1024 * 1024, 1024 * 5)]
        public void FlowShouldBeOk2(int sum, int step)
        {
            var speed = (long)((double)sum / 10);
            var flowGate = new FlowGate();
            flowGate.Maximum = speed;
            flowGate.MaximumWaitTime = TimeSpan.FromSeconds(10);

            var timeSpan = TimeSpan.FromSeconds((double)sum / speed);

            var time = TimeMeasurer.Run(() =>
            {
                while (sum > 0)
                {
                    flowGate.AddCheckWait(step);
                    sum -= step;
                }
            });

            Assert.True(Math.Abs(timeSpan.Seconds - time.Seconds) < 3);
        }

        [Theory]
        [InlineData(1000, 10)]
        [InlineData(1024 * 1024, 10)]
        public async Task FlowAsyncShouldBeOk2(int sum, int step)
        {
            var speed = (long)((double)sum / 10);
            var flowGate = new FlowGate();
            flowGate.Maximum = speed;
            flowGate.MaximumWaitTime = TimeSpan.FromSeconds(10);

            var timeSpan = TimeSpan.FromSeconds((double)sum / speed);

            var time = await TimeMeasurer.RunAsync(async () =>
            {
                while (sum > 0)
                {
                    await flowGate.AddCheckWaitAsync(step);
                    sum -= step;
                }
            });

            Assert.True(Math.Abs(timeSpan.Seconds - time.Seconds) < 3);
        }
    }
}