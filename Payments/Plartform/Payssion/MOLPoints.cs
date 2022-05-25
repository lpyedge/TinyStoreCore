using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.MOLPoints)]
    public class MOLPoints : PayBase
    {
        public MOLPoints()
            : base()
        {
            m_pmid = "molpoints";
            Currencies = new List<ECurrency>
            {
                ECurrency.SGD,
                ECurrency.USD
            };
        }

        public MOLPoints(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "molpoints";
            Currencies = new List<ECurrency>
            {
                ECurrency.SGD,
                ECurrency.USD
            };
        }
    }
}