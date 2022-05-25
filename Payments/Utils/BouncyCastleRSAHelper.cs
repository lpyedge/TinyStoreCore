using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Xml;

namespace Payments.Utils
{
    public static class BouncyCastleRSAHelper
    {
        private static AsymmetricKeyParameter GetPublicKeyParameter(string s)
        {
            s = s.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            byte[] publicInfoByte = Convert.FromBase64String(s);
            Asn1Object pubKeyObj = Asn1Object.FromByteArray(publicInfoByte); //这里也可以从流中读取，从本地导入
            AsymmetricKeyParameter pubKey = PublicKeyFactory.CreateKey(publicInfoByte);
            return pubKey;
        }

        private static AsymmetricKeyParameter GetPrivateKeyParameter(string s)
        {
            s = s.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            byte[] privateInfoByte = Convert.FromBase64String(s);
            // Asn1Object priKeyObj = Asn1Object.FromByteArray(privateInfoByte);//这里也可以从流中读取，从本地导入
            // PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);
            AsymmetricKeyParameter priKey = PrivateKeyFactory.CreateKey(privateInfoByte);
            return priKey;
        }

        public static bool IsXmlPublicKey(string publickey)
        {
            return publickey.EndsWith("</Exponent></RSAKeyValue>", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsXmlPrivateKey(string privatekey)
        {
            return privatekey.EndsWith("</D></RSAKeyValue>", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// RSA私钥格式转换，java->.net
        /// </summary>
        /// <param name="privateKey">java生成的RSA私钥</param>
        /// <returns></returns>
        public static string RSAPrivateKeyJava2DotNet(string privateKey)
        {
            RsaPrivateCrtKeyParameters privateKeyParam =
                (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        /// <summary>
        /// RSA私钥格式转换，.net->java
        /// </summary>
        /// <param name="privateKey">.net生成的私钥</param>
        /// <returns></returns>
        public static string RSAPrivateKeyDotNet2Java(string privateKey)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(privateKey);
            BigInteger m = new BigInteger(1,
                Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
            BigInteger exp = new BigInteger(1,
                Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
            BigInteger d = new BigInteger(1,
                Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("D")[0].InnerText));
            BigInteger p = new BigInteger(1,
                Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("P")[0].InnerText));
            BigInteger q = new BigInteger(1,
                Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Q")[0].InnerText));
            BigInteger dp = new BigInteger(1,
                Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DP")[0].InnerText));
            BigInteger dq = new BigInteger(1,
                Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DQ")[0].InnerText));
            BigInteger qinv = new BigInteger(1,
                Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("InverseQ")[0].InnerText));
            RsaPrivateCrtKeyParameters privateKeyParam = new RsaPrivateCrtKeyParameters(m, exp, d, p, q, dp, dq, qinv);
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParam);
            byte[] serializedPrivateBytes = privateKeyInfo.ToAsn1Object().GetEncoded();
            return Convert.ToBase64String(serializedPrivateBytes);
        }

        /// <summary>
        /// RSA公钥格式转换，java->.net
        /// </summary>
        /// <param name="publicKey">java生成的公钥</param>
        /// <returns></returns>
        public static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam =
                (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        /// <summary>
        /// RSA公钥格式转换，.net->java
        /// </summary>
        /// <param name="publicKey">.net生成的公钥</param>
        /// <returns></returns>
        public static string RSAPublicKeyDotNet2Java(string publicKey)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(publicKey);
            BigInteger m = new BigInteger(1,
                Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
            BigInteger p = new BigInteger(1,
                Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
            RsaKeyParameters pub = new RsaKeyParameters(false, m, p);
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pub);
            byte[] serializedPublicBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            return Convert.ToBase64String(serializedPublicBytes);
        }

        public static byte[] EncryptByPrivateKey(byte[] input, string key)
        {
            if (IsXmlPrivateKey(key))
                key = RSAPrivateKeyDotNet2Java(key);

            //非对称加密算法，加解密用
            IAsymmetricBlockCipher engine = new Pkcs1Encoding(new RsaEngine());
            //加密
            try
            {
                engine.Init(true, GetPrivateKeyParameter(key));

                Func<byte[], byte[]> encrypt = sou =>
                {
                    int maxBlockSize = engine.GetInputBlockSize();
                    if (sou.Length <= maxBlockSize)
                    {
                        return engine.ProcessBlock(sou, 0, sou.Length);
                    }
                    using (MemoryStream plaiStream = new MemoryStream(sou))
                    {
                        using (MemoryStream crypStream = new MemoryStream())
                        {
                            byte[] buffer = new byte[maxBlockSize];
                            int blockSize = plaiStream.Read(buffer, 0, maxBlockSize);

                            while (blockSize > 0)
                            {
                                byte[] toEncrypt = new byte[blockSize];
                                Array.Copy(buffer, 0, toEncrypt, 0, blockSize);

                                byte[] cryptograph = engine.ProcessBlock(toEncrypt, 0, blockSize);
                                crypStream.Write(cryptograph, 0, cryptograph.Length);

                                blockSize = plaiStream.Read(buffer, 0, maxBlockSize);
                            }

                            return crypStream.ToArray();
                        }
                    }
                };
                return encrypt(input);
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public static byte[] DecryptByPublicKey(byte[] input, string key)
        {
            if (IsXmlPublicKey(key))
                key = RSAPublicKeyDotNet2Java(key);

            //非对称加密算法，加解密用
            IAsymmetricBlockCipher engine = new Pkcs1Encoding(new RsaEngine());
            //解密
            try
            {
                engine.Init(false, GetPublicKeyParameter(key));

                Func<byte[], byte[]> decrypt = sou =>
                {
                    var maxBlockSize = engine.GetInputBlockSize();
                    if (sou.Length <= maxBlockSize)
                        return engine.ProcessBlock(sou, 0, sou.Length);
                    ;
                    using (MemoryStream crypStream = new MemoryStream(sou))
                    {
                        using (MemoryStream plaiStream = new MemoryStream())
                        {
                            byte[] buffer = new byte[maxBlockSize];
                            int blockSize = crypStream.Read(buffer, 0, maxBlockSize);

                            while (blockSize > 0)
                            {
                                byte[] toDecrypt = new byte[blockSize];
                                Array.Copy(buffer, 0, toDecrypt, 0, blockSize);

                                byte[] plaintext = engine.ProcessBlock(toDecrypt, 0, blockSize);
                                plaiStream.Write(plaintext, 0, plaintext.Length);

                                blockSize = crypStream.Read(buffer, 0, maxBlockSize);
                            }

                            return plaiStream.ToArray();
                        }
                    }
                };
                return decrypt(input);
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }
}