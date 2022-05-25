using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.MOLPay)]
    public class MOLPay : PayBase
    {
        public MOLPay()
            : base()
        {
            m_pmid = "molpay";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public MOLPay(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "molpay";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}