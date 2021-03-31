using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.EuropeBankTransfer)]
    public class EuropeBankTransfer : PayBase
    {
        public EuropeBankTransfer()
            : base()
        {
            m_pmid = "banktransfer_eu";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }

        public EuropeBankTransfer(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "banktransfer_eu";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }
    }
}