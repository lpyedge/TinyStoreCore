// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Text;
// using System.Web;
//
// namespace Payments.Plartform.YeePay
// {
//     [PayChannel("易宝支付", EChannel.ChinaBanks)]
//     public class Banks : IPayChannel
//     {
//         public const string MerId = "MerId";
//         public const string Key = "Key";
//
//         protected const string GATEWAY = "https://www.yeepay.com/app-merchant-proxy/node";
//         protected string m_bankCode = "";
//
//         public Banks()
//         {
//             Init();
//         }
//
//         public Banks(string p_SettingsJson)
//         {
//             Init();
//             if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//         }
//
//         protected override void Init()
//         {
//             Settings = new List<Setting>
//             {
//                 new Setting {Name = MerId, Description = "易宝支付的商户编号", Regex = @"^\w+$", Requied = true},
//                 new Setting {Name = Key, Description = "易宝支付的商户密钥", Regex = @"^[\w\-\.\\:;&+=/<>]+$", Requied = true},
//             };
//
//             Currencies = new List<ECurrency>
//             {
//                 ECurrency.CNY,
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
//             PayResult result = new PayResult
//             {
//                 Status = PayResult.EStatus.Pending,
//                 Message = "fail"
//             };
//
//             var dic = query.Count > 0 ? query : form;
//
//             if (dic.ContainsKey("p1_MerId")
//                 && dic.ContainsKey("r0_Cmd")
//                 && dic.ContainsKey("r1_Code")
//                 && dic.ContainsKey("r9_BType")
//             )
//             {
//                 //生成hamc的请求参数
//                 string[] list_tohmac =
//                 {
//                     "p1_MerId", "r0_Cmd", "r1_Code", "r2_TrxId", "r3_Amt", "r4_Cur", "r5_Pid", "r6_Order", "r7_Uid",
//                     "r8_MP", "r9_BType"
//                 };
//                 //生成hmac_safe的请求参数
//                 string[] list_tohmac_safe =
//                 {
//                     "p1_MerId", "r0_Cmd", "r1_Code", "r2_TrxId", "r3_Amt", "r4_Cur", "r5_Pid", "r6_Order", "r7_Uid",
//                     "r8_MP", "r9_BType"
//                 };
//
//                 var signstr = "";
//                 foreach (var item in list_tohmac_safe)
//                 {
//                     if (dic.ContainsKey(item) && !string.IsNullOrWhiteSpace(dic[item]))
//                         signstr += dic[item] + "#";
//                 }
//
//                 signstr = signstr.TrimEnd('#');
//
//                 var provider = Utils.HASHCrypto.Generate(Utils.HASHCrypto.CryptoEnum.MD5, this[Key]);
//                 var hash = Utils.HASHCrypto.Encrypt(provider, signstr);
//
//                 if (dic["p1_MerId"] == this[MerId]
//                     && string.Equals(dic["r0_Cmd"], "Buy", StringComparison.OrdinalIgnoreCase)
//                     && string.Equals(dic["r1_Code"], "1", StringComparison.OrdinalIgnoreCase)
//                     && string.Equals(dic["hmac_safe"], hash, StringComparison.OrdinalIgnoreCase)
//                 )
//                 {
//                     result = new PayResult
//                     {
//                         OrderName = HttpUtility.UrlDecode(dic["r5_Pid"]),
//                         OrderID = dic["r6_Order"],
//                         Amount = double.Parse(dic["r3_Amt"]),
//                         Tax = -1,
//                         Currency = ECurrency.CNY,
//                         Business = this[MerId],
//                         TxnID = dic["r2_TrxId"],
//                         PaymentName = Name,
//                         PaymentDate = DateTime.UtcNow,
//
//                         Message = "SUCCESS",
//
//                         Customer = new PayResult._Customer
//                         {
//                             Business = dic["r7_Uid"],
//                         }
//                     };
//                 }
//             }
//
//             return result;
//         }
//
//         public override PayTicket Pay(string p_OrderId, double p_Amount,
//             ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
//             string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");
//
//             if (string.IsNullOrEmpty(this[MerId])) throw new ArgumentNullException("MerId");
//             if (string.IsNullOrEmpty(this[Key])) throw new ArgumentNullException("Key");
//             if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");
//
//             if (extend_params != null)
//             {
//                 try
//                 {
//                     var extend =
//                         Utils.JsonUtility.Deserialize<PayExtend>(
//                             Utils.JsonUtility.Serialize(extend_params));
//                     m_bankCode = BankDic[extend.Bank];
//                 }
//                 catch (Exception ex)
//                 {
//                     throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend",
//                         "extend_params");
//                 }
//             }
//
//             var dic = new Dictionary<string, string>
//             {
//                 ["p0_Cmd"] = "Buy",
//                 ["p1_MerId"] = this[MerId],
//                 ["p2_Order"] = p_OrderId,
//                 ["p3_Amt"] = p_Amount.ToString("0.##"),
//                 ["p4_Cur"] = "CNY",
//                 ["p5_Pid"] = HttpUtility.UrlEncode(p_OrderName),
//                 ["p8_Url"] = p_NotifyUrl,
//                 //["pb_ServerNotifyUrl"] = p_NotifyUrl,
//                 //["pr_NeedResponse"] = "1",
//             };
//
//             if (!string.IsNullOrEmpty(m_bankCode))
//                 dic["pd_FrpId"] = m_bankCode;
//
//             //需要生成签名的字段
//             string[] list_tohmac =
//             {
//                 "p0_Cmd", "p1_MerId", "p2_Order", "p3_Amt", "p4_Cur", "p5_Pid", "p6_Pcat", "p7_Pdesc", "p8_Url",
//                 "p9_SAF", "pa_MP", "pd_FrpId", "pm_Period", "pn_Unit", "pr_NeedResponse", "pt_UserName",
//                 "pt_PostalCode", "pt_Address", "pt_TeleNo", "pt_Mobile", "pt_Email", "pt_LeaveMessage"
//             };
//             //所有请求接口数据字段名
//             string[] list =
//             {
//                 "p0_Cmd", "p1_MerId", "p2_Order", "p3_Amt", "p4_Cur", "p5_Pid", "p6_Pcat", "p7_Pdesc", "p8_Url",
//                 "p9_SAF", "pa_MP", "pd_FrpId", "pn_Unit", "pm_Period", "pr_NeedResponse", "pt_UserName",
//                 "pt_PostalCode", "pt_Address", "pt_TeleNo", "pt_Mobile", "pt_Email", "pt_LeaveMessage", "hmac"
//             };
//             var signstr = list_tohmac.Aggregate("", (s, k) => s += dic.ContainsKey(k) ? dic[k] : "");
//
//             var provider = Utils.HASHCrypto.Generate(Utils.HASHCrypto.CryptoEnum.MD5, this[Key]);
//             dic["hmac"] = Utils.HASHCrypto.Encrypt(provider, signstr);
//
//             //accept-charset='gb2312' onsubmit='document.charset=\"gb2312\";'
//             var formhtml = new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
//                                              "' action='" + GATEWAY + "' method='get' >");
//             foreach (var item in dic)
//             {
//                 if (list.Contains(item.Key))
//                     formhtml.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", item.Key, item.Value);
//             }
//
//             formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
//             formhtml.Append("</form>");
//
//             //var payuri = GATEWAY+"?"+ dic.Aggregate("", (s, kv) => s += kv.Key + "=" + HttpUtility.UrlEncode(kv.Value, Encoding.GetEncoding("GBK")) + "&").TrimEnd('&');
//             //formhtml = new StringBuilder("<script>location.href='" + payuri + "';</script>");
//
//             var pt = new PayTicket();
//             pt.FormHtml = formhtml.ToString();
//             return pt;
//         }
//
//         public class PayExtend
//         {
//             public EChinaBank Bank { get; set; }
//         }
//
//         public Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
//         {
//             [EChinaBank.ICBC] = "ICBC-NET-B2C",
//             [EChinaBank.CMB] = "CMBCHINA-NET-B2C",
//             [EChinaBank.CCB] = "CCB-NET-B2C",
//             [EChinaBank.BCOM] = "BOCO-NET-B2C",
//             [EChinaBank.CIB] = "CIB-NET-B2C",
//             [EChinaBank.CMBC] = "CMBC-NET-B2C",
//             [EChinaBank.CEB] = "CEB-NET-B2C",
//             [EChinaBank.BOC] = "BOC-NET-B2C",
//             [EChinaBank.SZPAB] = "PINGANBANK-NET-B2C",
//             [EChinaBank.CITIC] = "ECITIC-NET-B2C",
//             [EChinaBank.SDB] = "SDB-NET-B2C",
//             [EChinaBank.GDB] = "GDB-NET-B2C",
//             [EChinaBank.BOSH] = "SHB-NET-B2C",
//             [EChinaBank.SPDB] = "SPDB-NET-B2C",
//             [EChinaBank.HXB] = "HXB-NET-B2C",
//             [EChinaBank.BOBJ] = "BCCB-NET-B2C",
//             [EChinaBank.ABC] = "ABC-NET-B2C",
//             [EChinaBank.PSBC] = "POST-NET-B2C",
//             [EChinaBank.BRCB] = "BJRCB-NET-B2C",
//         };
//     }
// }