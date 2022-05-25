namespace Payments.Plartform.WeChat
{
    [PayChannel(EChannel.WeChat, PayType = EPayType.Other)]
    public class Pay_MP : _PayBase
    {
        public Pay_MP() : base()
        {
            m_tradetype = "JSAPI";
        }

        public Pay_MP(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_tradetype = "JSAPI";
        }

        public class PayExtend
        {
            public string OpenId { get; set; }

            public bool IsFollow { get; set; } = false;
        }
    }
}