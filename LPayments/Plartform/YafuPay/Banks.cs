// using System;
// using System.Collections.Generic;
// using System.Net;
//
// namespace LPayments.Plartform.YafuPay
// {
//     [PayChannel("雅付", EChannel.ChinaBanks)]
//     public class Banks : _YafuPay
//     {
//         public Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
//         {
//             [EChinaBank.CCB] = "CCB",
//             [EChinaBank.CMB] = "CMB",
//             [EChinaBank.ICBC] = "ICBC",
//             [EChinaBank.BOC] = "BOC",
//             [EChinaBank.ABC] = "ABC",
//             [EChinaBank.BCOM] = "BOCOM",
//             [EChinaBank.CMBC] = "CMBC",
//             [EChinaBank.HXB] = "HXBC",
//             [EChinaBank.CIB] = "CIB",
//             [EChinaBank.SPDB] = "SPDB",
//             [EChinaBank.GDB] = "GDB",
//             [EChinaBank.CITIC] = "CITIC",
//             [EChinaBank.CEB] = "CEB",
//             [EChinaBank.PSBC] = "PSBC",
//             [EChinaBank.SDB] = "SDB",
//             [EChinaBank.BOBJ] = "BOBJ",
//             [EChinaBank.TCCB] = "TCCB",
//             [EChinaBank.BOSH] = "BOS",
//             [EChinaBank.SZPAB] = "PAB",
//             [EChinaBank.NBCB] = "NBCB",
//             [EChinaBank.NJCB] = "NJCB",
//             [EChinaBank.JSB] = "JSB",
//         };
//
//         public Banks() : base()
//         {
//             m_service = "0101";
//         }
//
//         public Banks(string p_SettingsJson) : base(p_SettingsJson)
//         {
//             m_service = "0101";
//         }
//
//         public class PayExtend
//         {
//             public EChinaBank Bank { get; set; }
//         }
//
//         public override  PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (extend_params == null) throw new ArgumentNullException("extend_params");
//             try
//             {
//                 var extend = Utils.Json.Deserialize<PayExtend>(Utils.Json.Serialize(extend_params));
//                 m_bankcode = BankDic[extend.Bank];
//             }
//             catch (Exception ex)
//             {
//                 throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend", "extend_params");
//             }
//             return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl, p_CancelUrl);
//         }
//     }
//
//     [PayChannel("雅付", EChannel.ChinaBanks, ePayType = EPayType.H5)]
//     public class Banks_Wap : Banks
//     {
//         public Banks_Wap() : base()
//         {
//             m_service = "0102";
//         }
//
//         public Banks_Wap(string p_SettingsJson) : base(p_SettingsJson)
//         {
//             m_service = "0102";
//         }      
//     }
// }