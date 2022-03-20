//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Serialization;
using System;
using System.IO;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件流
    /// </summary>
    public class RRQMStream : FileStream
    {
        internal RRQMStream(string path, FileMode mode, FileAccess access) : base(path, mode, access)
        {
        }

        internal int reference;
        internal RRQMFileInfo fileInfo;
        internal StreamOperationType streamType;
        internal string rrqmPath;
        internal object owner;

        /// <summary>
        /// 文件信息
        /// </summary>
        public RRQMFileInfo FileInfo => this.fileInfo;

        /// <summary>
        /// 流类型
        /// </summary>
        public StreamOperationType StreamType => this.streamType;

        /// <summary>
        /// 不支持操作
        /// </summary>
        public new void Dispose()
        {
            throw new InvalidOperationException($"RRQM流不允许直接释放，请在{nameof(RRQMStreamPool)}中操作。");
        }

        internal void AbsoluteDispose()
        {
            base.Dispose();
        }

        internal bool FinishStream(bool overwrite)
        {
            string path = this.rrqmPath.Replace(".rrqm", string.Empty);
            if (overwrite && File.Exists(path))
            {
                File.Delete(path);
            }
            this.SetLength(this.fileInfo.FileLength);
            this.Flush();
            base.Dispose();
            File.Move(this.rrqmPath, path);
            path = this.rrqmPath.Replace(".rrqm", string.Empty) + ".temp";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return true;
        }

        private static int saveInterval = 1000;

        /// <summary>
        /// 进度保存时间，默认1000毫秒。
        /// </summary>
        public static int SaveInterval
        {
            get => saveInterval;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                saveInterval = value;
            }
        }

        private TimeSpan lastTime;

        /// <summary>
        /// 保存进度
        /// </summary>
        public void SaveProgress()
        {
            if (this.streamType == StreamOperationType.RRQMWrite)
            {
                if (DateTime.Now.TimeOfDay - this.lastTime > TimeSpan.FromMilliseconds(saveInterval))
                {
                    SerializeConvert.XmlSerializeToFile(this.fileInfo, this.rrqmPath.Replace(".rrqm", string.Empty) + ".temp");
                    this.lastTime = DateTime.Now.TimeOfDay;
                }
            }
        }
    }
}