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

namespace TouchSocket.Core
{
        /// <summary>
    /// IResolver 接口定义了如何解析类型实例。
    /// 它继承自 IServiceProvider 和 IRegistered 接口。
    /// </summary>
    public interface IResolver : IServiceProvider, IRegistered
    {
        /// <summary>
        /// 解析给定类型和键对应的实例。
        /// </summary>
        /// <param name="fromType">要解析的目标类型。</param>
        /// <param name="key">可选的实例标识符。</param>
        /// <returns>解析出的实例。</returns>
        object Resolve(Type fromType, string key);

        /// <summary>
        /// 解析给定类型的实例，不使用键。
        /// </summary>
        /// <param name="fromType">要解析的目标类型。</param>
        /// <returns>解析出的实例。</returns>
        object Resolve(Type fromType);
    }
}