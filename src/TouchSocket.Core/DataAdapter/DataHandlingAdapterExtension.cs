//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// DateHandleAdapterExtension
    /// </summary>
    public static class DataHandlingAdapterExtension
    {
        #region SingleStreamDataHandlingAdapter

        /// <summary>
        /// 将<see cref="TouchSocketConfig"/>中的配置，装载在<see cref="SingleStreamDataHandlingAdapter"/>上。
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="config"></param>
        public static void Config(this SingleStreamDataHandlingAdapter adapter, TouchSocketConfig config)
        {
            var option = config.GetValue(AdapterOptionProperty) ?? throw new ArgumentNullException(nameof(AdapterOptionProperty));

            if (option.MaxPackageSize.HasValue)
            {
                adapter.MaxPackageSize = option.MaxPackageSize.Value;
            }

            if (option.CacheTimeout.HasValue)
            {
                adapter.CacheTimeout = option.CacheTimeout.Value;
            }

            if (option.CacheTimeoutEnable.HasValue)
            {
                adapter.CacheTimeoutEnable = option.CacheTimeoutEnable.Value;
            }

            if (option.UpdateCacheTimeWhenRev.HasValue)
            {
                adapter.UpdateCacheTimeWhenRev = option.UpdateCacheTimeWhenRev.Value;
            }
        }

        /// <summary>
        /// 将<see cref="TouchSocketConfig"/>中的配置，装载在<see cref="SingleStreamDataHandlingAdapter"/>上。
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="config"></param>
        public static void Config(this DataHandlingAdapter adapter, TouchSocketConfig config)
        {
            var option = config.GetValue(AdapterOptionProperty) ?? throw new ArgumentNullException(nameof(AdapterOptionProperty));

            if (option.MaxPackageSize.HasValue)
            {
                adapter.MaxPackageSize = option.MaxPackageSize.Value;
            }
        }

        #endregion SingleStreamDataHandlingAdapter

        #region 适配器配置

        /// <summary>
        /// 适配器数据包缓存启用。默认为缺省（null），如果有正常值会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.CacheTimeout"/>
        /// </summary>
        [Obsolete("本配置已被弃用，适配器相关设定，请使用SetAdapterOption", true)]
        public static readonly DependencyProperty<bool?> CacheTimeoutEnableProperty = DependencyProperty<bool?>.Register("CacheTimeoutEnable", null);

        /// <summary>
        /// 适配器数据包缓存时长。默认为缺省（<see cref="TimeSpan.Zero"/>）。当该值有效时会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.CacheTimeout"/>
        /// </summary>
        [Obsolete("本配置已被弃用，适配器相关设定，请使用SetAdapterOption", true)]
        public static readonly DependencyProperty<TimeSpan> CacheTimeoutProperty = DependencyProperty<TimeSpan>.Register("CacheTimeout", TimeSpan.Zero);

        /// <summary>
        /// 适配器数据包最大值。默认缺省（null），当该值有效时会在设置适配器时，直接作用于<see cref="DataHandlingAdapter.MaxPackageSize"/>
        /// </summary>
        [Obsolete("本配置已被弃用，适配器相关设定，请使用SetAdapterOption", true)]
        public static readonly DependencyProperty<int?> MaxPackageSizeProperty = DependencyProperty<int?>.Register("MaxPackageSize", null);

        /// <summary>
        /// 适配器数据包缓存策略。默认缺省（null），当该值有效时会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.UpdateCacheTimeWhenRev"/>
        /// </summary>
        [Obsolete("本配置已被弃用，适配器相关设定，请使用SetAdapterOption", true)]
        public static readonly DependencyProperty<bool?> UpdateCacheTimeWhenRevProperty = DependencyProperty<bool?>.Register("UpdateCacheTimeWhenRev", null);

        /// <summary>
        /// 适配器数据包缓存时长。默认为缺省（<see cref="TimeSpan.Zero"/>）。当该值有效时会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.CacheTimeout"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("本配置已被弃用，适配器相关设定，请使用SetAdapterOption", true)]
        public static TouchSocketConfig SetCacheTimeout(this TouchSocketConfig config, TimeSpan value)
        {
            config.SetValue(CacheTimeoutProperty, value);
            return config;
        }

        /// <summary>
        /// 适配器数据包缓存启用。默认为缺省（null），如果有正常值会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.CacheTimeoutEnable"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("本配置已被弃用，适配器相关设定，请使用SetAdapterOption", true)]
        public static TouchSocketConfig SetCacheTimeoutEnable(this TouchSocketConfig config, bool value)
        {
            config.SetValue(CacheTimeoutEnableProperty, value);
            return config;
        }

        /// <summary>
        /// 适配器数据包最大值。默认缺省（null），当该值有效时会在设置适配器时，直接作用于<see cref="DataHandlingAdapter.MaxPackageSize"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("本配置已被弃用，适配器相关设定，请使用SetAdapterOption", true)]
        public static TouchSocketConfig SetMaxPackageSize(this TouchSocketConfig config, int value)
        {
            config.SetValue(MaxPackageSizeProperty, value);
            return config;
        }

        /// <summary>
        /// 适配器数据包缓存策略。默认缺省（null），当该值有效时会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.UpdateCacheTimeWhenRev"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("本配置已被弃用，适配器相关设定，请使用SetAdapterOption", true)]
        public static TouchSocketConfig SetUpdateCacheTimeWhenRev(this TouchSocketConfig config, bool value)
        {
            config.SetValue(UpdateCacheTimeWhenRevProperty, value);
            return config;
        }

        #endregion 适配器配置

        /// <summary>
        /// 设置适配器相关的配置
        /// </summary>
        public static readonly DependencyProperty<AdapterOption> AdapterOptionProperty = DependencyProperty<AdapterOption>.Register("AdapterOption", new AdapterOption());

        /// <summary>
        /// 设置适配器相关的配置
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetAdapterOption(this TouchSocketConfig config, AdapterOption value)
        {
            config.SetValue(AdapterOptionProperty, value);
            return config;
        }

        #region MyRegion
        /// <summary>
        /// 将对象构建到字节数组
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <returns></returns>
        public static byte[] BuildAsBytes(this IRequestInfoBuilder requestInfo)
        {
            using (var byteBlock=new ByteBlock(requestInfo.MaxLength))
            {
                requestInfo.Build(byteBlock);
                return byteBlock.ToArray();
            }
        }
        #endregion
    }
}