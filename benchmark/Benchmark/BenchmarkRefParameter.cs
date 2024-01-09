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

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class BenchmarkRefParameter : BenchmarkBase
    {
        private string m_str = "123123123xssdfsdfsdfdsfdsfsdfl;l123123ol12312lk3m123m12312k312ko312oki312k312k312k31k23k13k12312k3k;123123l123l1212;3;123;12;'3;'123;12;312;3;123;12;312;312;312;312;312;3";
        private int m_intValue = int.MaxValue;
        private long m_longValue = long.MaxValue;

        [Benchmark]
        public void StringParameter()
        {
            for (var i = 0; i < this.Count; i++)
            {
                this.PrivateStringParameter(this.m_str);
            }
        }

        [Benchmark]
        public void RefStringParameter()
        {
            for (var i = 0; i < this.Count; i++)
            {
                this.PrivateStringInParameter(this.m_str);
            }
        }

        [Benchmark]
        public void IntParameter()
        {
            for (var i = 0; i < this.Count; i++)
            {
                this.PrivateIntParameter(this.m_intValue);
            }
        }

        [Benchmark]
        public void RefIntParameter()
        {
            for (var i = 0; i < this.Count; i++)
            {
                this.PrivateIntInParameter(this.m_intValue);
            }
        }

        [Benchmark]
        public void LongPameter()
        {
            for (var i = 0; i < this.Count; i++)
            {
                this.PrivateLongParameter(this.m_longValue);
            }
        }

        [Benchmark]
        public void RefLongParameter()
        {
            for (var i = 0; i < this.Count; i++)
            {
                this.PrivateLongInParameter(this.m_longValue);
            }
        }

        private void PrivateStringParameter(string value)
        {
        }

        private void PrivateStringInParameter(in string value)
        {
        }

        private void PrivateIntParameter(int value)
        {
        }

        private void PrivateIntInParameter(in int value)
        {
        }

        private void PrivateLongParameter(long value)
        {
        }

        private void PrivateLongInParameter(in long value)
        {
        }
    }
}