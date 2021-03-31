namespace LPayments.Plartform.AliPay
{
    //https://docs.open.alipay.com/270/105900/
    //https://docs.open.alipay.com/270/alipay.trade.page.pay

    [PayChannel(EChannel.AliPay, ePayType = EPayType.App)]
    public class Pay_App : _PayBase
    {
        public Pay_App() : base()
        {
            m_trade_type = "alipay.trade.app.pay";
            m_product_code = "QUICK_MSECURITY_PAY";
        }

        public Pay_App(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }
    }
}