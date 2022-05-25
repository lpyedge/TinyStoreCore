using System;
using System.ComponentModel;

namespace Payments
{
    /// <summary>
    /// 支付平台
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PlatformAttribute : Attribute
    {
        public PlatformAttribute(string name, string memo = "")
        {
            Name = name;
            Memo = memo;
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
        public PayChannelAttribute(EChannel eChannel, EPayType ePayType = EPayType.PC)
        {
            Channel = eChannel;
            PayType = ePayType;
        }
        
        public EChannel Channel { get; set; }
        public EPayType PayType { get; set; }

        public string ChannelDesc => Utils.Core.EnumAttribute<DescriptionAttribute>(Channel).Description;

        public string PayTypeDesc => Utils.Core.EnumAttribute<DescriptionAttribute>(PayType).Description;

    }
}