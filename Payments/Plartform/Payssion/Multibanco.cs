using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.Poli)]
    public class Multibanco : PayBase
    {
        public Multibanco()
            : base()
        {
            m_pmid = "multibanco_pt";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }

        public Multibanco(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "multibanco_pt";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }
    }
}