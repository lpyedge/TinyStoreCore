using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.Cashu)]
    public class Cashu : PayBase
    {
        public Cashu()
            : base()
        {
            m_pmid = "cashu";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public Cashu(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "cashu";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}