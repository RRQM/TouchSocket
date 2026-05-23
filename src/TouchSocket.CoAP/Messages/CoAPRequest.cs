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

namespace TouchSocket.CoAP;

/// <summary>
/// CoAP 请求消息。
/// </summary>
public sealed class CoAPRequest : CoAPMessage
{
    /// <summary>
    /// 获取或设置请求方法。
    /// </summary>
    public CoAPMethod Method
    {
        get => (CoAPMethod)this.Code;
        set => this.Code = (byte)value;
    }

    /// <summary>
    /// 获取 URI 路径（将所有 Uri-Path 选项以 "/" 拼接）。
    /// </summary>
    public string UriPath
    {
        get
        {
            var parts = new System.Collections.Generic.List<string>();
            foreach (var opt in this.Options.GetOptions(CoAPOptionNumber.UriPath))
            {
                parts.Add(opt.GetStringValue());
            }

            return "/" + string.Join("/", parts);
        }
    }

    /// <summary>
    /// 获取 URI 查询字符串（将所有 Uri-Query 选项以 "&amp;" 拼接）。
    /// </summary>
    public string UriQuery
    {
        get
        {
            var parts = new System.Collections.Generic.List<string>();
            foreach (var opt in this.Options.GetOptions(CoAPOptionNumber.UriQuery))
            {
                parts.Add(opt.GetStringValue());
            }

            return string.Join("&", parts);
        }
    }
}
