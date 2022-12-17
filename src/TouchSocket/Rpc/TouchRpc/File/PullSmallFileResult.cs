using System;
using TouchSocket.Core;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// PullSmallFileResult
    /// </summary>
    public class PullSmallFileResult : ResultBase
    {
        /// <summary>
        /// 初始化PullSmallFileResult
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="message"></param>
        public PullSmallFileResult(ResultCode resultCode, string message) : base(resultCode, message)
        {
        }

        /// <summary>
        /// 初始化PullSmallFileResult
        /// </summary>
        /// <param name="bytes"></param>
        public PullSmallFileResult(byte[] bytes) : base(ResultCode.Success)
        {
            Value = bytes;
        }

        /// <summary>
        /// 初始化PullSmallFileResult
        /// </summary>
        /// <param name="resultCode"></param>
        public PullSmallFileResult(ResultCode resultCode) : base(resultCode)
        {
        }

        /// <summary>
        /// 实际的文件数据
        /// </summary>
        public byte[] Value { get; private set; }

        /// <summary>
        /// 将拉取的数据保存为文件。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public Result Save(string path, bool overwrite = true)
        {
            try
            {
                if (overwrite)
                {
                    FileUtility.Delete(path);
                }
                using (FileStorageWriter writer = FilePool.GetWriter(path))
                {
                    writer.Write(Value);
                    return Result.Success;
                }
            }
            catch (Exception ex)
            {
                return new Result(ex);
            }
        }
    }
}
