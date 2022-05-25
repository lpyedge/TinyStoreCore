namespace Payments.Plartform.PaymentWall
{
    [PayChannel( EChannel.CreditCard)]
    public class PaymentWall_CreditCard : PaymentWall
    {
        public PaymentWall_CreditCard()
            : base()
        {
            m_ps = "gateway";
        }

        public PaymentWall_CreditCard(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_ps = "gateway";
        }
    }
}