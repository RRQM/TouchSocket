namespace TouchSocket.Dmtp.FileTransfer
{
    /// <summary>
    /// TransferTypeExtension
    /// </summary>
    public static class TransferTypeExtension
    {
        /// <summary>
        /// 表示当前传输类型是否属于<see cref="TransferType.Pull"/>、<see cref="TransferType.SmallPull"/>其中的一种。
        /// </summary>
        /// <param name="transferType"></param>
        /// <returns></returns>
        public static bool IsPull(this TransferType transferType)
        {
            return transferType == TransferType.Pull || transferType == TransferType.SmallPull;
        }
    }
}