using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using MistCore.Framework.Config;

namespace MistCore.Framework.Text
{
    /// <summary>
    /// 文本加密
    /// </summary>
    public class TextEncryptor
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="aseKey">16byte</param>
        /// <param name="aseIV">16byte</param>
        /// <param name="desKey">8byte</param>
        /// <param name="desIV">8byte</param>
        public TextEncryptor(byte[] aseKey = null, byte[] aseIV = null, byte[] desKey = null, byte[] desIV = null)
        {
            this.keyBtyesASE = aseKey;
            this.keyIVASE = aseIV;
            this.keyBtyesDES = desKey;
            this.keyIVDES = desIV;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encryptorInfo"></param>
        internal TextEncryptor(EncryptorInfo encryptorInfo)
        {
            if (encryptorInfo != null)
            {
                this.keyBtyesASE = Convert.FromBase64String(encryptorInfo.ASEKey);
                this.keyIVASE = Convert.FromBase64String(encryptorInfo.ASEIV);
                this.keyBtyesDES = Convert.FromBase64String(encryptorInfo.DESKey);
                this.keyIVDES = Convert.FromBase64String(encryptorInfo.DESIV);
            }
        }

        #region MD5 Encrypt

        /// <summary>
        /// 计算文本的MD5哈希值
        /// </summary>
        /// <param name="bytes">要进行哈希计算</param>
        /// <returns></returns>
        public string Hash(byte[] bytes)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            var hash = sb.ToString();
            return hash;
        }

        /// <summary>
        /// 计算文本的MD5哈希值
        /// </summary>
        /// <param name="text">要进行哈希计算的字符串</param>
        /// <returns></returns>
        public string Hash(string text)
        {
            return Hash(Encoding.UTF8.GetBytes(text));
        }
        #endregion

        #region SHA1 Encrypt

