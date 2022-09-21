using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core.Extensions;

namespace TouchSocket.Core.Collections
{
    /// <summary>
    /// NameValueCollectionDebugView
    /// </summary>
    public class NameValueCollectionDebugView
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly NameValueCollection m_nameValue;

        /// <summary>
        /// NameValueCollectionDebugView
        /// </summary>
        /// <param name="nameValue"></param>
        public NameValueCollectionDebugView(NameValueCollection nameValue)
        {
            this.m_nameValue = nameValue;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private Dictionary<string, string> KV
        {
            get
            {
                var dic = new Dictionary<string, string>();
                foreach (var item in m_nameValue.AllKeys)
                {
                    dic.TryAdd(item, m_nameValue[item]);
                }
                return dic;
            }
        }
    }
}
