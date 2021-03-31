namespace LPayments.Plartform.iJuHePay
{
    [PayChannel(EChannel.Wechat, ePayType = EPayType.QRcode)]
    public class WeixinQR : _PayBase
    {
        public WeixinQR() : base()
        {
            m_ChannelId = "1000";
        }

        public WeixinQR(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_ChannelId = "1000";
        }
    }
}