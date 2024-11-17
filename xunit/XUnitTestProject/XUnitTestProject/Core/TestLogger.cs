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
    
    public class TestLogger : UnitBase
    {
        [Fact]
        public void FileLoggerShouldBeOk()
        {
            var dir1 = Guid.NewGuid().ToString();
            var dir2 = Guid.NewGuid().ToString();
            var fileLogger1 = new FileLogger(dir1);
            var fileLogger2 = new FileLogger(dir2);

            fileLogger1.LogLevel = LogLevel.Info;
            fileLogger2.LogLevel = LogLevel.Info;

            fileLogger1.Debug("Debug");
            fileLogger2.Debug("Debug");

            Assert.False(Directory.Exists(dir1));
            Assert.False(Directory.Exists(dir2));

            fileLogger1.Info("Info");
            fileLogger2.Info("Info");

            Assert.True(Directory.Exists(dir1));
            Assert.True(Directory.Exists(dir2));

            Assert.True(Directory.GetFiles(dir1, "*.*", SearchOption.AllDirectories).Length > 0);
            Assert.True(Directory.GetFiles(dir2, "*.*", SearchOption.AllDirectories).Length > 0);

            fileLogger1.Dispose();
            fileLogger2.Dispose();

            Directory.Delete(dir1, true);
            Directory.Delete(dir2, true);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(1000)]
        [InlineData(100000)]
        [InlineData(1000000)]
        public void BigFileLoggerShouldBeOk(int count)
        {
            var dir1 = Guid.NewGuid().ToString();
            var fileLogger1 = new FileLogger(dir1);

            fileLogger1.LogLevel = LogLevel.Info;

            for (var i = 0; i < count; i++)
            {
                fileLogger1.Info("Info");
            }

            Assert.True(Directory.GetFiles(dir1, "*.*", SearchOption.AllDirectories).Length > 0);

            fileLogger1.Dispose();

            Directory.Delete(dir1, true);
        }
    }
}