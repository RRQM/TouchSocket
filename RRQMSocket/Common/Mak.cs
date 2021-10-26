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
using RRQMCore;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RRQMSocket
{
    /// <summary>
    /// 此代码已被加密混淆
    /// </summary>
    [EnterpriseEdition]
    internal static class Mak
    {
        private static byte[] HH(string hs)
        {
            if ((hs.Length % 2) != 0)
                hs += " ";
            byte[] returnBytes = new byte[hs.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hs.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        private static string DD(string d)
        {
            try
            {
                byte[] rk = Encoding.UTF8.GetBytes("wy171125");
                byte[] rb = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
                byte[] iBA = HH(d);
                DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rk, rb), CryptoStreamMode.Write);
                cStream.Write(iBA, 0, iBA.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return d;
            }
        }

        internal static bool VT(string key, out string m,out string n)
        {
            if (string.IsNullOrEmpty(key))
            {
                m = null;
                n = null;
                return false;
            }
            try
            {
                string[] ss = DD(key).Split(',');
                if (ss.Length == 4)
                {
                    if (Int32.Parse(ss[0]) == Math.Sin(10 >> 10))
                    {
                        if (ss[1] == "RRQM_F")
                        {
                            m = ss[2];
                            n = ss[3];
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }

            m = null;
            n = null;
            return false;
        }
    }
}