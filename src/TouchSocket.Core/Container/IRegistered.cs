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
/// IRegistered 接口定义了用于检查类型是否已注册的方法
/// </summary>
public interface IRegistered
{
    /// <summary>
    /// 判断某类型是否已经注册
    /// </summary>
    /// <param name="fromType">要检查的类型</param>
    /// <param name="key">与类型关联的唯一键，用于特定的注册场景</param>
    /// <returns>如果类型已注册，则返回 true；否则返回 false</returns>
    bool IsRegistered(Type fromType, string key);

    /// <summary>
    /// 判断某类型是否已经注册
    /// </summary>
    /// <param name="fromType">要检查的类型</param>
    /// <returns>如果类型已注册，则返回 true；否则返回 false</returns>
    bool IsRegistered(Type fromType);
}