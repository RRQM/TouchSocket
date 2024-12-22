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

namespace TouchSocket.Core
{
    /// <summary>
    /// 泛型接口<see cref="IResult{T}"/>，继承自<see cref="IResult"/>。
    /// 用于定义具有特定类型结果值的对象所需的行为。
    /// </summary>
    /// <typeparam name="T">结果值的类型。</typeparam>
    public interface IResult<T> : IResult
    {
        /// <summary>
        /// 获取结果值。
        /// </summary>
        /// <value>结果值，类型为 T。</value>
        T Value { get; }
    }
}
