using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.Bancodobrasil)]
    public class Bancodobrasil : PayBase
    {
        public Bancodobrasil()
            : base()
        {
            m_pmid = "bancodobrasil_br";

            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public Bancodobrasil(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "bancodobrasil_br";

            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}