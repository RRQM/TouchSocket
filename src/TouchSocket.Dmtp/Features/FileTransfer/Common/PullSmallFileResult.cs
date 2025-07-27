//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.FileTransfer;

/// <summary>
/// 表示拉取小文件操作的结果，继承自结果基类
/// </summary>
public class PullSmallFileResult : ResultBase
{
    /// <summary>
    /// 初始化PullSmallFileResult对象。
    /// 该构造函数用于创建PullSmallFileResult实例，根据结果代码和消息初始化对象。
    /// </summary>
    /// <param name="resultCode">结果代码，表示操作的执行结果。</param>
    /// <param name="message">伴随结果的详细信息或错误消息。</param>
    public PullSmallFileResult(ResultCode resultCode, string message) : base(resultCode, message)
    {
    }

    /// <summary>
    /// 初始化PullSmallFileResult对象。
    /// 该构造函数用于创建一个表示文件拉取结果的实例，特别针对小型文件。
    /// </summary>
    /// <param name="bytes">byte数组，包含被拉取文件的内容。</param>
    public PullSmallFileResult(byte[] bytes) : base(ResultCode.Success)
    {
        this.Value = bytes;
    }
    /// <summary>
    /// 初始化PullSmallFileResult对象。
    /// 此构造函数用于为PullSmallFileResult对象设置初始的状态码。
    /// </summary>
    /// <param name="resultCode">结果码，用于指示文件拉取操作的结果。</param>
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
    /// <param name="path">要保存文件的路径。</param>
    /// <param name="overwrite">是否覆盖同名文件，默认为<see langword="true"/>。</param>
    /// <returns>返回保存结果。</returns>
    public Result Save(string path, bool overwrite = true)
    {
        try
        {
            if (overwrite)
            {
                FileUtility.Delete(path);
            }
            using (var byteBlock = FilePool.GetWriter(path))
            {
                byteBlock.Write(this.Value);
                return Result.Success;
            }
        }
        catch (Exception ex)
        {
            return new Result(ex);
        }
    }
}