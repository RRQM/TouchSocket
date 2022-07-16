using TouchSocket.Core.ByteManager;

namespace TouchSocket.Sockets
{

    /// <summary>
    /// 过滤结果
    /// </summary>
    public enum FilterResult
    {
        /// <summary>
        /// 缓存后续所有<see cref="ByteBlock.CanReadLen"/>数据。
        /// </summary>
        Cache,

        /// <summary>
        /// 操作成功
        /// </summary>
        Success,

        /// <summary>
        /// 继续操作，一般原因是本次数据有部分无效，但已经调整了<see cref="ByteBlock.Pos"/>属性，所以继续后续解析。
        /// <para>或者想放弃当前数据的操作，直接设置<see cref="ByteBlock.Pos"/>与<see cref="ByteBlock.Len"/>相等即可。</para>
        /// </summary>
        GoOn
    }
}
