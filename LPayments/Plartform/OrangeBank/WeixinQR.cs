namespace LPayments.Plartform.OrangeBank
{
    /// <summary>
    /// 平安银行
    /// </summary>
    [PayChannel(EChannel.Wechat, ePayType = EPayType.QRcode)]
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