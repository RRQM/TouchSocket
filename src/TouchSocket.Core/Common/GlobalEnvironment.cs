namespace TouchSocket.Core
{
    /// <summary>
    /// 全局环境设置
    /// </summary>
    public static class GlobalEnvironment
    {
        /// <summary>
        /// 动态构建类型，默认使用IL
        /// </summary>
        public static DynamicBuilderType DynamicBuilderType { get; set; } = DynamicBuilderType.IL;
    }
}