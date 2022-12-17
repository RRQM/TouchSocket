using System.IO;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// 文件资源控制器。
    /// </summary>
    public interface IFileResourceController
    {
        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="md5"></param>
        /// <param name="fileInfo"></param>
        void GetFileInfo<T>(string path, bool md5, ref T fileInfo) where T : RemoteFileInfo;

        /// <summary>
        /// 获取全路径
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetFullPath(string root, string path);

        /// <summary>
        /// 读取缓存文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="flags"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        bool TryReadTempInfo(string path, TransferFlags flags, ref TouchRpcFileInfo info);

        /// <summary>
        /// 读取文件的所有数据
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        int ReadAllBytes(FileInfo fileInfo, byte[] buffer);

        /// <summary>
        /// 写入数据到文件
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void WriteAllBytes(FileInfo fileInfo, byte[] buffer, int offset, int length);
    }
}
