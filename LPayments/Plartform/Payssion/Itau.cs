using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.Itau)]
    public class Itau : PayBase
    {
        public Itau()
            : base()
        {
            m_pmid = "itau_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public Itau(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "itau_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}