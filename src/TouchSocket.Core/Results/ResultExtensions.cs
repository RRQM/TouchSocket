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

namespace TouchSocket.Core;

/// <summary>
/// ResultExtensions
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// 是否成功。
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    [Obsolete("建议使用IsSuccess属性代替该扩展方法")]
    public static bool IsSuccess(this IResult result)
    {
        return result.ResultCode == ResultCode.Success;
    }

    /// <summary>
    /// 是否没有成功。
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    [Obsolete("建议使用IsSuccess属性取反代替该扩展方法")]
    public static bool NotSuccess(this IResult result)
    {
        return result.ResultCode != ResultCode.Success;
    }

    /// <summary>
    /// 转换为<see cref="Result"/>
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static Result ToResult(this IResult result)
    {
        return new Result(result);
    }
}