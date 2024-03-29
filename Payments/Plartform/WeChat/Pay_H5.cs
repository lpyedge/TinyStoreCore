﻿namespace Payments.Plartform.WeChat
{
    [PayChannel(EChannel.WeChat, PayType = EPayType.H5)]
    public class Pay_H5 : _PayBase
    {
        public Pay_H5() : base()
        {
            m_tradetype = "MWEB";
        }

        public Pay_H5(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_tradetype = "MWEB";
        }

        public class PayExtend
        {
            public string Domain { get; set; }
            
            public bool IsFollow { get; set; } = false;
        }
    }
}