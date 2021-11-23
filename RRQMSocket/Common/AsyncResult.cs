//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
namespace RRQMSocket
{
    /// <summary>
    /// 结果实现类
    /// </summary>
    public class AsyncResult : IResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="message"></param>
        public AsyncResult(bool isSuccess, string message)
        {
            this.IsSuccess = isSuccess;
            this.Message = message;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isSuccess"></param>
        public AsyncResult(bool isSuccess)
        {
            this.IsSuccess = isSuccess;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public AsyncResult(string message)
        {
            this.Message = message;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsSuccess { get; private set; }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Message { get; private set; }


    }
}
