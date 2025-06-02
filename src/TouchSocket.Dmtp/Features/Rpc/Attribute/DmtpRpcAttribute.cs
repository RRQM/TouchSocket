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
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc;

/// <summary>
/// DmtpRpc方法标记属性类
/// </summary>
[DynamicMethod]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class DmtpRpcAttribute : RpcAttribute
{
    /// <summary>
    ///  适用于DmtpRpc的标记.
    ///  <para>是否仅以函数名调用，当为<see langword="true"/>是，调用时仅需要传入方法名即可。</para>
    /// </summary>
    /// <param name="methodInvoke">指示是否仅通过方法名进行调用的布尔值。</param>
    [Obsolete("由于构造函数直接设置参数在源生成时效果不一致，所以取消该方式，如果想要设置参数，请使用属性直接设置，例如：MethodInvoke=true", true)]
    public DmtpRpcAttribute(bool methodInvoke)
    {
        this.MethodInvoke = methodInvoke;
    }

    /// <summary>
    /// 适用于DmtpRpc的标记
    /// </summary>
    public DmtpRpcAttribute()
    {
    }

    /// <summary>
    /// 适用于DmtpRpc的标记.
    /// </summary>
    /// <param name="invokeKey">用于指定DmtpRpc调用的关键字</param>
    [Obsolete("由于构造函数直接设置参数在源生成时效果不一致，所以取消该方式，如果想要设置参数，请使用属性直接设置，例如：MethodInvoke=true", true)]
    public DmtpRpcAttribute(string invokeKey)
    {
        this.InvokeKey = invokeKey;
    }

    /// <inheritdoc/>
    public override Type[] GetGenericConstraintTypes()
    {
        return new Type[] { typeof(IDmtpRpcActor) };
    }
}