using System;

namespace TouchSocket.Rpc.WebApi
{
    /// <summary>
    /// 表示WebApi路由。
    /// </summary>
    public class RouterAttribute : Attribute
    {
        /// <summary>
        /// 表示WebApi路由。
        /// 该模板在用于方法时，会覆盖类的使用。
        /// <para>模板必须由“/”开始，如果没有设置，会自动补齐。</para>
        /// <para>模板不支持参数约定，仅支持方法路由。</para>
        /// <para>模板有以下约定：
        /// <list type="number">
        /// <item>不区分大小写</item>
        /// <item>以“[Api]”表示当前类名，如果不包含此字段，则意味着会使用绝对设置</item>
        /// <item>以“[Action]”表示当前方法名，如果不包含此字段，则意味着会使用绝对设置</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="routeTemple"></param>
        public RouterAttribute(string routeTemple)
        {
            if (!routeTemple.StartsWith("/"))
            {
                routeTemple.Insert(0, "/");
            }
            this.RouteTemple = routeTemple;
        }

        /// <summary>
        /// 路由模板。
        /// </summary>
        public string RouteTemple { get; }
    }
}
