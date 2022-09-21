using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core.Collections
{
    /// <summary>
    /// IgnoreCaseNameValueCollection
    /// </summary>
    [DebuggerTypeProxy(typeof(NameValueCollectionDebugView))]
    public class IgnoreCaseNameValueCollection:NameValueCollection
    {
        /// <summary>
        /// IgnoreCaseNameValueCollection
        /// </summary>
        public IgnoreCaseNameValueCollection():base(StringComparer.OrdinalIgnoreCase)
        {

        }
    }
}
