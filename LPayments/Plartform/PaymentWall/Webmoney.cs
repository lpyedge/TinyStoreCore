namespace LPayments.Plartform.PaymentWall
{
    [PayChannel(EChannel.WebMoney)]
    public class PaymentWall_Webmoney : PaymentWall
    {
        public PaymentWall_Webmoney()
            : base()
        {
            m_ps = "webmoney";
        }

        public PaymentWall_Webmoney(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_ps = "webmoney";
        }
    }
}