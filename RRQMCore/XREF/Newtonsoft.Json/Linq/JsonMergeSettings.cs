//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;

namespace RRQMCore.XREF.Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when merging JSON.
    /// </summary>
    public class JsonMergeSettings
    {
        private MergeArrayHandling _mergeArrayHandling;
        private MergeNullValueHandling _mergeNullValueHandling;

        /// <summary>
        /// Gets or sets the method used when merging JSON arrays.
        /// </summary>
        /// <value>The method used when merging JSON arrays.</value>
        public MergeArrayHandling MergeArrayHandling
        {
            get => _mergeArrayHandling;
            set
            {
                if (value < MergeArrayHandling.Concat || value > MergeArrayHandling.Merge)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _mergeArrayHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how null value properties are merged.
        /// </summary>
        /// <value>How null value properties are merged.</value>
        public MergeNullValueHandling MergeNullValueHandling
        {
            get => _mergeNullValueHandling;
            set
            {
                if (value < MergeNullValueHandling.Ignore || value > MergeNullValueHandling.Merge)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _mergeNullValueHandling = value;
            }
        }
    }
}