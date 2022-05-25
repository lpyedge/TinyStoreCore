using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.P24)]
    public class P24 : PayBase
    {
        public P24()
            : base()
        {
            m_pmid = "p24_pl";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }

        public P24(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "p24_pl";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }
    }
}