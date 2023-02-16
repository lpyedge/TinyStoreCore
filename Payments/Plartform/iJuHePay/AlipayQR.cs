namespace Payments.Plartform.iJuHePay
{
    [PayChannel( EChannel.AliPay, PayType = EPayType.QRCode)]
    public class AliPayQR : _PayBase
    {
        public AliPayQR() : base()
        {
            m_ChannelId = "1000";
        }

        public AliPayQR(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_ChannelId = "1000";
        }
    }
}