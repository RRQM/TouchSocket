using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    /// <summary>
    /// 定义一个多文件集合接口，继承自IEnumerable{IFormFile}。
    /// 此接口用于统一处理多个文件的集合，提供了遍历集合中每个文件的能力。
    /// </summary>
    public interface IMultifileCollection : IEnumerable<IFormFile>
    {

    }
}
