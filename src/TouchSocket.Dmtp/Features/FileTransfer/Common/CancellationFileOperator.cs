using System;
using System.Threading;

namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// 可取消文件传输操作器
    /// </summary>
    public class CancellationFileOperator : FileOperator
    {
        private readonly CancellationTokenSource m_tokenSource=new CancellationTokenSource();

        /// <inheritdoc/>
        public override CancellationToken Token
        {
            get
            {
                return this.m_tokenSource.Token;
            }

            set => throw new NotSupportedException();
        }

        /// <summary>
        /// 取消传输
        /// </summary>
        public void Cancel()
        {
            this.m_tokenSource.Cancel();
        }
        
        /// <summary>
        /// 在指定的时间之后取消传输。
        /// </summary>
        /// <param name="delay"></param>
        public void CancelAfter(TimeSpan delay)
        {
            this.m_tokenSource.CancelAfter(delay);
        }
    }
}
