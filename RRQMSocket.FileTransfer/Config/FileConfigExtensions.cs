using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 文件配置扩展
    /// </summary>
    public static class FileConfigExtensions
    {
        /// <summary>
        /// 根目录
        /// 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty RootPathProperty =
            DependencyProperty.Register("RootPath", typeof(string), typeof(FileConfigExtensions), string.Empty);

        /// <summary>
        /// 允许的响应类型,
        ///  所需类型<see cref="RRQMSocket.FileTransfer.ResponseType"/>
        /// </summary>
        public static readonly DependencyProperty ResponseTypeProperty =
            DependencyProperty.Register("ResponseType", typeof(ResponseType), typeof(FileConfigExtensions), ResponseType.Both);


        /// <summary>
        /// 设置根路径
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetRootPath(this RRQMConfig config, string value)
        {
            config.SetValue(RootPathProperty, value);
            return config;
        }

        /// <summary>
        /// 设置允许的响应类型
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetResponseType(this RRQMConfig config, ResponseType value)
        {
            config.SetValue(RootPathProperty, value);
            return config;
        }
    }
}
