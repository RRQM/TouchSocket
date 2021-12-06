using RRQMCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件传输操作器
    /// </summary>
    public class FileOperator:StreamOperator
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FileOperator():base()
        {
        }

        internal void AddFileFlow(int flow, long length)
        {
            this.speedTemp += flow;
            this.completedLength += flow;
            this.progress = (float)((double)this.completedLength / length);
        }
        
        internal void SetFileCompletedLength(long completedLength)
        {
            this.completedLength = completedLength;
        }

        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal Result SetFileResult(Result result)
        {
            this.result = result;
            return result;
        }
    }
}
