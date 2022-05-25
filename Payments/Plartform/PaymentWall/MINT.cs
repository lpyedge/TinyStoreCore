namespace Payments.Plartform.PaymentWall
{
    [PayChannel(EChannel.MINT)]
    public class PaymentWall_MINT : PaymentWall
    {
        public PaymentWall_MINT()
            : base()
        {
            m_ps = "mint";
        }

        public PaymentWall_MINT(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_ps = "mint";
        }
    }
}