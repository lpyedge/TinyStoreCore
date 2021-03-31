using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.TrustPay)]
    public class TrustPay : PayBase
    {
        public TrustPay()
            : base()
        {
            m_pmid = "trustpay";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }

        public TrustPay(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "trustpay";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }
    }
}