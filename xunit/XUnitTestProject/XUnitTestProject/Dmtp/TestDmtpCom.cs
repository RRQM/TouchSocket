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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp.FileTransfer;

namespace XUnitTestProject.Dmtp
{
    public class TestDmtpCom:UnitBase
    {
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(1024)]
        [InlineData(1024*512)]
        [Theory]
        public void FileResourceInfoShouldBeOk(int fileSectionSize)
        {
            var path = "FileResourceInfoShouldBeOk";

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using (var writer = FilePool.GetWriter(path))
            {
                var buffer = new byte[1000000];
                new Random().NextBytes(buffer);
                writer.Write(buffer);
            }

            var fileResourceInfo = new FileResourceInfo(path, fileSectionSize);


            var byteBlock = new ByteBlock(1024*1024);
            fileResourceInfo.Save(byteBlock);

            byteBlock.SeekToStart();
            var fileResourceInfo1=new FileResourceInfo(byteBlock);

            Assert.Equal(fileResourceInfo1.FileSectionSize, fileResourceInfo.FileSectionSize);
            Assert.Equal(fileResourceInfo1.ResourceHandle, fileResourceInfo.ResourceHandle);
            Assert.Equal(fileResourceInfo1.FileSections.Length, fileResourceInfo.FileSections.Length);

            for (int i = 0; i < fileResourceInfo1.FileSections.Length; i++)
            {
                Assert.True(fileResourceInfo1.FileSections[i].Equals(fileResourceInfo.FileSections[i]));
            }
        }
    }
}