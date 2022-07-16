using System.Security.Cryptography;
using System.Text;

namespace TouchSocket.Core.Data
{
    /// <summary>
    /// MD5相关操作类
    /// </summary>
    public static class MD5
    {
        /// <summary>
        /// 从字符串获取MD5值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetMD5Hash(string str)
        {
            StringBuilder sb = new StringBuilder();
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                int length = data.Length;
                for (int i = 0; i < length; i++)
                    sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 从字节获取MD5值
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetMD5Hash(byte[] buffer, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] data = md5.ComputeHash(buffer, offset, length);
                int count = data.Length;
                for (int i = 0; i < count; i++)
                    sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 从字节获取MD5值
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string GetMD5Hash(byte[] buffer)
        {
            return GetMD5Hash(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 验证MD5值。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static bool VerifyMD5Hash(string str, string hash)
        {
            string hashOfInput = GetMD5Hash(str);
            if (hashOfInput.CompareTo(hash) == 0)
                return true;
            else
                return false;
        }
    }
}