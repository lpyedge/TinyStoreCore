using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.iJuHePay
{
    //测试商户号: 1635  测试密钥:pSHPKEKeYpR8UwKzG2mB6tphO3AEtwSy

    //安全码(密钥):1772   测试密钥:n0Ca6f0kgYjzrtwjk2jYDBzba2dlqvnV

    
    [PayPlatformAttribute("聚合支付", "", SiteUrl = "http://www.ijuhepay.com")]
    public abstract class _iJuHePay : IPayChannel
    {
        public const string MerchantId = "MerchantId";
        public const string SecretKey = "SecretKey";
        public const string PayUrl = "PayUrl";

        public _iJuHePay():base()
        {
        }

        public _iJuHePay(string p_SettingsJson):this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = MerchantId, Description = "i聚合平台的商户ID", Regex = @"^\d+$", Requied = true},
                new Setting {Name = SecretKey, Description = "i聚合平台的安全码(密钥)", Regex = @"^\w+$", Requied = true},
                new Setting {Name = PayUrl, Description = "最终支付网址", Regex = @"^[\w\W]+$", Requied = true}
            };

            Currencies = new List<ECurrency> {ECurrency.CNY,};
        }
    }
}