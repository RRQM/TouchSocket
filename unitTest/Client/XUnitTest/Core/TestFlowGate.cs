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
using System;
using TouchSocket.Core;
using TouchSocket.Core.Diagnostics;
using Xunit;

namespace XUnitTest.Core
{
    public class TestFlowGate
    {
        [Fact]
        public void FlowShouldBeOk()
        {
            try
            {
                Enterprise.ForTest();
            }
            catch (Exception)
            {
            }
            FlowGate flowGate = new FlowGate();
            flowGate.Maximum = 1024 * 1024;
            flowGate.MaximumPeriod = 1000;

            int sum = 1024 * 1024 * 10;

            TimeSpan timeSpan = TimeSpan.FromSeconds(3);

            var time = TimeMeasurer.Run(() =>
              {
                  while (sum > 0)
                  {
                      int step = 1024 * 1024;
                      flowGate.AddCheckWait(step);
                      sum -= step;
                  }
              });

            Assert.True(Math.Abs(timeSpan.Seconds - time.Seconds) < 2);
        }

        [Fact]
        public void FlowShouldBeOk2()
        {
            try
            {
                Enterprise.ForTest();
            }
            catch (Exception)
            {
            }
            FlowGate flowGate = new FlowGate();
            flowGate.Maximum = 10;
            flowGate.MaximumPeriod = 2000;

            int sum = 1000;

            TimeSpan timeSpan = TimeSpan.FromSeconds(10);

            var time = TimeMeasurer.Run(() =>
            {
                while (sum > 0)
                {
                    int step = 100;
                    flowGate.AddCheckWait(step);
                    sum -= step;
                }
            });

            Assert.True(Math.Abs(timeSpan.Seconds - time.Seconds) < 2);
        }
    }
}