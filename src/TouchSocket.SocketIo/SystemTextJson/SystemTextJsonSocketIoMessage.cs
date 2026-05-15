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

using System.Text.Json.Nodes;

namespace TouchSocket.SocketIo;

internal class SystemTextJsonSocketIoMessage : ISocketIoMessage
{
    private int[] m_bytesIndices;
    private string m_event;
    private JsonArray m_jsonArray;
    private bool m_parsed;

    public SystemTextJsonSocketIoMessage(SocketIoMessageType type)
    {
        this.MessageType = type;
    }

    public int ArgsCount
    {
        get
        {
            this.Parse();
            return this.m_jsonArray?.Count ?? 0;
        }
    }

    public List<ReadOnlyMemory<byte>> Bytes { get; set; }

    public int BytesCount { get; set; }

    public int[] BytesIndices
    {
        get
        {
            this.Parse();
            return this.m_bytesIndices;
        }
    }

    public string Error { get; set; }

    public string Event
    {
        get
        {
            this.Parse();
            return this.m_event;
        }
        set => this.m_event = value;
    }

    public int? Id { get; set; }

    public JsonArray JsonArray
    {
        get
        {
            this.Parse();
            return this.m_jsonArray;
        }
    }

    public SocketIoMessageType MessageType { get; }

    public string Namespace { get; set; }

    public int Sign { get => (int)this.Id; set => this.Id = value; }

    public string Text { get; set; }

    public bool TryGetBytes(int index, out ReadOnlyMemory<byte> bytes)
    {
        var indices = this.BytesIndices;
        for (var i = 0; i < indices.Length; i++)
        {
            if (indices[i] == index)
            {
                bytes = this.Bytes[i];
                return true;
            }
        }

        bytes = default;
        return false;
    }

    private void Parse()
    {
        if (this.m_parsed)
        {
            return;
        }

        if (this.Text.HasValue())
        {
            var jsonArray = JsonNode.Parse(this.Text)?.AsArray();
            this.SetEvent(jsonArray);
            this.m_jsonArray = jsonArray;

            var bytesIndices = new List<int>();
            if (jsonArray != null)
            {
                for (var i = 0; i < jsonArray.Count; i++)
                {
                    var item = jsonArray[i];
                    if (item is JsonObject obj && obj["_placeholder"] != null)
                    {
                        bytesIndices.Add(i);
                    }
                }
            }

            this.m_bytesIndices = bytesIndices.ToArray();
        }
        else
        {
            this.m_bytesIndices = new int[0];
            this.m_jsonArray = new JsonArray();
        }

        this.m_parsed = true;
    }

    private void SetEvent(JsonArray jsonArray)
    {
        if (this.MessageType != SocketIoMessageType.Event && this.MessageType != SocketIoMessageType.Binary)
        {
            return;
        }

        if (jsonArray == null || jsonArray.Count < 1)
        {
            throw new ArgumentException("未找到事件名称");
        }

        if (jsonArray[0] is null)
        {
            throw new ArgumentException("事件名称为 null");
        }

        this.m_event = jsonArray[0].GetValue<string>();
        jsonArray.RemoveAt(0);
    }
}
