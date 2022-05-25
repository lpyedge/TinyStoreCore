
namespace Payments.Plartform.AliPay
{
    [PayChannel(EChannel.AliPay, PayType = EPayType.QRcode)]
    public class Pay_QR : _PayBase
    {
        public Pay_QR()
        {
            m_trade_type = "alipay.trade.precreate";
            m_product_code = null;
            Init();
        }

        public Pay_QR(string p_SettingsJson):this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }
    }
}