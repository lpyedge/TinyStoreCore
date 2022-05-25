using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.Dineromail)]
    public class Dineromail : PayBase
    {
        public Dineromail()
            : base()
        {
            m_pmid = "dineromail_ar";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public Dineromail(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "dineromail_ar";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}