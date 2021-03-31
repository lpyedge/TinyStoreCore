using System;
using System.ComponentModel;

namespace LPayments
{
    /// <summary>
    /// 支付平台
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PayPlatformAttribute : Attribute
    {
        public PayPlatformAttribute(string p_name, string p_memo = "")
        {
            Name = p_name;
            Memo = p_memo;
        }

        public string Name { get; set; }


        public string Memo { get; set; }


        public string SiteUrl { get; set; } = "";

        public bool NotifyProxy { get; set; } = false;
    }

    /// <summary>
    /// 支付通道及方式
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PayChannelAttribute : Attribute
    {
        public PayChannelAttribute(EChannel p_EChannel, EPayType p_EPayType = EPayType.PC)
        {
            eChannel = p_EChannel;
            ePayType = EPayType.PC;
        }

        // public EPlartform ePlartform { get; set; }
        public EChannel eChannel { get; set; }
        public EPayType ePayType { get; set; }


        public string Channel => Utils.Core.Description(eChannel);

        public string PayType => Utils.Core.Description(ePayType);
    }
}