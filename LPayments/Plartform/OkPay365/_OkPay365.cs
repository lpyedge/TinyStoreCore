// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Security.Cryptography;
// using System.Text;
// using System.Web;
// using LPayments.Utils;
//
// namespace LPayments.Plartform.OkPay365
// {
//     //盛腾鑫商贸测试参数
//     //测是商户号：928000000001403
//     //测试终端号：00019221
//     //测试编号：100968 (GroupId)
//     //测试密钥：ebdd81
//
//     //测试平台：http://164a2test.jszhongqu.com:8042/RBGroupManagerNew-Sale
//     //测试平台登陆账号：928000000001403
//     //测试平台登陆密码：00019221
//     //测试访问地址：http://121.41.121.164:8044/TransInterface/TransRequest
//
//     public abstract class _OkPay365 : IPayChannel, IBankTransfer
//     {
//         public const string GroupId = "GroupId";
//         public const string MerchantCode = "MerchantCode";
//         public const string MerchantName = "MerchantName";
//         public const string MerchantSubCode = "MerchantSubCode";
//         public const string TerminalCode = "TerminalCode";
//         public const string PublicRSAXml = "PublicRSAXml";
//         public const string MerchantNum = "MerchantNum";
//         public const string TerminalNum = "TerminalNum";
//
//         protected string m_service = "";
//         protected string m_bankCode = "";
//         protected string m_bankCard = "";
//         protected bool m_qrcode = true;
//
//         private readonly RSACryptoServiceProvider m_PublicRSAProvider;
//
//         protected _OkPay365()
//         {
//             Init();
//         }
//
//         protected _OkPay365(string p_SettingsJson)
//         {
//             Init();
//
//             if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//
//             try
//             {
//                 m_PublicRSAProvider = new RSACryptoServiceProvider();
//                 m_PublicRSAProvider.FromXmlString(this[PublicRSAXml]);
//             }
//             catch (Exception ex)
//             {
//             }
//         }
//
//         protected override void Init()
//         {
//             Settings = new List<Setting>
//             {
//                 new Setting {Name = GroupId, Description = "行付天下的合作编号", Regex = @"^\w+$", Requied = true},
//                 new Setting {Name = MerchantCode, Description = "行付天下的平台商户编号", Regex = @"^\w+$", Requied = true},
//                 new Setting {Name = MerchantSubCode, Description = "行付天下的平台商户子商户号", Regex = @"^\w+$", Requied = true},
//                 new Setting {Name = TerminalCode, Description = "行付天下的平台商户终端号", Regex = @"^\w+$", Requied = true},
//                 new Setting {Name = MerchantName, Description = "行付天下的收款商户名称", Regex = @"^\w+$", Requied = true},
//                 new Setting {Name = MerchantNum, Description = "行付天下的商户门店编号", Regex = @"^[\w\-]+$", Requied = true},
//                 new Setting {Name = TerminalNum, Description = "行付天下的商户机具终端编号", Regex = @"^[\w\-]+$", Requied = true},
//                 new Setting
//                 {
//                     Name = PublicRSAXml, Description = "行付天下的XML公钥", Regex = @"^[\w\-\.\\:;&+=/<>]+$", Requied = true
//                 },
//             };
//
//             Currencies = new List<ECurrency>
//             {
//                 ECurrency.CNY
//             };
//         }
//
//         public override PayResult Notify(Microsoft.AspNetCore.Http.HttpContext context)
//         {
//             return base.Notify(context);
//         }
//
//         public override PayResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
//             IDictionary<string, string> head, string body, string notifyip)
//         {
//             var sign = form["pl_sign"].Replace(" ", "+"); //替换空格为+符号
//             var res = Encoding.UTF8.GetString(
//                 BouncyCastleRSAHelper.DecryptByPublicKey(Convert.FromBase64String(sign), this[PublicRSAXml]));
//             if (res.Contains("&") && res.Contains("="))
//             {
//                 var dic = new Dictionary<string, string>();
//                 foreach (var item in res.Split(new char[] {'&'}, StringSplitOptions.RemoveEmptyEntries))
//                 {
//                     var equalsindex = item.IndexOf('=');
//                     if (equalsindex > 0)
//                         dic.Add(item.Substring(0, equalsindex),
//                             item.Substring(equalsindex + 1, (item.Length - 1 - equalsindex)));
//                 }
//
//                 if (string.Equals(dic["pl_payState"], "4") && form["pl_code"] == "0000")
//                 {
//                     var Result = new PayResult
//                     {
//                         OrderName = "",
//                         OrderID = dic["orderNum"],
//                         Amount = -1,
//                         Tax = -1,
//                         Currency = ECurrency.CNY,
//                         Business = this[MerchantName],
//                         TxnID = dic["pl_datetime"],
//                         PaymentName = Name,
//                         PaymentDate = DateTime.UtcNow,
//
//                         Message = "SUCCESS",
//                     };
//                     return Result;
//                 }
//             }
//
//             return null;
//         }
//
//         public override PayTicket Pay(string p_OrderId, double p_Amount,
//             ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
//             string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (string.IsNullOrEmpty(this[GroupId])) throw new ArgumentNullException("GroupId");
//             if (string.IsNullOrEmpty(this[MerchantCode])) throw new ArgumentNullException("MerchantCode");
//             if (string.IsNullOrEmpty(this[TerminalCode])) throw new ArgumentNullException("TerminalCode");
//             if (string.IsNullOrEmpty(this[MerchantName])) throw new ArgumentNullException("MerchantName");
//             if (string.IsNullOrEmpty(this[MerchantNum])) throw new ArgumentNullException("MerchantNum");
//             if (string.IsNullOrEmpty(this[TerminalNum])) throw new ArgumentNullException("TerminalNum");
//             if (string.IsNullOrEmpty(this[PublicRSAXml])) throw new ArgumentNullException("PublicRSAXml");
//
//             if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");
//             if (p_Amount <= 0) throw new ArgumentException("p_Amount must large than 0!");
//             if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");
//
//
//             var pt = new PayTicket();
//
//             var dic = new Dictionary<string, string>();
//
//             dic.Add("merchantCode", this[MerchantCode]); //平台商户编号
//             dic.Add("terminalCode", this[TerminalCode]); //平台商户终端编号
//             dic.Add("orderNum", p_OrderId); //合作商订单号，全局唯一
//             dic.Add("transMoney", (p_Amount * 100).ToString("0")); //交易金额，单位分
//             dic.Add("merchantName", this[MerchantName]); //收款商户名称
//             dic.Add("commodityName", p_OrderName); //商品名称
//             dic.Add("merchantNum", this[MerchantNum]); //商户门店编号
//             dic.Add("notifyUrl", p_NotifyUrl);
//
//             //网银
//             if (m_qrcode)
//             {
//                 dic.Add("merchantSubCode", this[MerchantSubCode] ?? ""); //平台商户子商户号
//                 dic.Add("terminalNum", this[TerminalNum]); //商户机具终端编号
//             }
//             else
//             {
//                 dic.Add("returnUrl", p_ReturnUrl);
//
//                 dic.Add("bankCode", m_bankCode);
//                 dic.Add("bankCard", m_bankCard);
//             }
//
//             var signstr = dic.Aggregate("", (x, y) => x + y.Key + "=" + y.Value + "&").TrimEnd('&');
//
//             //var sign = Convert.ToBase64String(m_PublicRSAProvider.PublicEncrypt(Encoding.UTF8.GetBytes(signstr)));
//             var sign = m_PublicRSAProvider.Encrypt(signstr);
//             sign = sign.Replace(" ", "+"); //替换空格为+符号
//             dic.Add("groupId", this[GroupId]); //合作编号
//             dic.Add("service", m_service); //交易服务码
//             dic.Add("signType", "RSA"); //签名类型
//             dic.Add("sign", sign); //签名字符串
//             dic.Add("datetime", DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss")); //系统时间
//
//             var uri = new Uri(
// #if DEBUG
//                 "http://121.41.121.164:8044/TransInterface/TransRequest"
// #else
//                 "http://180.96.28.8:8044/TransInterface/TransRequest"
// #endif
//             );
//
//             var res = _HWU.Response(uri, HttpWebUtility.HttpMethod.Post, dic);
//
//             var json = Utils.DynamicJson.Parse(res);
//
//             var pl_res = "";
//             sign = (json.pl_sign as string).Replace(" ", "+"); //替换空格为+符号
//             if (!string.IsNullOrWhiteSpace(sign))
//                 pl_res = Encoding.UTF8.GetString(
//                     BouncyCastleRSAHelper.DecryptByPublicKey(Convert.FromBase64String(sign), this[PublicRSAXml]));
//
//             if (!res.Contains("\"0000\""))
//             {
//                 pt.Message = (json.pl_message as string);
//             }
//             else
//             {
//                 if (pl_res.Contains("&") && pl_res.Contains("="))
//                 {
//                     var data = Core.UnLinkStr(pl_res);
//                     //foreach (var item in pl_res.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
//                     //{
//                     //    var equalsindex = item.IndexOf('=');
//                     //    if (equalsindex > 0)
//                     //        data.Add(item.Substring(0, equalsindex),
//                     //            item.Substring(equalsindex + 1, (item.Length - 1 - equalsindex)));
//                     //}
//
//                     pt.Url = data["pl_url"];
//                     pt.Message = data["pl_orderNum"];
//                     pt.Extra = data["orderNum"];
//
//                     //if (m_qrcode)
//                     //{
//                     //    string imgbase64 = Core.QR(pt.Url, this.GetType());
//                     //    pt.FormHtml = Core.FormQR(imgbase64, p_OrderId, p_Amount, p_OrderName);
//                     //    //pt.Extra = imgbase64;
//                     //}
//                     //else
//                     //{
//                     //    pt.FormHtml = pt.Url;
//                     //}
//                 }
//             }
//
//             return pt;
//         }
//
//         public static readonly Dictionary<EChinaBank, string> CashEBankDic = new Dictionary<EChinaBank, string>
//         {
//             {EChinaBank.ICBC, "工商银行"},
//             {EChinaBank.ABC, "农业银行"},
//             {EChinaBank.CCB, "建设银行"},
//             {EChinaBank.BOC, "中国银行"},
//             {EChinaBank.BCOM, "交通银行"},
//             {EChinaBank.CMB, "招商银行"},
//             {EChinaBank.CIB, "兴业银行"},
//             {EChinaBank.CITIC, "中信银行"},
//             {EChinaBank.SPDB, "浦发银行"},
//             {EChinaBank.CEB, "光大银行"},
//             {EChinaBank.CMBC, "民生银行"},
//         };
//
//         /// <summary>
//         /// 下发
//         /// </summary>
//         /// <param name="p_OrderId"></param>
//         /// <param name="p_EBank"></param>
//         /// <param name="p_ChannelAccount"></param>
//         /// <param name="p_ChannelName"></param>
//         /// <param name="p_Amount"></param>
//         /// <param name="p_Currency"></param>
//         /// <param name="extend_params">额度代付:1  普通代付:2</param>
//         /// <returns></returns>
//         public bool BankTransfer(string p_OrderId, EChinaBank p_EBank, string p_ChannelAccount, string p_ChannelName,
//             double p_Amount, ECurrency p_Currency, dynamic extend_params = null)
//         {
//             if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");
//             if (!CashEBankDic.ContainsKey(p_EBank)) throw new ArgumentException("p_EChannel is not allowed!");
//             if (p_Amount <= 0) throw new ArgumentException("p_Amount must large than 0!");
//             if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");
//             if (extend_params == null) throw new ArgumentNullException("extend_params");
//
//             var dic = new Dictionary<string, string>();
//
//             dic.Add("merchantCode", this[MerchantCode]); //平台商户编号
//             dic.Add("terminalCode", this[TerminalCode]); //平台商户终端编号
//             dic.Add("transDate", DateTime.UtcNow.AddHours(8).ToString("yyyyMMdd")); //交易日期
//             dic.Add("transTime", DateTime.UtcNow.AddHours(8).ToString("HHmmss")); //交易时间
//             dic.Add("transMoney", (p_Amount * 100).ToString("0")); //交易金额，单位分
//             dic.Add("orderNum", p_OrderId); //合作商订单号，全局唯一
//             dic.Add("accountName", p_ChannelName); //收款人账户名
//             dic.Add("bankCard", p_ChannelAccount); //收款人账户号
//             dic.Add("bankName", CashEBankDic[p_EBank]); //收款人账户开户行名称
//             dic.Add("bankLinked",
//                 string.Concat(DateTime.UtcNow.Ticks.ToString().Reverse().Take(6).ToArray())); //收款人账户开户行联行号)
//             //paramlist.Add("transMoney", p_Amount.ToString("0##")); //交易金额
//
//             var signstr = dic.Aggregate("", (x, y) => x + y.Key + "=" + y.Value + "&").TrimEnd('&');
//
//             //var sign = Convert.ToBase64String(m_PublicRSAProvider.PublicEncrypt(Encoding.UTF8.GetBytes(signstr)));
//             var sign = m_PublicRSAProvider.Encrypt(Encoding.UTF8.GetBytes(signstr));
//             sign = sign.Replace(" ", "+"); //替换空格为+符号
//             dic.Add("groupId", this[GroupId]); //合作编号
//             dic.Add("service", (int) extend_params == 1 ? "DF001" : "DF002"); //交易服务码 DF001:额度代付 DF002:普通代付
//             dic.Add("signType", "RSA"); //签名类型
//             dic.Add("sign", sign); //签名字符串
//             dic.Add("datetime", DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss")); //系统时间
//
//             var uri = new Uri(
// #if DEBUG
//                 "http://121.41.121.164:8044/TransInterface/TransRequest"
// #else
//                 "http://180.96.28.8:8044/TransInterface/TransRequest"
// #endif
//             );
//
//             var res = _HWU.Response(uri, HttpWebUtility.HttpMethod.Post, dic);
//
//             if (res.Contains("\"pl_code\":\"0000\""))
//             {
//                 var json = Utils.DynamicJson.Parse(res);
//                 var pl_res = "";
//                 sign = (json.pl_sign as string).Replace(" ", "+"); //替换空格为+符号
//                 if (!string.IsNullOrWhiteSpace(sign))
//                     pl_res = Encoding.UTF8.GetString(
//                         BouncyCastleRSAHelper.DecryptByPublicKey(Convert.FromBase64String(sign), this[PublicRSAXml]));
//                 return pl_res.Contains(p_OrderId);
//             }
//
//             return false;
//         }
//     }
// }