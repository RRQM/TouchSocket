using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 单线程流式适配器配置
    /// </summary>
    public class SingleStreamAdapterOption
    {
        /// <summary>
        /// 适配器数据包缓存启用。默认为缺省（null），如果有正常值会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.CacheTimeout"/>
        /// </summary>
        public bool? CacheTimeoutEnable { get; set; }=true;

        /// <summary>
        /// 适配器数据包缓存时长。默认为缺省（null）。当该值有效时会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.CacheTimeout"/>
        /// </summary>
        public int? CacheTimeout { get; set; }

        /// <summary>
        /// 适配器数据包最大值。默认缺省（null），当该值有效时会在设置适配器时，直接作用于<see cref="DataHandlingAdapter.MaxPackageSize"/>
        /// </summary>
        public int? MaxPackageSize { get; set; }

        /// <summary>
        ///  适配器数据包缓存策略。默认缺省（null），当该值有效时会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.UpdateCacheTimeWhenRev"/>
        /// </summary>
        public bool? UpdateCacheTimeWhenRev { get; set; }
    }
}
