//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TouchSocket.Core;

//namespace TouchSocket.Dmtp.FileTransfer
//{
//    /// <summary>
//    /// 文件传输操作器。
//    /// </summary>
//    public class ResponseFileOperator : FlowOperator
//    {
//        public ResponseFileOperator(FileResourceInfo resourceInfo)
//        {
//            this.ResourceInfo = resourceInfo;
//        }

//        /// <summary>
//        /// <inheritdoc/>但是当前控制器不支持该操作。
//        /// </summary>
//        public override long MaxSpeed { get =>throw new NotSupportedException("该控制器不支持该操作"); set => throw new NotSupportedException("该控制器不支持该操作"); }

//        /// <summary>
//        /// 文件分块大小。
//        /// </summary>
//        public int FileSectionSize=>this.ResourceInfo.FileSectionSize;

//        /// <summary>
//        /// 文件资源信息。
//        /// </summary>
//        public FileResourceInfo ResourceInfo { get;}

//        internal Result SetResult(Result result)
//        {
//            this.Result = result;
//            return result;
//        }

//        internal Task AddFlowAsync(int flow)
//        {
//            return this.ProtectedAddFlowAsync(flow);
//        }

//        internal void AddCompletedLength(int flow)
//        {
//            this.completedLength += flow;
//        }

//        /// <summary>
//        /// 设置流长度
//        /// </summary>
//        /// <param name="len"></param>
//        internal void SetLength(long len)
//        {
//            this.Length = len;
//        }
//    }
//}