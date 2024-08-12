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
    
    public class TestFilePool : UnitBase
    {
        [Fact]
        public void FileStorageWriterShouldBeOk()
        {
            var path = "ShouldCanSetPool.test";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var bytes = new List<byte[]>();

            using (var writer = FilePool.GetWriter(path))
            {
                for (var i = 0; i < 100; i++)
                {
                    var buffer = new byte[1024 * 1024];
                    new Random().NextBytes(buffer);
                    //byte[] buffer = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                    writer.Write(buffer);

                    bytes.Add(buffer);
                }
            }

            using (var pathReader = FilePool.GetReader(path))
            {
                var buffer1 = new byte[1024 * 1024];

                foreach (var item in bytes)
                {
                    var r1 = pathReader.Read(buffer1, 0, buffer1.Length);

                    Assert.True(r1 == buffer1.Length);
                    Assert.True(buffer1.SequenceEqual(item));
                }
            }
        }
    }
}