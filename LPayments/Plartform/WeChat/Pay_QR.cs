namespace LPayments.Plartform.WeChat
{
    [PayChannel(EChannel.WeChat, ePayType = EPayType.QRcode)]
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