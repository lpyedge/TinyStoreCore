using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.Giropay)]
    public class Giropay : PayBase
    {
        public Giropay()
            : base()
        {
            m_pmid = "giropay_de";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }

        public Giropay(string p_SettingsJson)
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