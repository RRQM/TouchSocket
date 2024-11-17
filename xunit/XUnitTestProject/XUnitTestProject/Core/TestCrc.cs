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
    
    public class TestCrc
    {
        [Fact]
        public void Crc16ShouldBeOk()
        {
            var data = new byte[1024];
            new Random().NextBytes(data);

            var crc = Crc.Crc16(data);

            Assert.True(2 == crc.Length);
            for (var i = 0; i < 10; i++)
            {
                var newCrc = Crc.Crc16(data);
                for (var j = 0; j < newCrc.Length; j++)
                {
                    Assert.Equal(crc[j], newCrc[j]);
                }
            }
        }
    }
}