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
using System.IO;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Core.Extensions;
using TouchSocket.Core.IO;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 文件流
    /// </summary>
    internal class TouchRpcFileStream : DisposableObject
    {
        private static int m_saveInterval = 1000;

        private TouchRpcFileInfo m_fileInfo;

        private TimeSpan m_lastTime;

        private string m_path;

        private bool m_resume;

        private FileStorageWriter m_fileWriter;

        protected override void Dispose(bool disposing)
        {
            this.m_fileWriter.SafeDispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 进度保存时间，默认1000毫秒。
        /// </summary>
        public static int SaveInterval
        {
            get => m_saveInterval;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                m_saveInterval = value;
            }
        }

        public FileStorageWriter FileWriter => this.m_fileWriter;

        public static TouchRpcFileStream Create(string path, ref TouchRpcFileInfo fileInfo, bool resume)
        {
            TouchRpcFileStream stream = new TouchRpcFileStream();
            FileTool.TryReadTempInfo(path, ref fileInfo);
            stream.m_fileWriter = FilePool.GetWriter(path + ".rrqm", true);
            stream.m_fileWriter.Position = fileInfo.Position;
            stream.m_fileInfo = fileInfo;
            stream.m_path = path;
            stream.m_resume = resume;
            return stream;
        }

        public void Write(byte[] buffer, int offset, int length)
        {
            this.m_fileWriter.Write(buffer, offset, length);
            this.SaveProgress();
        }

        public void FinishStream()
        {
            if (this.m_fileWriter.Position != this.m_fileInfo.FileLength)
            {
                throw new Exception("已完成传输，但是文件长度不对。");
            }
            if (File.Exists(this.m_path))
            {
                File.Delete(this.m_path);
            }
            this.m_fileWriter.SafeDispose();

            string rrqmPath = this.m_path + ".rrqm";
            string tempPath = this.m_path + ".temp";

            if (!SpinWait.SpinUntil(() =>
            {
                File.Move(rrqmPath, this.m_path);
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                if (File.Exists(this.m_path))
                {
                    return true;
                }
                return false;
            }, 5000))
            {
                throw new IOException("已完成传输，但是在保存路径并未检测到文件存在。");
            }

        }

        /// <summary>
        /// 保存进度
        /// </summary>
        private void SaveProgress()
        {
            if (this.m_resume)
            {
                if (DateTime.Now.TimeOfDay - this.m_lastTime > TimeSpan.FromMilliseconds(m_saveInterval))
                {
                    try
                    {
                        File.WriteAllText(this.m_path + ".temp", this.m_fileInfo.ToJsonString());
                        this.m_lastTime = DateTime.Now.TimeOfDay;
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}