using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.Caixa)]
    public class Caixa : PayBase
    {
        public Caixa()
            : base()
        {
            m_pmid = "caixa_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public Caixa(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "caixa_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}