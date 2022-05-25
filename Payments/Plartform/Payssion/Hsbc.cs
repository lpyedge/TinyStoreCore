using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.Hsbc)]
    public class Hsbc : PayBase
    {
        public Hsbc()
            : base()
        {
            m_pmid = "hsbc_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public Hsbc(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "hsbc_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}