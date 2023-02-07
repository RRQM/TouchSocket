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