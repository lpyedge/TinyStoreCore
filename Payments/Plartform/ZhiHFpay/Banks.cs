// using System;
// using System.Collections.Generic;
// using System.Net;
//
// namespace Payments.Plartform.ZhiHFpay
// {
//     [PayChannel("智汇付", EChannel.ChinaBanks)]
//     public class Banks : _ZhiHFpay
//     {
//         public Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
//         {
//             [EChinaBank.ABC] = "ABC",
//             [EChinaBank.ICBC] = "ICBC",
//             [EChinaBank.CCB] = "CCB",
//             [EChinaBank.BCOM] = "BCOM",
//             [EChinaBank.BOC] = "BOC",
//             [EChinaBank.CMB] = "CMB",
//             [EChinaBank.CMBC] = "CMBC",
//             [EChinaBank.CEB] = "CEBB",
//             [EChinaBank.BOBJ] = "BOB",
//             [EChinaBank.BOSH] = "SHB",
//             [EChinaBank.NBCB] = "NBB",
//             [EChinaBank.HXB] = "HXB",
//             [EChinaBank.CIB] = "CIB",
//             [EChinaBank.PSBC] = "PSBC",
//             [EChinaBank.SZPAB] = "SPABANK",
//             [EChinaBank.SPDB] = "SPDB",
//             [EChinaBank.CITIC] = "ECITIC",
//             [EChinaBank.HZCB] = "HZB",
//             [EChinaBank.GDB] = "GDB",
//         };
//
//         public Banks() : base()
//         {
//             m_payType = "b2c";
//             m_bankCode = "";
//         }
//
//         public Banks(string p_SettingsJson) : base(p_SettingsJson)
//         {
//             m_payType = "b2c";
//             m_bankCode = "";
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
//                     var extend = Utils.JsonUtility.Deserialize<PayExtend>(Utils.JsonUtility.Serialize(extend_params));
//                     m_bankCode = BankDic[extend.Bank];
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