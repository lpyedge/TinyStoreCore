namespace Payments.Plartform.OrangeBank
{
    /// <summary>
    /// 平安银行
    /// </summary>
    [PayChannel(EChannel.WeChat, PayType = EPayType.QRCode)]
    public class WeixinQR : _OrangeBank
    {
        public WeixinQR() : base()
        {
            m_qrcode = true;
#if DEBUG
            m_service = "WeixinBERL";
#else
            m_service = "Weixin";
#endif
        }

        public WeixinQR(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_qrcode = true;
#if DEBUG
            m_service = "WeixinBERL";
#else
            m_service = "Weixin";
#endif
        }
    }
}