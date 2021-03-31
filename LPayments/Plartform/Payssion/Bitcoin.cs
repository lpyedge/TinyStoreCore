using System;
using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.Bitcoin)]
    public class Bitcoin : PayBase
    {
        public Bitcoin()
            : base()
        {
            m_pmid = "bitcoin";
            Currencies = new List<ECurrency>();
            foreach (var name in Enum.GetNames(typeof(ECurrency)))
                Currencies.Add((ECurrency)Enum.Parse(typeof(ECurrency), name, true));
        }

        public Bitcoin(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "bitcoin";
            Currencies = new List<ECurrency>();
            foreach (var name in Enum.GetNames(typeof(ECurrency)))
                Currencies.Add((ECurrency)Enum.Parse(typeof(ECurrency), name, true));
        }
    }
}