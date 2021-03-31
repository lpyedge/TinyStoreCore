// using System;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Net;
//
// namespace LPayments.Plartform.OkPay365
// {
//     [PayChannel("行付天下", EChannel.ChinaBanks)]
//     public class Banks : _OkPay365
//     {
//         public Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
//         {
//             [EChinaBank.ICBC] = "1020000",
//             [EChinaBank.CCB] = "1050000",
//             [EChinaBank.ABC] = "1030000",
//             [EChinaBank.CMB] = "3080000",
//             [EChinaBank.BCOM] = "3010000",
//             [EChinaBank.BOC] = "1040000",
//             [EChinaBank.CEB] = "3030000",
//             [EChinaBank.CMBC] = "3050000",
//             [EChinaBank.CIB] = "3090000",
//             [EChinaBank.CITIC] = "3020000",
//             [EChinaBank.GDB] = "3060000",
//             [EChinaBank.SPDB] = "3100000",
//             [EChinaBank.SZPAB] = "3070000",
//             [EChinaBank.HXB] = "3040000",
//             [EChinaBank.NBCB] = "4083320",
//             [EChinaBank.BEA] = "3200000",
//             [EChinaBank.BOSH] = "4012900",
//             [EChinaBank.PSBC] = "1000000",
//             [EChinaBank.NJCB] = "4243010",
//             [EChinaBank.SRCB] = "65012900",
//             [EChinaBank.CBHB] = "3170000",
//             [EChinaBank.BOCD] = "64296510",
//             [EChinaBank.BOBJ] = "4031000",
//             [EChinaBank.HSB] = "64296511",
//             [EChinaBank.TCCB] = "4341101",
//         };
//
//         //public Dictionary<EBank, string> BankDic = new Dictionary<EBank, string>()
//         //{
//         //    [EBank.CCB] = "1050000",
//         //    [EBank.CMB] = "3080000",
//         //    [EBank.BOC] = "1040000",
//         //    [EBank.CEB] = "3030000",
//         //    [EBank.CMBC] = "3050000",
//         //};
//
//         public enum EBankName
//         {
//             [Description("01020000")] 工商银行,
//             [Description("01030000")] 农业银行,
//             [Description("01040000")] 中国银行,
//             [Description("01050000")] 建设银行,
//             [Description("03010000")] 交通银行,
//             [Description("03030000")] 光大银行,
//             [Description("03050000")] 民生银行,
//             [Description("03080000")] 招商银行,
//             [Description("03100000")] 浦发银行,
//             [Description("03060000")] 广发银行,
//             [Description("03020000")] 中信银行,
//             [Description("03040000")] 华夏银行,
//             [Description("04083320")] 宁波银行,
//             //[Description("65012900")]
//             //上海农商银行,
//         }
//
//         public Banks() : base()
//         {
//             m_service = "WGZF003";
//             m_qrcode = false;
//         }
//
//         public Banks(string p_SettingsJson) : base(p_SettingsJson)
//         {
//             m_service = "WGZF003";
//             m_qrcode = false;
//         }
//
//         public class PayExtend
//         {
//
//             public PayExtend(string p_BankCard = "")
//             {
//                 BankCard = p_BankCard;
//             }
//             public EChinaBank Bank { get; set; }
//
//             public string BankCard { get; set; }
//         }
//
//         public override PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (extend_params == null) throw new ArgumentNullException("extend_params");
//             try
//             {
//                 var extend = Utils.Json.Deserialize<PayExtend>(Utils.Json.Serialize(extend_params));
//                 m_bankCode = BankDic[extend.Bank];
//                 m_bankCard = extend.BankCard;
//             }
//             catch (Exception ex)
//             {
//                 throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend", "extend_params");
//             }
//             return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl, p_CancelUrl);
//         }
//     }
//
//     [PayChannel("行付天下", EChannel.ChinaBanksCredit)]
//     public class BanksDebit : _OkPay365
//     {
//         public Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
//         {
//             [EChinaBank.ICBC] = "1020000",
//             [EChinaBank.CCB] = "1050000",
//             [EChinaBank.ABC] = "1030000",
//             [EChinaBank.CMB] = "3080000",
//             [EChinaBank.BCOM] = "3010000",
//             [EChinaBank.BOC] = "1040000",
//             [EChinaBank.CEB] = "3030000",
//             [EChinaBank.CMBC] = "3050000",
//             [EChinaBank.CIB] = "3090000",
//             [EChinaBank.CITIC] = "3020000",
//             [EChinaBank.GDB] = "3060000",
//             [EChinaBank.SPDB] = "3100000",
//             [EChinaBank.SZPAB] = "3070000",
//             [EChinaBank.HXB] = "3040000",
//             [EChinaBank.NBCB] = "4083320",
//             [EChinaBank.BEA] = "3200000",
//             [EChinaBank.BOSH] = "4012900",
//             [EChinaBank.PSBC] = "1000000",
//             [EChinaBank.NJCB] = "4243010",
//             [EChinaBank.SRCB] = "65012900",
//             [EChinaBank.CBHB] = "3170000",
//             [EChinaBank.BOCD] = "64296510",
//             [EChinaBank.BOBJ] = "4031000",
//             [EChinaBank.HSB] = "64296511",
//             [EChinaBank.TCCB] = "4341101",
//         };
//
//
//         //public Dictionary<EBank, string> BankDic = new Dictionary<EBank, string>()
//         //{
//         //    [EBank.ABC] = "1030000",
//         //    [EBank.SZPAB] = "3070000",
//         //    [EBank.PSBC] = "1000000",
//         //    [EBank.ICBC] = "1020000",
//         //    [EBank.BOBJ] = "4031000",
//         //};
//         public BanksDebit() : base()
//         {
//             m_service = "WGZF001";
//             m_qrcode = false;
//         }
//
//         public BanksDebit(string p_SettingsJson) : base(p_SettingsJson)
//         {
//             m_service = "WGZF001";
//             m_qrcode = false;
//         }
//
//         public class PayExtend
//         {
//             public PayExtend(string p_BankCard = "")
//             {
//                 BankCard = p_BankCard;
//             }
//             public EChinaBank Bank { get; set; }
//
//             public string BankCard { get; set; }
//         }
//
//         public override PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (extend_params == null) throw new ArgumentNullException("extend_params");
//             try
//             {
//                 var extend = Utils.Json.Deserialize<PayExtend>(Utils.Json.Serialize(extend_params));
//                 m_bankCode = BankDic[extend.Bank];
//                 m_bankCard = extend.BankCard;
//             }
//             catch (Exception ex)
//             {
//                 throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend", "extend_params");
//             }
//             return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl, p_CancelUrl);
//         }
//     }
//
//     [PayChannel("行付天下", EChannel.ChinaBanksCredit)]
//     public class BanksCredit : _OkPay365
//     {
//         public Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
//         {
//             [EChinaBank.ICBC] = "1020000",
//             [EChinaBank.CCB] = "1050000",
//             [EChinaBank.ABC] = "1030000",
//             [EChinaBank.CMB] = "3080000",
//             [EChinaBank.BCOM] = "3010000",
//             [EChinaBank.BOC] = "1040000",
//             [EChinaBank.CEB] = "3030000",
//             [EChinaBank.CMBC] = "3050000",
//             [EChinaBank.CIB] = "3090000",
//             [EChinaBank.CITIC] = "3020000",
//             [EChinaBank.GDB] = "3060000",
//             [EChinaBank.SPDB] = "3100000",
//             [EChinaBank.SZPAB] = "3070000",
//             [EChinaBank.HXB] = "3040000",
//             [EChinaBank.NBCB] = "4083320",
//             [EChinaBank.BEA] = "3200000",
//             [EChinaBank.BOSH] = "4012900",
//             [EChinaBank.PSBC] = "1000000",
//             [EChinaBank.NJCB] = "4243010",
//             [EChinaBank.SRCB] = "65012900",
//             [EChinaBank.CBHB] = "3170000",
//             [EChinaBank.BOCD] = "64296510",
//             [EChinaBank.BOBJ] = "4031000",
//             [EChinaBank.HSB] = "64296511",
//             [EChinaBank.TCCB] = "4341101",
//         };
//
//         public BanksCredit() : base()
//         {
//             m_service = "WGZF002";
//             m_qrcode = false;
//         }
//
//         public BanksCredit(string p_SettingsJson) : base(p_SettingsJson)
//         {
//             m_service = "WGZF002";
//             m_qrcode = false;
//         }
//
//         public class PayExtend
//         {
//             public PayExtend(string p_BankCard = "")
//             {
//                 BankCard = p_BankCard;
//             }
//             public EChinaBank Bank { get; set; }
//
//             public string BankCard { get; set; }
//         }
//
//         public override PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (extend_params == null) throw new ArgumentNullException("extend_params");
//             try
//             {
//                 var extend = Utils.Json.Deserialize<PayExtend>(Utils.Json.Serialize(extend_params));
//                 m_bankCode = BankDic[extend.Bank];
//                 m_bankCard = extend.BankCard;
//             }
//             catch (Exception ex)
//             {
//                 throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend", "extend_params");
//             }
//             return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl, p_CancelUrl);
//         }
//     }
// }