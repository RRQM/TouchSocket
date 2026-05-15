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

namespace TouchSocket.SocketIo;

public struct EngineIoMessage
{
    public EngineIoMessage(EngineIoMessageType messageType, bool isText, ReadOnlyMemory<byte> rawData)
    {
        this.MessageType = messageType;
        this.IsText = isText;
        this.RawData = rawData.ToArray();
    }

    public bool IsText { get; }

    public EngineIoMessageType MessageType { get; }

    public ReadOnlyMemory<byte> RawData { get; }

    #region Create

    public static EngineIoMessage CreateOpenMessage(string sid, int pingInterval, int pingTimeout, int maxPayload, params string[] upgrades)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append('{');
        stringBuilder.Append($"\"sid\": \"{sid}\",");
        stringBuilder.Append("\"upgrades\":");
        stringBuilder.Append('[');
        if (upgrades != null)
        {
            foreach (var item in upgrades)
            {
                if (item.HasValue())
                {
                    stringBuilder.Append($"\"{item}\"");
                }
            }
        }
        stringBuilder.Append("],");
        stringBuilder.Append($"\"pingInterval\": {pingInterval},");
        stringBuilder.Append($"\"pingTimeout\": {pingTimeout},");
        stringBuilder.Append($"\"maxPayload\": {maxPayload}");
        stringBuilder.Append('}');

        return new EngineIoMessage(EngineIoMessageType.Open,true, stringBuilder.ToString().ToUtf8Bytes());
    }

    public static EngineIoMessage CreateOpenMessage(string sid, int pingInterval, int pingTimeout, int maxPayload, bool upgrade)
    {
        return CreateOpenMessage(sid, pingInterval, pingTimeout, maxPayload, upgrade ? "websocket" : default);
    }

    #endregion Create
}