        /// <summary>
        /// 计算文本的SHA1
        /// </summary>
        /// <param name="text">要校验的文本</param>
        /// <returns></returns> 
        public string SHA1Encrypt(string text)
        {
            System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] bytResult = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
            sha1.Clear();
            string strResult = BitConverter.ToString(bytResult);
            strResult = strResult.Replace("-", "");
            return strResult;
            /* 另一种方法
            pass = FormsAuthentication.HashPasswordForStoringInConfigFile(pass, "SHA1");
            return pass;
            */
        }

        /// <summary>
        /// 计算流的SHA1
        /// </summary>
        /// <param name="stream">要校验的流</param>
        /// <returns></returns>
        public string SHA1HashFromFile(Stream stream)
        {
            System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] retVal = sha1.ComputeHash(stream);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        #endregion

        #region ASE Encrypt
        /// <summary>
        /// 算法的密钥，长度为16
        /// </summary>
        private byte[] keyBtyesASE = new byte[16];

        /// <summary>
        /// 算法的初始化向量，长度16
        /// </summary>
        private byte[] keyIVASE = new byte[16];

        /// <summary>
        /// AES加密字符串
        /// </summary>
        /// <param name="plainText">待加密的字符串</param>
        /// <returns></returns>
        public string ASEBase64Encrypt(string plainText)
        {
            byte[] encrypted = null;
            using (Rijndael rijAlg = Rijndael.Create())
            {
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(this.keyBtyesASE, this.keyIVASE);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// AES解密字符串
        /// </summary>
        /// <param name="cipherText">待解密的字符串</param>
        /// <returns></returns> 
        public string ASEDBase64ecrypt(string cipherText)
        {
            byte[] cipherTexts = Convert.FromBase64String(cipherText);
            string plaintext = null;
            using (Rijndael rijAlg = Rijndael.Create())
            {
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(this.keyBtyesASE, this.keyIVASE);
                using (MemoryStream msDecrypt = new MemoryStream(cipherTexts))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }

        #endregion

        #region DES Encrypt
        /// <summary>
        /// 算法的密钥，长度为8的倍数，最大长度64
        /// </summary>
        private byte[] keyBtyesDES = new byte[8];

        /// <summary>
        /// 算法的初始化向量，长度为8的倍数，最大长度64
        /// </summary>
        private byte[] keyIVDES = new byte[8];

        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param>
        /// <returns></returns> 
        public string DESEncrypt(string encryptString)
        {
            byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
            System.Security.Cryptography.DESCryptoServiceProvider dCSP = new System.Security.Cryptography.DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            System.Security.Cryptography.CryptoStream cStream = new System.Security.Cryptography.CryptoStream(mStream, dCSP.CreateEncryptor(this.keyBtyesDES, this.keyIVDES), System.Security.Cryptography.CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            cStream.Close();
            return Convert.ToBase64String(mStream.ToArray());
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <returns></returns> 
        public string DESDecrypt(string decryptString)
        {
            byte[] inputByteArray = Convert.FromBase64String(decryptString);
            System.Security.Cryptography.DESCryptoServiceProvider DCSP = new System.Security.Cryptography.DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            System.Security.Cryptography.CryptoStream cStream = new System.Security.Cryptography.CryptoStream(mStream, DCSP.CreateDecryptor(this.keyBtyesDES, this.keyIVDES), System.Security.Cryptography.CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            cStream.Close();
            return Encoding.UTF8.GetString(mStream.ToArray());
        }
        #endregion

        #region HEX Encrypt

        /// <summary> 
        /// 从汉字转换到16进制 
        /// </summary> 
        /// <param name="s"></param> 
        /// <param name="charset">编码,如"utf-8","gb2312"</param> 
        /// <param name="fenge">是否每字符用逗号分隔</param> 
        /// <returns></returns> 
        public string ToHex(string s, Encoding encoding, bool fenge)
        {
            if ((s.Length % 2) != 0)
            {
                s += " ";//空格 
                //throw new ArgumentException("s is not valid chinese string!"); 
            }
            System.Text.Encoding chs = encoding;
            byte[] bytes = chs.GetBytes(s);
            string str = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str += string.Format("{0:X}", bytes[i]);
                if (fenge && (i != bytes.Length - 1))
                {
                    str += string.Format("{0}", ",");
                }
            }
            return str.ToLower();
        }

        ///<summary> 
        /// 从16进制转换成汉字 
        /// </summary> 
        /// <param name="hex"></param> 
        /// <param name="charset">编码,如"utf-8","gb2312"</param> 
        /// <returns></returns> 
        public string UnHex(string hex, Encoding encoding)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            hex = hex.Replace(",", "");
            hex = hex.Replace("\n", "");
            hex = hex.Replace("\\", "");
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0)
            {
                hex += "20";//空格 
            }
            // 需要将 hex 转换成 byte 数组。 
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                try
                {
                    // 每两个字符是一个 byte。 
                    bytes[i] = byte.Parse(hex.Substring(i * 2, 2),
                    System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    // Rethrow an exception with custom message. 
                    throw new ArgumentException("hex is not a valid hex number!", "hex");
                }
            }
            System.Text.Encoding chs = encoding;
            return chs.GetString(bytes);
        }

        /// <summary>
        /// Char To Byte[]
        /// </summary>
        /// <param name="cChar">char</param>
        /// <returns>byte[] length = 2</returns>
        public static byte[] CharToBytes(char cChar)
        {
            byte[] b = new byte[2];
            b[0] = (byte)((cChar & 0xFF00) >> 8);
            b[1] = (byte)(cChar & 0xFF);
            return b;
        }

        /// <summary>
        /// Byte[] To Char
        /// </summary>
        /// <param name="bytes">byte[] length = 2</param>
        /// <returns>char</returns>
        public static char BytesToChar(byte[] bBytes)
        {
            char c = (char)(((bBytes[0] & 0xFF) << 8) | (bBytes[1] & 0xFF));
            return c;
        }
        #endregion

    }
}
