// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace TouchSocket.Core;

/// <summary>
/// 表示动态方法的信息。
/// </summary>
public interface IDynamicMethodInfo
{
    /// <summary>
    /// 真实返回值类型。
    /// <para>当方法为 void 或 Task 时，为 null。</para>
    /// <para>当方法为 Task 泛型时，为泛型元素类型。</para>
    /// </summary>
    Type RealReturnType { get; }

    /// <summary>
    /// 返回值的 Task 类型。
    /// </summary>
    MethodReturnKind ReturnKind { get; }

    /// <summary>
    /// 异步获取方法的结果。
    /// </summary>
    /// <param name="result">方法的返回值。</param>
    /// <returns>异步任务，包含方法的结果。</returns>
    Task<object> GetResultAsync(object result);

    /// <summary>
    /// 调用方法。
    /// </summary>
    /// <param name="instance">方法所属的实例对象。</param>
    /// <param name="parameters">方法的参数。</param>
    /// <returns>方法的返回值。</returns>
    object Invoke(object instance, object[] parameters);
}