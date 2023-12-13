namespace TouchSocket.ProtocolHouse;

public class TelegramListDto
{
    #region 实体属性

    /// <summary>
    /// 电文号
    /// </summary>
    public virtual string TelegramNo { get; set; }

    /// <summary>
    /// 电文名称
    /// </summary>
    public virtual string TelegramName { get; set; }

    /// <summary>
    /// 英文名称
    /// </summary>
    public virtual string TelegramTableName { get; set; }

    /// <summary>
    /// 发送/接收
    /// </summary>
    public virtual string CommunicationType { get; set; }

    /// <summary>
    /// 外部系统
    /// </summary>
    public virtual string ExternalSystem { get; set; }

    /// <summary>
    /// 电文类型
    /// </summary>
    public virtual string TelegramType { get; set; }

    /// <summary>
    /// 电文长度
    /// </summary>
    public virtual int TelegramLength { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string Remarks { get; set; }

    #endregion 实体属性
}