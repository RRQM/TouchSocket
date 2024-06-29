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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    public class StaticEntry
    {
        public StaticEntry(byte[] value, TimeSpan timeout)
        {
            this.Value = value;
            this.Timespan = timeout;
        }

        public StaticEntry(FileInfo fileInfo, TimeSpan timespan)
        {
            this.FileInfo = fileInfo;
            this.Timespan = timespan;
        }

        public bool IsCacheBytes => this.Value != null;

        public FileInfo FileInfo { get; set; }
        public byte[] Value { get; }
        public TimeSpan Timespan { get; set; }
    }
}
