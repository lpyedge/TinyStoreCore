using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.EPS)]
    public class EPS : PayBase
    {
        public EPS()
            : base()
        {
            m_pmid = "eps_at";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }

        public EPS(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "eps_at";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }
    }
}