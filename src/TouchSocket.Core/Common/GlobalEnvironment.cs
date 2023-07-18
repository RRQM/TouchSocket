namespace TouchSocket.Core
{
    /// <summary>
    /// 全局环境设置
    /// </summary>
    public static class GlobalEnvironment
    {
        /// <summary>
        /// 优化平台
        /// </summary>
        public static OptimizedPlatforms OptimizedPlatforms { get; set; } = OptimizedPlatforms.None;
    }
}
