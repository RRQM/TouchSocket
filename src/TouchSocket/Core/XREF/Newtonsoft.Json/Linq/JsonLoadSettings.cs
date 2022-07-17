//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when loading JSON.
    /// </summary>
    public class JsonLoadSettings
    {
        private CommentHandling _commentHandling;
        private LineInfoHandling _lineInfoHandling;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonLoadSettings"/> class.
        /// </summary>
        public JsonLoadSettings()
        {
            this._lineInfoHandling = LineInfoHandling.Load;
            this._commentHandling = CommentHandling.Ignore;
        }

        /// <summary>
        /// Gets or sets how JSON comments are handled when loading JSON.
        /// </summary>
        /// <value>The JSON comment handling.</value>
        public CommentHandling CommentHandling
        {
            get => this._commentHandling;
            set
            {
                if (value < CommentHandling.Ignore || value > CommentHandling.Load)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._commentHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how JSON line info is handled when loading JSON.
        /// </summary>
        /// <value>The JSON line info handling.</value>
        public LineInfoHandling LineInfoHandling
        {
            get => this._lineInfoHandling;
            set
            {
                if (value < LineInfoHandling.Ignore || value > LineInfoHandling.Load)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._lineInfoHandling = value;
            }
        }
    }
}