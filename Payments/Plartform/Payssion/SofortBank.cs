using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.SofortBank)]
    public class SofortBank : PayBase
    {
        public SofortBank()
            : base()
        {
            m_pmid = "sofort";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR,
                ECurrency.GBP,
                ECurrency.PLN
            };
        }

        public SofortBank(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "sofort";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR,
                ECurrency.GBP,
                ECurrency.PLN
            };
        }
    }
}