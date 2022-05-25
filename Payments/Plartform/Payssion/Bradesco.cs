using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.Bradesco)]
    public class Bradesco : PayBase
    {
        public Bradesco()
            : base()
        {
            m_pmid = "bradesco_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public Bradesco(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "bradesco_br";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}