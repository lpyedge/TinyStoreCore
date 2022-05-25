using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.QiWi)]
    public class QiWi : PayBase
    {
        public QiWi()
            : base()
        {
            m_pmid = "qiwi";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.EUR,
                ECurrency.RUB
            };
        }

        public QiWi(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "qiwi";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.EUR,
                ECurrency.RUB
            };
        }
    }
}