﻿using System.Collections.Generic;

namespace Payments.Plartform.WeChat
{
    //https://pay.weixin.qq.com/wiki/doc/api/index.html
    //https://pay.weixin.qq.com/wiki/doc/api/H5.php?chapter=15_4

    public abstract class _WeChat : IPayChannel
    {
        public const string MchId = "MchId";
        public const string Key = "Key";
        public const string AppId = "AppId";
        public const string AppSecret = "AppSecret";

        public const string Domain = "Domain";

        public _WeChat() : base()
        {
            Platform = EPlatform.WeChat;
        }

        public _WeChat(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = MchId, Description = "微信支付的商家帐号MchId", Regex = @"^[\w\.@]+$", Requied = true},
                new Setting {Name = Key, Description = "微信支付的Key", Regex = @"^\w+$", Requied = true},
                new Setting {Name = AppId, Description = "微信公众号的AppId(开发者Id)", Regex = @"^\w+$", Requied = true},
                new Setting
                {
                    Name = AppSecret, Description = "微信公众号的AppSecret(开发者密码)", Regex = @"^\w+$", Requied = true
                },
                new Setting {Name = Domain, Description = "当前支付方式的相关域名", Regex = @"^[\w%&=:/.]+$", Requied = false},
                //new Setting {Name = IsSubscribe, Description = "是否自动关注公众号", Regex = @"^\w+$", Requied = false}
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.CNY
            };
        }
    }
}