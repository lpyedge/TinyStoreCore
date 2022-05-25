// namespace Payments.Plartform.OkPay365
// {
//     [PayChannel("行付天下", EChannel.Wechat, ePayType = EPayType.QRcode)]
//     public class WeixinQR : _OkPay365
//     {
//         //protected const string CreditCard = "CreditCard";
//
//         public WeixinQR() : base()
//         {
//             m_service = "SMZF004";
//             m_qrcode = true;
//         }
//
//         public WeixinQR(string p_SettingsJson) : base(p_SettingsJson)
//         {
//             //Settings.Add(new Setting { Name = CreditCard, Description = "支持信用卡(1 是,0 否)", Regex = @"^(1|0)$", Requied = true });
//             m_service = "SMZF004";
//             m_qrcode = true;
//         }
//     }
// }