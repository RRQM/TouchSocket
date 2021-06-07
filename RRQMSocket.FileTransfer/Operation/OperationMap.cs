using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RRQMCore.Exceptions;

namespace RRQMSocket.FileTransfer
{
    internal class OperationMap
    {
        Dictionary<string, OperationEntity> map = new Dictionary<string, OperationEntity>();

        internal void Add(string key, OperationEntity entity)
        {
            if (!map.ContainsKey(key))
            {
                map.Add(key, entity);
            }
            else
            {
                throw new RRQMException($"操作名为{key}的已经注册");
            }
        }

        internal bool TryGet(string key, out OperationEntity entity)
        {
            return map.TryGetValue(key, out entity);
        }
    }
}
