using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.Boleto)]
    public class Boleto : PayBase
    {
        public Boleto()
            : base()
        {
            m_pmid = "boleto_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public Boleto(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "boleto_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}