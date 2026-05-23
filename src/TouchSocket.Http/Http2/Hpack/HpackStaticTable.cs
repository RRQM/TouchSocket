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

namespace TouchSocket.Http;

/// <summary>
/// HPACK 静态表，共 61 条，见 RFC 7541 Appendix A
/// </summary>
internal static class HpackStaticTable
{
    /// <summary>静态表条目数</summary>
    public const int Count = 61;

    /// <summary>
    /// 静态表条目，索引 1..61
    /// </summary>
    private static readonly Http2Header[] s_entries = new Http2Header[]
    {
        // index 1
        new Http2Header(":authority",                    ""),
        // index 2
        new Http2Header(":method",                       "GET"),
        // index 3
        new Http2Header(":method",                       "POST"),
        // index 4
        new Http2Header(":path",                         "/"),
        // index 5
        new Http2Header(":path",                         "/index.html"),
        // index 6
        new Http2Header(":scheme",                       "http"),
        // index 7
        new Http2Header(":scheme",                       "https"),
        // index 8
        new Http2Header(":status",                       "200"),
        // index 9
        new Http2Header(":status",                       "204"),
        // index 10
        new Http2Header(":status",                       "206"),
        // index 11
        new Http2Header(":status",                       "304"),
        // index 12
        new Http2Header(":status",                       "400"),
        // index 13
        new Http2Header(":status",                       "404"),
        // index 14
        new Http2Header(":status",                       "500"),
        // index 15
        new Http2Header("accept-charset",                ""),
        // index 16
        new Http2Header("accept-encoding",               "gzip, deflate"),
        // index 17
        new Http2Header("accept-language",               ""),
        // index 18
        new Http2Header("accept-ranges",                 ""),
        // index 19
        new Http2Header("accept",                        ""),
        // index 20
        new Http2Header("access-control-allow-origin",   ""),
        // index 21
        new Http2Header("age",                           ""),
        // index 22
        new Http2Header("allow",                         ""),
        // index 23
        new Http2Header("authorization",                 ""),
        // index 24
        new Http2Header("cache-control",                 ""),
        // index 25
        new Http2Header("content-disposition",           ""),
        // index 26
        new Http2Header("content-encoding",              ""),
        // index 27
        new Http2Header("content-language",              ""),
        // index 28
        new Http2Header("content-length",                ""),
        // index 29
        new Http2Header("content-location",              ""),
        // index 30
        new Http2Header("content-range",                 ""),
        // index 31
        new Http2Header("content-type",                  ""),
        // index 32
        new Http2Header("cookie",                        ""),
        // index 33
        new Http2Header("date",                          ""),
        // index 34
        new Http2Header("etag",                          ""),
        // index 35
        new Http2Header("expect",                        ""),
        // index 36
        new Http2Header("expires",                       ""),
        // index 37
        new Http2Header("from",                          ""),
        // index 38
        new Http2Header("host",                          ""),
        // index 39
        new Http2Header("if-match",                      ""),
        // index 40
        new Http2Header("if-modified-since",             ""),
        // index 41
        new Http2Header("if-none-match",                 ""),
        // index 42
        new Http2Header("if-range",                      ""),
        // index 43
        new Http2Header("if-unmodified-since",           ""),
        // index 44
        new Http2Header("last-modified",                 ""),
        // index 45
        new Http2Header("link",                          ""),
        // index 46
        new Http2Header("location",                      ""),
        // index 47
        new Http2Header("max-forwards",                  ""),
        // index 48
        new Http2Header("proxy-authenticate",            ""),
        // index 49
        new Http2Header("proxy-authorization",           ""),
        // index 50
        new Http2Header("range",                         ""),
        // index 51
        new Http2Header("referer",                       ""),
        // index 52
        new Http2Header("refresh",                       ""),
        // index 53
        new Http2Header("retry-after",                   ""),
        // index 54
        new Http2Header("server",                        ""),
        // index 55
        new Http2Header("set-cookie",                    ""),
        // index 56
        new Http2Header("strict-transport-security",     ""),
        // index 57
        new Http2Header("transfer-encoding",             ""),
        // index 58
        new Http2Header("user-agent",                    ""),
        // index 59
        new Http2Header("vary",                          ""),
        // index 60
        new Http2Header("via",                           ""),
        // index 61
        new Http2Header("www-authenticate",              ""),
    };

    /// <summary>
    /// 按索引（1-based）获取静态表条目
    /// </summary>
    public static Http2Header Get(int index)
    {
        if (index < 1 || index > Count)
        {
            throw new IndexOutOfRangeException($"HPACK 静态表索引 {index} 超出范围 [1, {Count}]");
        }
        return s_entries[index - 1];
    }

    /// <summary>
    /// 查找与给定名称和值完全匹配的静态表索引（1-based），未找到返回 0
    /// </summary>
    public static int FindIndex(string name, string value)
    {
        for (var i = 0; i < s_entries.Length; i++)
        {
            var e = s_entries[i];
            if (string.Equals(e.Name, name, StringComparison.Ordinal)
                && string.Equals(e.Value, value, StringComparison.Ordinal))
            {
                return i + 1;
            }
        }
        return 0;
    }

    /// <summary>
    /// 查找与给定名称匹配的第一个静态表索引（1-based），未找到返回 0
    /// </summary>
    public static int FindNameIndex(string name)
    {
        for (var i = 0; i < s_entries.Length; i++)
        {
            if (string.Equals(s_entries[i].Name, name, StringComparison.Ordinal))
            {
                return i + 1;
            }
        }
        return 0;
    }
}
