using System;

namespace RRQMCore.Serialization
{
    /// <summary>
    /// 忽略的RRQN序列化
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RRQMNonSerializedAttribute : Attribute
    {
    }
}
