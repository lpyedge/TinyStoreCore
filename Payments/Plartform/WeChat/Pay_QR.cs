namespace Payments.Plartform.WeChat
{
    [PayChannel(EChannel.WeChat, PayType = EPayType.QRCode)]
    public class Pay_QR : _PayBase
    {
        public Pay_QR() : base()
        {
            m_tradetype = "NATIVE";
        }

        public Pay_QR(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_tradetype = "NATIVE";
        }
        
        public class PayExtend
        {
            public bool IsFollow { get; set; } = false;
        }
    }
}