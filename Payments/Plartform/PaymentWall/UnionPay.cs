namespace Payments.Plartform.PaymentWall
{
    [PayChannel(EChannel.UnionPay)]
    public class PaymentWall_UnionPay : PaymentWall
    {
        public PaymentWall_UnionPay()
            : base()
        {
            m_ps = "unionpay";
        }

        public PaymentWall_UnionPay(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_ps = "unionpay";
        }
    }
}