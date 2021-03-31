namespace LPayments.Plartform.PaymentWall
{
    [PayChannel( EChannel.AliPay)]
    public class PaymentWall_AliPay : PaymentWall
    {
        public PaymentWall_AliPay()
            : base()
        {
            m_ps = "alipay";
        }

        public PaymentWall_AliPay(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_ps = "alipay";
        }
    }
}