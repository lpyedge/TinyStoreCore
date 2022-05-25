using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace LPayments.Utils
{

    /// <summary>
    /// 不可逆加密辅助类
    /// </summary>
    public static class HASHCrypto
    {
        public enum CryptoEnum
        {
            MD5 = 32,
            SHA1 = 40,
            SHA256 = 64,
            SHA384 = 96,
            SHA512 = 128
        }

        /// <summary>
        /// 创建不可逆加密类
        /// </summary>
        /// <param name="cryptoEnum">加密类型</param>
        /// <param name="secretKey">密钥</param>
        /// <returns>返回加密类HashAlgorithm</returns>
        public static HashAlgorithm Generate(CryptoEnum cryptoEnum = CryptoEnum.SHA1, string secretKey = "")
        {
            byte[] secretBuff = null;
            if (!string.IsNullOrEmpty(secretKey))
            {
                secretBuff = Encoding.UTF8.GetBytes(secretKey);
            }
            return Generate2(cryptoEnum, secretBuff);
        }

        /// <summary>
        /// 创建不可逆加密类
        /// </summary>
        /// <param name="cryptoEnum">加密类型</param>
        /// <param name="secretBuff">密钥</param>
        /// <returns>返回加密类HashAlgorithm</returns>
        public static HashAlgorithm Generate2(CryptoEnum cryptoEnum = CryptoEnum.SHA1, byte[] secretBuff = null)
        {
            HashAlgorithm ha = null;
            if (secretBuff?.Length > 0)
            {
                switch (cryptoEnum)
                {
                    case CryptoEnum.MD5:
                        ha = new HMACMD5()
                        {
                            Key = secretBuff
                        };
                        break;

                    case CryptoEnum.SHA1:
                        ha = new HMACSHA1()
                        {
                            Key = secretBuff
                        };
                        break;

                    case CryptoEnum.SHA256:
                        ha = new HMACSHA256()
                        {
                            Key = secretBuff
                        };
                        break;

                    case CryptoEnum.SHA384:
                        ha = new HMACSHA384()
                        {
                            Key = secretBuff
                        };
                        break;

                    case CryptoEnum.SHA512:
                        ha = new HMACSHA512()
                        {
                            Key = secretBuff
                        };
                        break;
                }
            }
            else
            {
                switch (cryptoEnum)
                {
                    case CryptoEnum.MD5:
                        ha = MD5.Create(); // new MD5CryptoServiceProvider();
                        break;

                    case CryptoEnum.SHA1:
                        ha = SHA1.Create(); // new SHA1CryptoServiceProvider();
                        break;

                    case CryptoEnum.SHA256:
                        ha = SHA256.Create(); //new SHA256CryptoServiceProvider();
                        break;

                    case CryptoEnum.SHA384:
                        ha = SHA384.Create(); //new SHA384CryptoServiceProvider();
                        break;

                    case CryptoEnum.SHA512:
                        ha = SHA512.Create(); // new SHA512CryptoServiceProvider();
                        break;
                }
            }
            return ha;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="provider">加密类HashAlgorithm</param>
        /// <param name="inputText">待加密字符串</param>
        /// <returns>返回大写字符串</returns>
        public static string Encrypt(this HashAlgorithm provider, string inputText)
        {
            return Encrypt(provider, Encoding.UTF8.GetBytes(inputText));
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="provider">加密类HashAlgorithm</param>
        /// <param name="inputBuff">待加密字节数组</param>
        /// <returns>返回大写字符串</returns>
        public static string Encrypt(this HashAlgorithm provider, byte[] inputBuff)
        {
            var buff = Encrypt2Byte(provider, inputBuff);
            return BitConverter.ToString(buff).Replace("-", "");
        }

        /// <summary>
        ///加密
        /// </summary>
        /// <param name="provider">加密类HashAlgorithm</param>
        /// <param name="inputText">待加密字符串</param>
        /// <returns>返回字节数组</returns>
        public static byte[] Encrypt2Byte(this HashAlgorithm provider, string inputText)
        {
            return Encrypt2Byte(provider, Encoding.UTF8.GetBytes(inputText));
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="provider">加密类HashAlgorithm</param>
        /// <param name="inputBuff">待加密字节数组</param>
        /// <returns>返回字节数组</returns>
        public static byte[] Encrypt2Byte(this HashAlgorithm provider, byte[] inputBuff)
        {
            if (inputBuff != null)
            {
                return provider.ComputeHash(inputBuff);
            }
            return Array.Empty<byte>();
        }

        ///// <summary>
        ///// 加密
        ///// </summary>
        ///// <param name="p_InputText">待加密字符串</param>
        ///// <param name="p_CryptoEnum">加密类类型 DEncryptEnum</param>
        ///// <param name="p_SecretKey">加密盐值密钥</param>
        ///// <returns>返回大写字符串</returns>
        //public static string Encrypt(string p_InputText, CryptoEnum p_CryptoEnum, string p_SecretKey = "")
        //{
        //    return Generate(p_CryptoEnum, p_SecretKey).Encrypt(p_InputText);
        //}

        ///// <summary>
        /////加密
        ///// </summary>
        ///// <param name="p_InputText">待加密字符串</param>
        ///// <param name="p_CryptoEnum">加密类类型 DEncryptEnum</param>
        ///// <param name="p_SecretKey">加密盐值密钥</param>
        ///// <returns>返回字节数组</returns>
        //public static byte[] Encrypt2Byte(string p_InputText, CryptoEnum p_CryptoEnum, string p_SecretKey = "")
        //{
        //    return Generate(p_CryptoEnum, p_SecretKey).Encrypt2Byte(Encoding.UTF8.GetBytes(p_InputText));
        //}

        ///// <summary>
        ///// 加密
        ///// </summary>
        ///// <param name="p_InputBuff">待加密字节数组</param>
        ///// <param name="p_CryptoEnum">加密类类型 DEncryptEnum</param>
        ///// <param name="p_SecretKey">加密盐值密钥</param>
        ///// <returns>返回大写字符串</returns>
        //public static string Encrypt(byte[] p_InputBuff, CryptoEnum p_CryptoEnum, string p_SecretKey = "")
        //{
        //    return Generate(p_CryptoEnum, p_SecretKey).Encrypt(p_InputBuff);
        //}

        ///// <summary>
        ///// 加密
        ///// </summary>
        ///// <param name="p_InputBuff">待加密字节数组</param>
        ///// <param name="p_CryptoEnum">加密类类型 DEncryptEnum</param>
        ///// <param name="p_SecretKey">加密盐值密钥</param>
        ///// <returns>返回字节数组</returns>
        //public static byte[] Encrypt2Byte(byte[] p_InputBuff, CryptoEnum p_CryptoEnum, string p_SecretKey = "")
        //{
        //    return Generate(p_CryptoEnum, p_SecretKey).Encrypt2Byte(p_InputBuff);
        //}
    }

    /// <summary>
    /// 对称加密辅助类
    /// </summary>
    public static class DESCrypto
    {
        public enum CryptoEnum
        {
            AES,
            DES,
            TripleDES,
            RC2,
            Rijndael,
        }

        //private const int m_BufferSize = 1024;

        /// <summary>
        /// 简易生成密钥和初始化向量
        /// </summary>
        /// <param name="keyIV">任意字符串</param>
        /// <param name="cryptoEnum">加密类型</param>
        /// <returns>返回KeyValuePair,其中Key = Key,Value = IV</returns>
        public static KeyValuePair<string, string> KeyIV(string keyIV, CryptoEnum cryptoEnum = CryptoEnum.AES)
        {
            var m_Key = HASHCrypto.Generate(HASHCrypto.CryptoEnum.MD5).Encrypt(keyIV).ToLowerInvariant();
            var res = new KeyValuePair<string, string>(m_Key.Substring(0, 8), m_Key.Substring(24, 8));
            switch (cryptoEnum)
            {
                case CryptoEnum.AES:
                    {
                        res = new KeyValuePair<string, string>(m_Key.Substring(0, 16), m_Key.Substring(16, 16));
                    }
                    break;

                case CryptoEnum.DES:
                    {
                        res = new KeyValuePair<string, string>(m_Key.Substring(0, 24), m_Key.Substring(24, 8));
                    }
                    break;

                case CryptoEnum.TripleDES:
                    {
                        res = new KeyValuePair<string, string>(m_Key.Substring(0, 24), m_Key.Substring(24, 8));
                    }
                    break;

                case CryptoEnum.RC2:
                    {
                        res = new KeyValuePair<string, string>(m_Key.Substring(0, 16), m_Key.Substring(16, 8));
                    }
                    break;

                case CryptoEnum.Rijndael:
                    {
                        res = new KeyValuePair<string, string>(m_Key, m_Key.Substring(16, 16));
                    }
                    break;
            }
            return res;
        }

        //aes加解密
        //http://outofmemory.cn/code-snippet/35524/AES-with-javascript-java-csharp-python-or-php
        //http://blog.csdn.net/flysknow/article/details/324748?locationNum=2&fps=1
        //http://www.sufeinet.com/thread-12099-1-1.html
        public static SymmetricAlgorithm Generate(string keyIV,
            CryptoEnum cryptoEnum = CryptoEnum.AES,
            CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.PKCS7, int blockSize = 0, int keySize = 0)
        {
            var kv = KeyIV(keyIV, cryptoEnum);
            return Generate(kv.Key, kv.Value, cryptoEnum, cipherMode, paddingMode, blockSize, keySize);
        }

        /// <summary>
        /// 创建对称加密类
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <param name="cryptoEnum">加密类型</param>
        /// <param name="cipherMode">块密码模式</param>
        /// <param name="paddingMode">对其方式</param>
        /// <param name="blockSize">块大小</param>
        /// <param name="keySize">密钥大小</param>
        /// <returns>返回加密类SymmetricAlgorithm</returns>
        public static SymmetricAlgorithm Generate(string key, string iv,
            CryptoEnum cryptoEnum = CryptoEnum.AES,
            CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.PKCS7, int blockSize = 0, int keySize = 0)
        {
            SymmetricAlgorithm provider;
            switch (cryptoEnum)
            {
                case CryptoEnum.AES:
                    provider = Aes.Create();
                    break;

                case CryptoEnum.DES:
                    provider = DES.Create();
                    break;

                case CryptoEnum.TripleDES:
                    provider = TripleDES.Create();
                    break;

                case CryptoEnum.RC2:
                    provider = RC2.Create();
                    break;

                case CryptoEnum.Rijndael:
#pragma warning disable CS0618
                    provider = Rijndael.Create();
#pragma warning restore CS0618
                    break;

                default:
                    provider = Aes.Create();
                    break;
            }
            provider.Mode = cipherMode;
            provider.Padding = paddingMode;
            if (blockSize != 0)
            {
                provider.BlockSize = blockSize;
            }
            if (keySize != 0)
            {
                provider.KeySize = keySize;
            }
            switch (cryptoEnum)
            {
                case CryptoEnum.AES:
                    {
                        if (!string.IsNullOrWhiteSpace(key) && (Encoding.UTF8.GetByteCount(key) == 16 || Encoding.UTF8.GetByteCount(key) == 24 || Encoding.UTF8.GetByteCount(key) == 32))
                        {
                            provider.Key = Encoding.UTF8.GetBytes(key);
                        }
                        if (cipherMode != CipherMode.ECB)
                        {
                            if (!string.IsNullOrWhiteSpace(iv) && Encoding.UTF8.GetByteCount(iv) == 16)
                            {
                                provider.IV = Encoding.UTF8.GetBytes(iv);
                            }
                            else
                            {
                                provider.GenerateIV();
                            }
                        }
                    }
                    break;

                //todo 未测试
                case CryptoEnum.DES:
                    {
                        if (!string.IsNullOrWhiteSpace(key) && (Encoding.UTF8.GetByteCount(key) == 16 || Encoding.UTF8.GetByteCount(key) == 24))
                        {
                            provider.Key = Encoding.UTF8.GetBytes(key);
                        }
                        if (cipherMode != CipherMode.ECB)
                        {
                            if (!string.IsNullOrWhiteSpace(iv) && Encoding.UTF8.GetByteCount(iv) == 8)
                            {
                                provider.IV = Encoding.UTF8.GetBytes(iv);
                            }
                            else
                            {
                                provider.GenerateIV();
                            }
                        }
                    }
                    break;

                case CryptoEnum.TripleDES:
                    {
                        if (!string.IsNullOrWhiteSpace(key) && (Encoding.UTF8.GetByteCount(key) == 16 || Encoding.UTF8.GetByteCount(key) == 24))
                        {
                            provider.Key = Encoding.UTF8.GetBytes(key);
                        }
                        if (cipherMode != CipherMode.ECB)
                        {
                            if (!string.IsNullOrWhiteSpace(iv) && Encoding.UTF8.GetByteCount(iv) == 8)
                            {
                                provider.IV = Encoding.UTF8.GetBytes(iv);
                            }
                            else
                            {
                                provider.GenerateIV();
                            }
                        }
                    }
                    break;

                case CryptoEnum.RC2:
                    {
                        if (!string.IsNullOrWhiteSpace(key) && (Encoding.UTF8.GetByteCount(key) >= 5 && Encoding.UTF8.GetByteCount(key) <= 16))
                        {
                            provider.Key = Encoding.UTF8.GetBytes(key);
                        }
                        if (cipherMode != CipherMode.ECB)
                        {
                            if (!string.IsNullOrWhiteSpace(iv) && Encoding.UTF8.GetByteCount(iv) == 8)
                            {
                                provider.IV = Encoding.UTF8.GetBytes(iv);
                            }
                            else
                            {
                                provider.GenerateIV();
                            }
                        }
                    }
                    break;

                case CryptoEnum.Rijndael:
                    {
                        if (!string.IsNullOrWhiteSpace(key) && (Encoding.UTF8.GetByteCount(key) == 16 || Encoding.UTF8.GetByteCount(key) == 24 || Encoding.UTF8.GetByteCount(key) == 32))
                        {
                            provider.Key = Encoding.UTF8.GetBytes(key);
                        }
                        if (cipherMode != CipherMode.ECB)
                        {
                            if (!string.IsNullOrWhiteSpace(iv) && Encoding.UTF8.GetByteCount(iv) == 16)
                            {
                                provider.IV = Encoding.UTF8.GetBytes(iv);
                            }
                            else
                            {
                                provider.GenerateIV();
                            }
                        }
                    }
                    break;
            }
            return provider;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="provider">加密类SymmetricAlgorithm</param>
        /// <param name="inputText">待加密字符串</param>
        /// <returns>返回字节数组</returns>
        public static byte[] Encrypt2Byte(this SymmetricAlgorithm provider, string inputText)
        {
            if (inputText != null)
            {
                return Encrypt2Byte(provider, Encoding.UTF8.GetBytes(inputText));
            }
            return null;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="provider">加密类SymmetricAlgorithm</param>
        /// <param name="inputBuff">待加密字节数组</param>
        /// <returns>返回字节数组</returns>
        public static byte[] Encrypt2Byte(this SymmetricAlgorithm provider, byte[] inputBuff)
        {
            if (inputBuff != null)
            {
                ICryptoTransform transform = provider.CreateEncryptor();
                return transform.TransformFinalBlock(inputBuff, 0, inputBuff.Length);
            }
            return null;
        }

        public static string Encrypt(this SymmetricAlgorithm provider, string inputText)
        {
            if (inputText != null)
            {
                return Encrypt(provider, Encoding.UTF8.GetBytes(inputText));
            }
            return null;
        }

        public static string Encrypt(this SymmetricAlgorithm provider, byte[] inputBuff)
        {
            if (inputBuff != null)
            {
                return Convert.ToBase64String(Encrypt2Byte(provider, inputBuff));
            }
            return null;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="provider">加密类SymmetricAlgorithm</param>
        /// <param name="inputText">待加密字符串</param>
        /// <returns>返回字节数组</returns>
        public static byte[] Decrypt2Byte(this SymmetricAlgorithm provider, string inputText)
        {
            if (inputText != null)
            {
                return Decrypt2Byte(provider, Convert.FromBase64String(inputText));
            }
            return null;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="provider">加密类SymmetricAlgorithm</param>
        /// <param name="inputBuff">待解密字节数组</param>
        /// <returns>返回字节数组</returns>
        public static byte[] Decrypt2Byte(this SymmetricAlgorithm provider, byte[] inputBuff)
        {
            if (inputBuff != null)
            {
                ICryptoTransform transform = provider.CreateDecryptor();
                return transform.TransformFinalBlock(inputBuff, 0, inputBuff.Length);
            }
            return null;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="provider">加密类SymmetricAlgorithm</param>
        /// <param name="inputText">待加密字符串</param>
        /// <returns>返回字节数组</returns>
        public static string Decrypt(this SymmetricAlgorithm provider, string inputText)
        {
            if (inputText != null)
            {
                return Decrypt(provider, Convert.FromBase64String(inputText));
            }
            return null;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="provider">加密类SymmetricAlgorithm</param>
        /// <param name="inputBuff">待解密字节数组</param>
        /// <returns>返回字节数组</returns>
        public static string Decrypt(this SymmetricAlgorithm provider, byte[] inputBuff)
        {
            if (inputBuff != null)
            {
                return Encoding.UTF8.GetString(Decrypt2Byte(provider, inputBuff));
            }
            return null;
        }
    }

    /// <summary>
    /// 非对称加密辅助类（公钥私钥）
    /// </summary>
    public static class RSACrypto
    {
        class Extensions
        {
            public static readonly Regex _PEMCode = new Regex(@"--+.+?--+|\s+");
            public static readonly byte[] _SeqOID = new byte[] { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            public static readonly byte[] _Ver = new byte[] { 0x02, 0x01, 0x00 };

            /// <summary>
            /// 把字符串按每行多少个字断行
            /// </summary>
            public static string TextBreak(string text, int line)
            {
                var idx = 0;
                var len = text.Length;
                var str = new StringBuilder();
                while (idx < len)
                {
                    if (idx > 0)
                    {
                        str.Append('\n');
                    }

                    str.Append(
                        idx + line >= len ?
                            text.Substring(idx) :
                            text.Substring(idx, line)
                    );

                    idx += line;
                }
                return str.ToString();
            }

            /// <summary>
            /// 从数组start开始到指定长度复制一份
            /// </summary>
            public static T[] sub<T>(T[] arr, int start, int count)
            {
                T[] val = new T[count];
                for (var i = 0; i < count; i++)
                {
                    val[i] = arr[start + i];
                }
                return val;
            }

            public static void writeAll(Stream stream, byte[] bytes)
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            public static int GetIntegerSize(BinaryReader binaryReader)
            {
                byte bt = 0;
                byte lowbyte = 0x00;
                byte highbyte = 0x00;
                int count = 0;
                bt = binaryReader.ReadByte();
                if (bt != 0x02)     //expect integer
                    return 0;
                bt = binaryReader.ReadByte();

                if (bt == 0x81)
                    count = binaryReader.ReadByte();    // data size in next byte
                else
                if (bt == 0x82)
                {
                    highbyte = binaryReader.ReadByte(); // data size in next 2 bytes
                    lowbyte = binaryReader.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                }
                else
                {
                    count = bt;     // we already have the data size
                }

                while (binaryReader.ReadByte() == 0x00)
                {   //remove high order zeros in data
                    count -= 1;
                }
                binaryReader.BaseStream.Seek(-1, SeekOrigin.Current);       //last ReadByte wasn't a removed zero, so back up a byte
                return count;
            }
        }


        /// <summary>
        /// 获取证书ID
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        public static string CertId(this X509Certificate2 cert)
        {
            return string.IsNullOrWhiteSpace(cert.SerialNumber) ? "" : BigInteger.Parse(cert.SerialNumber, NumberStyles.HexNumber).ToString();
        }

        /// <summary>
        /// 根据路径和密码得到证书
        /// </summary>
        /// <param name="certPath"></param>
        /// <param name="certPwd"></param>
        /// <returns></returns>
        public static X509Certificate2 CertFile(string certPath, string certPwd = "")
        {
            return CertByte(File.ReadAllBytes(certPath), certPwd);
        }

        /// <summary>
        /// 根据字节数组和密码得到证书
        /// </summary>
        /// <param name="certData"></param>
        /// <param name="certPwd"></param>
        /// <returns></returns>
        public static X509Certificate2 CertByte(byte[] certData, string certPwd = "")
        {
            return string.IsNullOrWhiteSpace(certPwd) ? new X509Certificate2(certData) : new X509Certificate2(certData, certPwd, X509KeyStorageFlags.Exportable);
        }

        public static RSACryptoServiceProvider Cert2Provider(this X509Certificate2 cert, bool includePrivateParameters = false)
        {
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.PersistKeyInCsp = false;
            if (includePrivateParameters)
            {
                if (cert.HasPrivateKey)
                {
                    RSAParameters key =
                        (((RSACryptoServiceProvider)cert.GetRSAPrivateKey())!).ExportParameters(true);
                    provider.ImportParameters(key);
                    //provider.FromXmlString(cert.PrivateKey.ToXmlString(true));
                }
                else
                {
                    provider = null;
                }
            }
            else
            {
                RSAParameters key =
                    (((RSACryptoServiceProvider)cert.GetRSAPublicKey())!).ExportParameters(false);
                provider.ImportParameters(key);
                //provider.FromXmlString(cert.PublicKey.Key.ToXmlString(false));
            }
            return provider;
        }

        /// <summary>
        /// RSACryptoServiceProvider
        /// 如果安装了 Microsoft 增强的加密提供程序, 则支持从384位到16384位的密钥大小 
        /// 如果安装了 Microsoft 基本加密提供程序, 则它支持从384位到512位的密钥大小
        /// </summary>
        /// <param name="keySize">384位 - 512位 (增量8位)</param>
        /// <returns></returns>
        //[SupportedOSPlatform(("windows"))]
        public static RSACryptoServiceProvider Generate(int keySize)
        {
            var rsaParams = new CspParameters
            {
                Flags = CspProviderFlags.UseMachineKeyStore
            };
            return new RSACryptoServiceProvider(keySize, rsaParams);
        }

        public static RSACryptoServiceProvider FromXmlKey(this string xmlString)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            if (xmlDoc.DocumentElement != null && xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                RSAParameters parameters = new RSAParameters();
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus":
                            parameters.Modulus = Convert.FromBase64String(node.InnerText);
                            break;

                        case "Exponent":
                            parameters.Exponent = Convert.FromBase64String(node.InnerText);
                            break;

                        case "P":
                            parameters.P = Convert.FromBase64String(node.InnerText);
                            break;

                        case "Q":
                            parameters.Q = Convert.FromBase64String(node.InnerText);
                            break;

                        case "DP":
                            parameters.DP = Convert.FromBase64String(node.InnerText);
                            break;

                        case "DQ":
                            parameters.DQ = Convert.FromBase64String(node.InnerText);
                            break;

                        case "InverseQ":
                            parameters.InverseQ = Convert.FromBase64String(node.InnerText);
                            break;

                        case "D":
                            parameters.D = Convert.FromBase64String(node.InnerText);
                            break;
                    }
                }
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.PersistKeyInCsp = false;
                provider.ImportParameters(parameters);

                return provider;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_Provider"></param>
        /// <param name="convertToPublic">convertToPublic=true含私钥的RSA将只返回公钥，仅含公钥的RSA不受影响</param>
        /// <returns></returns>
        public static string ToXmlKey(this RSACryptoServiceProvider p_Provider, bool convertToPublic = false)
        {
            if (!convertToPublic)
            {
                if (!p_Provider.PublicOnly)
                {
                    RSAParameters parameters = p_Provider.ExportParameters(true);
                    return string.Format(
                        "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                        Convert.ToBase64String(parameters.Modulus),
                        Convert.ToBase64String(parameters.Exponent),
                        Convert.ToBase64String(parameters.P),
                        Convert.ToBase64String(parameters.Q),
                        Convert.ToBase64String(parameters.DP),
                        Convert.ToBase64String(parameters.DQ),
                        Convert.ToBase64String(parameters.InverseQ),
                        Convert.ToBase64String(parameters.D));
                }
                else
                {
                    RSAParameters parameters = p_Provider.ExportParameters(false);
                    return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                        Convert.ToBase64String(parameters.Modulus),
                        Convert.ToBase64String(parameters.Exponent));
                }
            }
            else
            {
                RSAParameters parameters = p_Provider.ExportParameters(false);
                return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                    Convert.ToBase64String(parameters.Modulus),
                    Convert.ToBase64String(parameters.Exponent));
            }
        }

        /// <summary>
        /// 用PEM格式密钥对创建RSA，支持PKCS#1、PKCS#8格式的PEM
        /// </summary>
        //[SupportedOSPlatform(("windows"))]
        public static RSACryptoServiceProvider FromPEM(string pem)
        {
            var rsaParams = new CspParameters();
            rsaParams.Flags = CspProviderFlags.UseMachineKeyStore;
            var rsa = new RSACryptoServiceProvider(rsaParams);

            var param = new RSAParameters();

            var base64 = Extensions._PEMCode.Replace(pem, "");
            var data = Convert.FromBase64String(base64);
            if (data == null)
            {
                throw new Exception("PEM内容无效");
            }
            var idx = 0;

            //读取长度
            Func<byte, int> readLen = (first) =>
            {
                if (data[idx] == first)
                {
                    idx++;
                    if (data[idx] == 0x81)
                    {
                        idx++;
                        return data[idx++];
                    }
                    else if (data[idx] == 0x82)
                    {
                        idx++;
                        return (((int)data[idx++]) << 8) + data[idx++];
                    }
                    else if (data[idx] < 0x80)
                    {
                        return data[idx++];
                    }
                }
                throw new Exception("PEM未能提取到数据");
            };
            //读取块数据
            Func<byte[]> readBlock = () =>
            {
                var len = readLen(0x02);
                if (data[idx] == 0x00)
                {
                    idx++;
                    len--;
                }
                var val = Extensions.sub(data, idx, len);
                idx += len;
                return val;
            };
            //比较data从idx位置开始是否是byts内容
            Func<byte[], bool> eq = (byts) =>
            {
                for (var i = 0; i < byts.Length; i++, idx++)
                {
                    if (idx >= data.Length)
                    {
                        return false;
                    }
                    if (byts[i] != data[idx])
                    {
                        return false;
                    }
                }
                return true;
            };




            if (pem.Contains("PUBLIC KEY"))
            {
                /****使用公钥****/
                //读取数据总长度
                readLen(0x30);
                if (!eq(Extensions._SeqOID))
                {
                    throw new Exception("PEM未知格式");
                }
                //读取1长度
                readLen(0x03);
                idx++;//跳过0x00
                      //读取2长度
                readLen(0x30);

                //Modulus
                param.Modulus = readBlock();

                //Exponent
                param.Exponent = readBlock();
            }
            else if (pem.Contains("PRIVATE KEY"))
            {
                /****使用私钥****/
                //读取数据总长度
                readLen(0x30);

                //读取版本号
                if (!eq(Extensions._Ver))
                {
                    throw new Exception("PEM未知版本");
                }

                //检测PKCS8
                var idx2 = idx;
                if (eq(Extensions._SeqOID))
                {
                    //读取1长度
                    readLen(0x04);
                    //读取2长度
                    readLen(0x30);

                    //读取版本号
                    if (!eq(Extensions._Ver))
                    {
                        throw new Exception("PEM版本无效");
                    }
                }
                else
                {
                    idx = idx2;
                }

                //读取数据
                param.Modulus = readBlock();
                param.Exponent = readBlock();
                param.D = readBlock();
                param.P = readBlock();
                param.Q = readBlock();
                param.DP = readBlock();
                param.DQ = readBlock();
                param.InverseQ = readBlock();
            }
            else
            {
                throw new Exception("pem需要BEGIN END标头");
            }

            rsa.ImportParameters(param);
            return rsa;
        }

        /// <summary>
        /// 将RSA中的密钥对转换成PEM格式
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="convertToPublic">convertToPublic=true含私钥的RSA将只返回公钥，仅含公钥的RSA不受影响</param>
        /// <param name="usePKCS8">usePKCS8=false时返回PKCS#1格式，否则返回PKCS#8格式</param>
        /// <returns></returns>
        public static string ToPEM(this RSACryptoServiceProvider rsa, bool convertToPublic = false, bool usePKCS8 = false)
        {
            //https://www.jianshu.com/p/25803dd9527d
            //https://www.cnblogs.com/ylz8401/p/8443819.html
            //https://blog.csdn.net/jiayanhui2877/article/details/47187077
            //https://blog.csdn.net/xuanshao_/article/details/51679824
            //https://blog.csdn.net/xuanshao_/article/details/51672547

            var ms = new MemoryStream();
            //写入一个长度字节码
            Action<int> writeLenByte = (len) =>
            {
                if (len < 0x80)
                {
                    ms.WriteByte((byte)len);
                }
                else if (len <= 0xff)
                {
                    ms.WriteByte(0x81);
                    ms.WriteByte((byte)len);
                }
                else
                {
                    ms.WriteByte(0x82);
                    ms.WriteByte((byte)(len >> 8 & 0xff));
                    ms.WriteByte((byte)(len & 0xff));
                }
            };
            //写入一块数据
            Action<byte[]> writeBlock = (bytes) =>
            {
                var addZero = (bytes[0] >> 4) >= 0x8;
                ms.WriteByte(0x02);
                var len = bytes.Length + (addZero ? 1 : 0);
                writeLenByte(len);

                if (addZero)
                {
                    ms.WriteByte(0x00);
                }
                ms.Write(bytes, 0, bytes.Length);
            };
            //根据后续内容长度写入长度数据
            Func<int, byte[], byte[]> writeLen = (index, bytes) =>
            {
                var len = bytes.Length - index;

                ms.SetLength(0);
                ms.Write(bytes, 0, index);
                writeLenByte(len);
                ms.Write(bytes, index, len);

                return ms.ToArray();
            };


            if (rsa.PublicOnly || convertToPublic)
            {
                /****生成公钥****/
                var param = rsa.ExportParameters(false);


                //写入总字节数，不含本段长度，额外需要24字节的头，后续计算好填入
                ms.WriteByte(0x30);
                var index1 = (int)ms.Length;

                //固定内容
                // encoded OID sequence for PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
                Extensions.writeAll(ms, Extensions._SeqOID);

                //从0x00开始的后续长度
                ms.WriteByte(0x03);
                var index2 = (int)ms.Length;
                ms.WriteByte(0x00);

                //后续内容长度
                ms.WriteByte(0x30);
                var index3 = (int)ms.Length;

                //写入Modulus
                writeBlock(param.Modulus);

                //写入Exponent
                writeBlock(param.Exponent);


                //计算空缺的长度
                var byts = ms.ToArray();

                byts = writeLen(index3, byts);
                byts = writeLen(index2, byts);
                byts = writeLen(index1, byts);


                return "-----BEGIN PUBLIC KEY-----\n" + Extensions.TextBreak(Convert.ToBase64String(byts), 64) + "\n-----END PUBLIC KEY-----";
            }
            else
            {
                /****生成私钥****/
                var param = rsa.ExportParameters(true);

                //写入总字节数，后续写入
                ms.WriteByte(0x30);
                int index1 = (int)ms.Length;

                //写入版本号
                Extensions.writeAll(ms, Extensions._Ver);

                //PKCS8 多一段数据
                int index2 = -1, index3 = -1;
                if (usePKCS8)
                {
                    //固定内容
                    Extensions.writeAll(ms, Extensions._SeqOID);

                    //后续内容长度
                    ms.WriteByte(0x04);
                    index2 = (int)ms.Length;

                    //后续内容长度
                    ms.WriteByte(0x30);
                    index3 = (int)ms.Length;

                    //写入版本号
                    Extensions.writeAll(ms, Extensions._Ver);
                }

                //写入数据
                writeBlock(param.Modulus);
                writeBlock(param.Exponent);
                writeBlock(param.D);
                writeBlock(param.P);
                writeBlock(param.Q);
                writeBlock(param.DP);
                writeBlock(param.DQ);
                writeBlock(param.InverseQ);


                //计算空缺的长度
                var bytes = ms.ToArray();

                if (index2 != -1)
                {
                    bytes = writeLen(index3, bytes);
                    bytes = writeLen(index2, bytes);
                }
                bytes = writeLen(index1, bytes);


                var flag = " PRIVATE KEY";
                if (!usePKCS8)
                {
                    flag = " RSA" + flag;
                }
                return "-----BEGIN" + flag + "-----\n" + Extensions.TextBreak(Convert.ToBase64String(bytes), 64) + "\n-----END" + flag + "-----";
            }
        }

        /// <summary>
        /// 判断加密类RSACryptoServiceProvider 仅有公钥
        /// </summary>
        /// <param name="provider">加密类RSACryptoServiceProvider</param>
        /// <returns>公钥则为true 私钥则为false</returns>
        public static bool IsPublicOnly(this RSACryptoServiceProvider provider)
        {
            return provider.PublicOnly;
        }

        /// <summary>
        /// 判断X509Certificate2证书 仅有公钥
        /// </summary>
        /// <param name="cert">X509Certificate2证书</param>
        /// <returns>公钥则为true 私钥则为false</returns>
        public static bool IsPublicOnly(this X509Certificate2 cert)
        {
            return !cert.HasPrivateKey;
        }

        /// <summary>
        /// 加密  .net平台默认公钥加密 私钥解密
        /// </summary>
        /// <param name="provider">加密类RSACryptoServiceProvider</param>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static byte[] Encrypt2Byte(this RSACryptoServiceProvider provider, string inputString)
        {
            if (inputString != null)
            {
                return Encrypt2Byte(provider, Encoding.UTF8.GetBytes(inputString));
            }
            return null;
        }

        /// <summary>
        /// 加密  .net平台默认公钥加密 私钥解密
        /// </summary>
        /// <param name="provider">加密类RSACryptoServiceProvider</param>
        /// <param name="inputBuff"></param>
        /// <returns></returns>
        public static byte[] Encrypt2Byte(this RSACryptoServiceProvider provider, byte[] inputBuff)
        {
            if (inputBuff != null)
            {
                if (provider.IsPublicOnly())
                {
                    var maxBlockSize = provider.KeySize / 8 - 11;
                    if (inputBuff.Length <= maxBlockSize)
                    {
                        return provider.Encrypt(inputBuff, false);
                    }
                    using (MemoryStream plaiStream = new MemoryStream(inputBuff))
                    {
                        using (MemoryStream crypStream = new MemoryStream())
                        {
                            byte[] buffer = new byte[maxBlockSize];
                            int blockSize = plaiStream.Read(buffer, 0, maxBlockSize);
                            while (blockSize > 0)
                            {
                                byte[] toEncrypt = new byte[blockSize];
                                Array.Copy(buffer, 0, toEncrypt, 0, blockSize);
                                byte[] data = provider.Encrypt(toEncrypt, false);
                                crypStream.Write(data, 0, data.Length);
                                blockSize = plaiStream.Read(buffer, 0, maxBlockSize);
                            }
                            return crypStream.ToArray();
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 加密  .net平台默认公钥加密 私钥解密
        /// </summary>
        /// <param name="provider">加密类RSACryptoServiceProvider</param>
        /// <param name="p_InputString"></param>
        /// <returns></returns>
        public static string Encrypt(this RSACryptoServiceProvider provider, string p_InputString)
        {
            if (p_InputString != null)
            {
                return Encrypt(provider, Encoding.UTF8.GetBytes(p_InputString));
            }
            return null;
        }

        /// <summary>
        /// 加密  .net平台默认公钥加密 私钥解密
        /// </summary>
        /// <param name="provider">加密类RSACryptoServiceProvider</param>
        /// <param name="p_InputBuff"></param>
        /// <returns></returns>
        public static string Encrypt(this RSACryptoServiceProvider provider, byte[] p_InputBuff)
        {
            if (p_InputBuff != null)
            {
                return Convert.ToBase64String(Encrypt2Byte(provider, p_InputBuff));
            }
            return null;
        }

        /// <summary>
        /// 解密  .net平台默认公钥加密 私钥解密
        /// </summary>
        /// <param name="provider">加密类RSACryptoServiceProvider</param>
        /// <param name="p_InputString"></param>
        /// <returns></returns>
        public static byte[] Decrypt2Byte(this RSACryptoServiceProvider provider, string p_InputString)
        {
            if (p_InputString != null)
            {
                return Decrypt2Byte(provider, Convert.FromBase64String(p_InputString));
            }
            return null;
        }

        /// <summary>
        ///  解密  .net平台默认公钥加密 私钥解密
        /// </summary>
        /// <param name="provider">加密类RSACryptoServiceProvider</param>
        /// <param name="p_InputBuff"></param>
        /// <returns></returns>
        public static byte[] Decrypt2Byte(this RSACryptoServiceProvider provider, byte[] p_InputBuff)
        {
            if (p_InputBuff != null)
            {
                if (!provider.IsPublicOnly())
                {
                    int maxBlockSize = provider.KeySize / 8;
                    if (p_InputBuff.Length <= maxBlockSize)
                    {
                        return provider.Decrypt(p_InputBuff, false);
                    }
                    using (MemoryStream crypStream = new MemoryStream(p_InputBuff))
                    {
                        using (MemoryStream plaiStream = new MemoryStream())
                        {
                            byte[] buffer = new byte[maxBlockSize];
                            int blockSize = crypStream.Read(buffer, 0, maxBlockSize);
                            while (blockSize > 0)
                            {
                                byte[] toDecrypt = new byte[blockSize];
                                Array.Copy(buffer, 0, toDecrypt, 0, blockSize);
                                byte[] data = provider.Decrypt(toDecrypt, false);
                                plaiStream.Write(data, 0, data.Length);
                                blockSize = crypStream.Read(buffer, 0, maxBlockSize);
                            }
                            return plaiStream.ToArray(); //得到解密结果
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 解密  .net平台默认公钥加密 私钥解密
        /// </summary>
        /// <param name="provider">加密类RSACryptoServiceProvider</param>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string Decrypt(this RSACryptoServiceProvider provider, string inputString)
        {
            if (inputString != null)
            {
                return Decrypt(provider, Convert.FromBase64String(inputString));
            }
            return null;
        }

        /// <summary>
        ///  解密  .net平台默认公钥加密 私钥解密
        /// </summary>
        /// <param name="provider">加密类RSACryptoServiceProvider</param>
        /// <param name="inputBuff"></param>
        /// <returns></returns>
        public static string Decrypt(this RSACryptoServiceProvider provider, byte[] inputBuff)
        {
            if (inputBuff != null)
            {
                return Encoding.UTF8.GetString(Decrypt2Byte(provider, inputBuff));
            }
            return null;
        }

        /// <summary>
        /// 计算签名
        /// </summary>
        /// <param name="provider">加密类RSACryptoServiceProvider</param>
        /// <param name="inputBuff">要生成签名的数据</param>
        /// <param name="hash_enum">签名类型</param>
        /// <returns></returns>
        public static byte[] SignData(this RSACryptoServiceProvider provider, HASHCrypto.CryptoEnum hash_enum, byte[] inputBuff)
        {
            if (inputBuff != null)
            {
                HashAlgorithm ha;
                switch (hash_enum)
                {
                    case HASHCrypto.CryptoEnum.MD5:
                        ha = MD5.Create();
                        break;

                    case HASHCrypto.CryptoEnum.SHA1:
                        ha = SHA1.Create();
                        break;

                    case HASHCrypto.CryptoEnum.SHA256:
                        ha = SHA256.Create();
                        break;

                    case HASHCrypto.CryptoEnum.SHA384:
                        ha = SHA384.Create();
                        break;

                    case HASHCrypto.CryptoEnum.SHA512:
                        ha = SHA512.Create();
                        break;

                    default:
                        ha = MD5.Create();
                        break;
                }
                return provider.SignData(inputBuff, ha);
            }
            return null;
        }

        /// <summary>
        /// 签名验证
        /// </summary>
        /// <param name="provider">加密类RSACryptoServiceProvider</param>
        /// <param name="signedBuff">签名数据</param>
        /// <param name="inputBuff">要验证的数据</param>
        /// <param name="hash_enum">签名类型</param>
        /// <returns></returns>
        public static bool VerifyData(this RSACryptoServiceProvider provider, HASHCrypto.CryptoEnum hash_enum, byte[] signedBuff, byte[] inputBuff)
        {
            if (inputBuff != null)
            {
                HashAlgorithm ha;
                switch (hash_enum)
                {
                    case HASHCrypto.CryptoEnum.MD5:
                        ha = MD5.Create();
                        break;

                    case HASHCrypto.CryptoEnum.SHA1:
                        ha = SHA1.Create();
                        break;

                    case HASHCrypto.CryptoEnum.SHA256:
                        ha = SHA256.Create();
                        break;

                    case HASHCrypto.CryptoEnum.SHA384:
                        ha = SHA384.Create();
                        break;

                    case HASHCrypto.CryptoEnum.SHA512:
                        ha = SHA512.Create();
                        break;

                    default:
                        ha = MD5.Create();
                        break;
                }
                return provider.VerifyData(inputBuff, ha, signedBuff);
            }
            return false;
        }

        ///// <summary>
        ///// 签名验证  VerifyHash 和 VerifyData 方法等价
        ///// </summary>
        ///// <param name="p_Provider">加密类RSACryptoServiceProvider</param>
        ///// <param name="p_SignedBuff">签名数据</param>
        ///// <param name="p_InputBuff">要验证的数据</param>
        ///// <param name="p_OID">签名类型</param>
        ///// <returns></returns>
        //public static bool VerifyHash(RSACryptoServiceProvider p_Provider, byte[] p_SignedBuff,
        //    byte[] p_InputBuffHash, string p_Oid)
        //{
        //    if (p_InputBuffHash != null)
        //    {
        //        return p_Provider.VerifyHash(p_InputBuffHash, p_Oid, p_SignedBuff);
        //    }
        //    return false;

        //    //https://www.mgenware.com/blog/?p=105
        //    //using (var rsa = new RSACryptoServiceProvider())
        //    //using (var sha1 = SHA1.Create())
        //    //{
        //    //    //原始数据
        //    //    var data = new byte[] { 1, 2, 3 };
        //    //    //计算哈希值
        //    //    var hash = sha1.ComputeHash(data);

        //    //    //用SignHash，注意使用CryptoConfig.MapNameToOID
        //    //    var sigHash = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
        //    //    //用SignData，直接传入数据（函数内部会计算哈希值）
        //    //    var sigData = rsa.SignData(data, typeof(SHA1));

        //    //    //输出两个签名数据
        //    //    Console.WriteLine(BitConverter.ToString(sigHash));
        //    //    Console.WriteLine(BitConverter.ToString(sigData));

        //    //    //验证
        //    //    Console.WriteLine(rsa.VerifyHash(hash, "SHA1", sigHash));
        //    //    Console.WriteLine(rsa.VerifyData(data, typeof(SHA1), sigData));
        //    //}
        //}
    }

}