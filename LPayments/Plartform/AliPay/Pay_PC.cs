namespace LPayments.Plartform.AliPay
{
    [PayChannel(EChannel.AliPay)]
    public class Pay_PC : _PayBase
    {
        public Pay_PC()
        {
            m_trade_type = "alipay.trade.page.pay";
            m_product_code = "FAST_INSTANT_TRADE_PAY";
            Init();
        }

        public Pay_PC(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }
    }
}