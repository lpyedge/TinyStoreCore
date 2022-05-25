

namespace Payments.Plartform.WeChat
{
    [PayChannel(EChannel.WeChat, PayType = EPayType.App)]
    public class Pay_App : _PayBase
    {
        public Pay_App() : base()
        {
            m_tradetype = "APP";
        }

        public Pay_App(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_tradetype = "APP";
        }

      
        public class PayExtend
        {
            public bool IsFollow { get; set; } = false;
        }
    }
}