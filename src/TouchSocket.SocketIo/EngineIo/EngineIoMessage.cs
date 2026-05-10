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
    private byte[] m_rawData;
    private string m_text;

    public EngineIoMessage(EngineIoMessageType messageType, bool isText, byte[] rawData)
    {
        this.MessageType = messageType;
        this.IsText = isText;
        this.m_rawData = rawData;
        this.m_text = string.Empty;
    }

    public EngineIoMessage(EngineIoMessageType messageType, byte[] rawData)
    {
        this.MessageType = messageType;
        this.IsText = false;
        this.m_rawData = rawData;
        this.m_text = string.Empty;
    }

    public EngineIoMessage(EngineIoMessageType messageType, string text)
    {
        this.MessageType = messageType;
        this.IsText = true;
        this.m_text = text;
        this.m_rawData = new byte[0];
    }

    public EngineIoMessage(EngineIoMessageType messageType)
    {
        this.MessageType = messageType;
        this.IsText = true;
        this.m_text = string.Empty;
        this.m_rawData = new byte[0];
    }

    public bool IsText { get; private set; }
    public EngineIoMessageType MessageType { get; private set; }

    public byte[] GetRawData()
    {
        if (this.m_rawData != null)
        {
            return this.m_rawData;
        }
        if (this.m_text == null)
        {
            return default;
        }
        this.m_rawData = Encoding.UTF8.GetBytes(this.m_text);
        return this.m_rawData;
    }

    public string GetText()
    {
        if (this.m_text != null)
        {
            return this.m_text;
        }

        if (this.m_rawData == null)
        {
            return default;
        }

        this.m_text = this.IsText ? Encoding.UTF8.GetString(this.m_rawData) : BitConverter.ToString(this.m_rawData);
        return this.m_text;
    }

    public override string ToString()
    {
        var Builder = new StringBuilder();
        Builder.Append(string.Format("Type={0}", this.MessageType));

        if (this.IsText)
        {
            Builder.Append(string.Format(", Data={0}", this.GetText()));
        }
        else
        {
            Builder.Append(string.Format(", RawData={0}", BitConverter.ToString(this.GetRawData())));
        }

        return Builder.ToString();
    }

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

        return new EngineIoMessage(EngineIoMessageType.Open, stringBuilder.ToString());
    }

    public static EngineIoMessage CreateOpenMessage(string sid, int pingInterval, int pingTimeout, int maxPayload, bool upgrade)
    {
        return CreateOpenMessage(sid, pingInterval, pingTimeout, maxPayload, upgrade ? "websocket" : default);
    }

    #endregion Create
}