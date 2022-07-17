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
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TouchSocket.Core.Data.Security
{
    /// <summary>
    /// 数据锁,用于加密或解密
    /// </summary>
    public static class DataLock
    {
        /// <summary>
        /// 使用3DES加密
        /// </summary>
        /// <param name="data">待加密字节</param>
        /// <param name="encryptKey">加密口令（长度为8）</param>
        /// <returns></returns>
        public static byte[] EncryptDES(byte[] data, string encryptKey)
        {
            byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
            byte[] rgbIV = { 0x12, 0x34, 4, 0x78, 0x90, 255, 0xCD, 0xEF };
            DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
            using (MemoryStream mStream = new MemoryStream())
            {
                using (CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write))
                {
                    cStream.Write(data, 0, data.Length);
                    cStream.FlushFinalBlock();
                    return mStream.ToArray();
                }
            }
        }

        /// <summary>
        /// 使用3DES解密
        /// </summary>
        /// <param name="data">待解密字节</param>
        /// <param name="decryptionKey">解密口令（长度为8）</param>
        /// <returns></returns>
        public static byte[] DecryptDES(byte[] data, string decryptionKey)
        {
            byte[] rgbKey = Encoding.UTF8.GetBytes(decryptionKey);
            byte[] rgbIV = { 0x12, 0x34, 4, 0x78, 0x90, 255, 0xCD, 0xEF };
            DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();

            using (MemoryStream mStream = new MemoryStream())
            {
                using (CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write))
                {
                    cStream.Write(data, 0, data.Length);
                    cStream.FlushFinalBlock();
                    return mStream.ToArray();
                }
            }
        }
    }
}