namespace Payments.Plartform.OrangeBank
{
    /// <summary>
    /// 平安银行
    /// </summary>
    [PayChannel(EChannel.AliPay, PayType = EPayType.QRcode)]
    public class AliPayQR : _OrangeBank
    {
        public AliPayQR() : base()
        {
            m_qrcode = true;
#if DEBUG
            m_service = "AlipayCS";
#else
            m_service = "AlipayPAZH";
#endif
        }

        public AliPayQR(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_qrcode = true;
#if DEBUG
            m_service = "AlipayCS";
#else
            m_service = "AlipayPAZH";
#endif
        }
    }
}