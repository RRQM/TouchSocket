namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 远程文件控制器。
    /// </summary>
    public class RemoteFileOperator : StreamOperator
    {
        internal void SetFileCompletedLength(long completedLength)
        {
            this.completedLength = completedLength;
        }

    }
}
