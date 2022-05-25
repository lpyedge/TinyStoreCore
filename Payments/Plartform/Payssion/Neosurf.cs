using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.Neosurf)]
    public class Neosurf : PayBase
    {
        public Neosurf()
            : base()
        {
            m_pmid = "neosurf";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }

        public Neosurf(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "neosurf";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }
    }
}