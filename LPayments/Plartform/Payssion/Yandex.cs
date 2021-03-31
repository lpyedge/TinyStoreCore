using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.Yandex)]
    public class Yandex : PayBase
    {
        public Yandex()
            : base()
        {
            m_pmid = "yandex";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.EUR,
                ECurrency.RUB
            };
        }

        public Yandex(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "yandex";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.EUR,
                ECurrency.RUB
            };
        }
    }
}