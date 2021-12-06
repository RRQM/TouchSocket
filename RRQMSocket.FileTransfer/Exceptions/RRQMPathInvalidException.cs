using RRQMCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.FileTransfer
{
    /// <summary>
    /// 路径不合法
    /// </summary>
    [System.Serializable]
    public class RRQMPathInvalidException : RRQMException
    {
        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        public RRQMPathInvalidException() : base() { }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="message"></param>
        public RRQMPathInvalidException(string message) : base(message) { }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RRQMPathInvalidException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RRQMPathInvalidException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
