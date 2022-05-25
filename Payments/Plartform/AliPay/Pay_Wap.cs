namespace Payments.Plartform.AliPay
{
    [PayChannel(EChannel.AliPay, PayType = EPayType.H5)]
    public class Pay_Wap : _PayBase
    {
        public Pay_Wap()
        {
            m_trade_type = "alipay.trade.wap.pay";
            m_product_code = "QUICK_WAP_WAY";
            Init();
        }

        public Pay_Wap(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        public class Extend
        {
            public Extend()
            {
            }

            /// <summary>
            /// 请求方法
            /// QR,Android,IOS
            /// </summary>
            public string Method { get; set; }
        }
    }
}