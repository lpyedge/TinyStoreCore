using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.Dotpay)]
    public class Dotpay : PayBase
    {
        public Dotpay()
            : base()
        {
            m_pmid = "giropay_de";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }

        public Dotpay(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "giropay_de";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }
    }
}