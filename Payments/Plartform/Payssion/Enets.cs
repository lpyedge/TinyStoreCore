using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.Enets)]
    public class Enets : PayBase
    {
        public Enets()
            : base()
        {
            m_pmid = "molpay_sg_enets";
            Currencies = new List<ECurrency>
            {
                ECurrency.SGD
            };
        }

        public Enets(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "molpay_sg_enets";
            Currencies = new List<ECurrency>
            {
                ECurrency.SGD
            };
        }
    }
}