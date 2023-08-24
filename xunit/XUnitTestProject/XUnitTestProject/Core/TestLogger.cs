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