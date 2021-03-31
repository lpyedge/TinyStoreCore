// using System;
// using System.Collections.Generic;
// using System.Net;
//
// namespace LPayments.Plartform.PayWorth
// {
//     [PayChannel("华势在线", EChannel.ChinaBanks)]
//     public class Banks : _PayWorth
//     {
//         public Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
//         {
//             [EChinaBank.CMB] = "CMB",
//             [EChinaBank.ICBC] = "ICBC",
//             [EChinaBank.CCB] = "CCB",
//             [EChinaBank.BOC] = "BOC",
//             [EChinaBank.ABC] = "ABC",
//             [EChinaBank.BCOM] = "BCOM",
//             [EChinaBank.SPDB] = "SPDB",
//             [EChinaBank.GDB] = "CGB",
//             [EChinaBank.CITIC] = "CITIC",
//             [EChinaBank.CEB] = "CEB",
//             [EChinaBank.CIB] = "CIB",
//             [EChinaBank.SZPAB] = "PAYH",
//             [EChinaBank.CMBC] = "CMBC",
//             [EChinaBank.HXB] = "HXB",
//             [EChinaBank.PSBC] = "PSBC",
//             [EChinaBank.BOBJ] = "BCCB",
//             [EChinaBank.BOSH] = "SHBANK",
//
//             [EChinaBank.WXPAY] = "WXPAY",
//             [EChinaBank.ALIPAY] = "ALIPAY",
//         };
//
//         public Banks() : base()
//         {
//             m_paymethod = "bankPay";
//             m_defaultbank = "";
//         }
//
//         public Banks(string p_SettingsJson) : base(p_SettingsJson)
//         {
//             m_paymethod = "bankPay";
//             m_defaultbank = "";
//         }
//         public class PayExtend
//         {
//             public EChinaBank Bank { get; set; }
//         }
//
//         public override PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (extend_params != null)
//             {
//                 try
//                 {
//                     var extend = Utils.Json.Deserialize<PayExtend>(Utils.Json.Serialize(extend_params));
//                     m_defaultbank = BankDic[extend.Bank];
//                     m_paymethod = "directPay";
//                 }
//                 catch (Exception ex)
//                 {
//                     throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend", "extend_params");
//                 }
//             }
//             return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl, p_CancelUrl);
//         }
//     }
// }