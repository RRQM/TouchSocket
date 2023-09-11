using TouchSocket.Core;

namespace TouchSocket.WebApi.Swagger
{
    /// <summary>
    /// SwaggerPluginsManagerExtension
    /// </summary>
    public static class SwaggerPluginsManagerExtension
    {
        /// <summary>
        /// 使用<see cref="SwaggerPlugin"/>插件。
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static SwaggerPlugin UseSwagger(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<SwaggerPlugin>();
        }
    }
}
