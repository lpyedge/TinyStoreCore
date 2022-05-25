using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.Onecard)]
    public class Onecard : PayBase
    {
        public Onecard()
            : base()
        {
            m_pmid = "onecard";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public Onecard(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "onecard";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}