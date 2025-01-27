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

using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// 结果返回
/// </summary>
public class ResultBase : IResult
{

    /// <summary>
    /// 初始化 ResultBase 类的新实例。
    /// </summary>
    /// <param name="resultCode">结果代码，表示操作的结果。</param>
    /// <param name="message">消息，提供有关操作结果的详细信息。</param>
    public ResultBase(ResultCode resultCode, string message)
    {
        this.ResultCode = resultCode;
        this.Message = message;
    }


    /// <summary>
    /// 初始化 ResultBase 类的新实例。
    /// </summary>
    /// <param name="resultCode">结果代码，表示操作的结果。</param>
    public ResultBase(ResultCode resultCode)
    {
        // 将传入的结果代码赋值给类的 ResultCode 属性。
        this.ResultCode = resultCode;

        // 根据结果代码获取其描述信息，并赋值给 Message 属性。
        this.Message = resultCode.GetDescription();
    }

    /// <summary>
    /// 初始化 ResultBase 类的新实例。
    /// </summary>
    /// <param name="result">要复制其属性值的 Result 对象。</param>
    public ResultBase(Result result)
    {
        // 复制传入 Result 对象的 ResultCode 属性值。
        this.ResultCode = result.ResultCode;

        // 复制传入 Result 对象的 Message 属性值。
        this.Message = result.Message;
    }


    /// <summary>
    /// ResultBase 类的构造函数。
    /// </summary>
    public ResultBase()
    {
    }

    /// <inheritdoc/>
    public ResultCode ResultCode { get; protected set; }

    /// <inheritdoc/>
    public string Message { get; protected set; }

    /// <inheritdoc/>
    public bool IsSuccess => this.ResultCode == ResultCode.Success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return TouchSocketCoreResource.ResultToString.Format(this.ResultCode, this.Message);
    }
}
