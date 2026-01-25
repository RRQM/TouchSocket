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

using System.Reflection;
using TouchSocket.Rpc;

namespace TouchSocket.XmlRpc;

/// <summary>
/// 适用于XmlRpc的标记
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[DynamicMethod]
public sealed class XmlRpcAttribute : RpcAttribute
{
    /// <inheritdoc/>
    public override Type[] GetGenericConstraintTypes()
    {
        return new Type[] { typeof(IXmlRpcClient) };
    }

    /// <inheritdoc/>
    protected override PropertyInfo[] GetPublicProperties()
    {
        return typeof(XmlRpcAttribute).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }
}