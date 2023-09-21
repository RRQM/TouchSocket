﻿using System;

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
            if (config.GetValue(DataHandlingAdapterExtension.MaxPackageSizeProperty) is int v1)
            {
                adapter.MaxPackageSize = v1;
            }
            if (config.GetValue(DataHandlingAdapterExtension.CacheTimeoutProperty) != TimeSpan.Zero)
            {
                adapter.CacheTimeout = config.GetValue(DataHandlingAdapterExtension.CacheTimeoutProperty);
            }
            if (config.GetValue(DataHandlingAdapterExtension.CacheTimeoutEnableProperty) is bool v2)
            {
                adapter.CacheTimeoutEnable = v2;
            }
            if (config.GetValue(DataHandlingAdapterExtension.UpdateCacheTimeWhenRevProperty) is bool v3)
            {
                adapter.UpdateCacheTimeWhenRev = v3;
            }
        }

        #endregion SingleStreamDataHandlingAdapter

        #region 适配器配置

        /// <summary>
        /// 适配器数据包缓存启用。默认为缺省（null），如果有正常值会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.CacheTimeout"/>
        /// </summary>
        public static readonly DependencyProperty<bool?> CacheTimeoutEnableProperty = DependencyProperty<bool?>.Register("CacheTimeoutEnable", null);

        /// <summary>
        /// 适配器数据包缓存时长。默认为缺省（<see cref="TimeSpan.Zero"/>）。当该值有效时会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.CacheTimeout"/>
        /// </summary>
        public static readonly DependencyProperty<TimeSpan> CacheTimeoutProperty = DependencyProperty<TimeSpan>.Register("CacheTimeout", TimeSpan.Zero);

        /// <summary>
        /// 适配器数据包最大值。默认缺省（null），当该值有效时会在设置适配器时，直接作用于<see cref="DataHandlingAdapter.MaxPackageSize"/>
        /// </summary>
        public static readonly DependencyProperty<int?> MaxPackageSizeProperty = DependencyProperty<int?>.Register("MaxPackageSize", null);

        /// <summary>
        /// 适配器数据包缓存策略。默认缺省（null），当该值有效时会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.UpdateCacheTimeWhenRev"/>
        /// </summary>
        public static readonly DependencyProperty<bool?> UpdateCacheTimeWhenRevProperty = DependencyProperty<bool?>.Register("UpdateCacheTimeWhenRev", null);

        /// <summary>
        /// 适配器数据包缓存时长。默认为缺省（<see cref="TimeSpan.Zero"/>）。当该值有效时会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.CacheTimeout"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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
        public static TouchSocketConfig SetUpdateCacheTimeWhenRev(this TouchSocketConfig config, bool value)
        {
            config.SetValue(UpdateCacheTimeWhenRevProperty, value);
            return config;
        }

        #endregion 适配器配置
    }
}