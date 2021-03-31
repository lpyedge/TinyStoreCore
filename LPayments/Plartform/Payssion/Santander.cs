using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.Santander)]
    public class Santander : PayBase
    {
        public Santander()
            : base()
        {
            m_pmid = "santander_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public Santander(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "santander_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}