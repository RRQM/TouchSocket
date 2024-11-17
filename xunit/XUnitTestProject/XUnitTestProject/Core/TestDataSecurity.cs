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
    
    public class TestDataSecurity
    {
        [Fact]
        public void DESShouldBeOk()
        {
            var data = new byte[1024];
            new Random().NextBytes(data);
            var dataLocked = DataSecurity.EncryptDES(data, "12345678");//加密口令，长度为8。
            var newData = DataSecurity.DecryptDES(dataLocked, "12345678");//解密口令，和加密一致。

            for (var i = 0; i < data.Length; i++)
            {
                Assert.Equal(data[i], newData[i]);
            }
        }

        [Fact]
        public void DESStreamShouldBeOk()
        {
            var memoryStream = new MemoryStream();

            var data = new byte[1024];
            new Random().NextBytes(data);
            memoryStream.Write(data);
            memoryStream.Position = 0;

            var memoryStreamLocked = new MemoryStream();

            DataSecurity.StreamEncryptDES(memoryStream, memoryStreamLocked, "12345678");//加密口令，长度为8。

            memoryStreamLocked.Position = 0;
            memoryStream.Position = 0;
            memoryStream.SetLength(0);
            DataSecurity.StreamDecryptDES(memoryStreamLocked, memoryStream, "12345678");

            memoryStream.Position = 0;
            for (var i = 0; i < data.Length; i++)
            {
                Assert.Equal(data[i], memoryStream.ReadByte());
            }
        }
    }
}