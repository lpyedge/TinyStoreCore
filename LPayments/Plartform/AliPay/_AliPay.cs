﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using LPayments.Utils;

namespace LPayments.Plartform.AliPay
{
    [PayPlatformAttribute("蚂蚁金服", "支付宝开放平台", SiteUrl = "https://openhome.alipay.com/docCenter/docCenter.htm",
        NotifyProxy = true)]
    public abstract class _AliPay : IPayChannel
    {
        protected const string GateWay = "https://openapi.alipay.com/gateway.do";
        protected const string Charset = "utf-8";

        public const string APPID = "APPID";
        public const string APPPRIVATEKEY = "APPPRIVATEKEY";
        public const string AliPayPublicKey = "AliPayPublicKey";

        //"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAs4/QwZtO8EJU+SjPZn4B3jxD8NmOXk63j8995FkbSTtGP2rN1XRcU6dQ8pRS9qgY0RRc1t/mTHWZwCtJIwxGKQm1v2hyKFoJW+edCKd9LP60e39xQup7hjGrRUrUfpMKlxk7c2lh1IfhEjmaySSR+wwgDZGP9cD05NfjUCVxtgAbmN4aIxlxRoWpX3ykRp7F11SB6bsuedAfz4u0jdU3iIZ08Dyg4V5ik/9h3zaNlD2M5l11gmolqSeJNN9/TlJzu+1bp3soCRnNLhoJ81+7kNOJYs9T4xe58y4ihxkqahDS7amrDgLohkfo2vDgR0okCetTRQ6YdnQyFiRkioRAVwIDAQAB";
        //protected const string PublicKey = "<RSAKeyValue><Modulus>s4/QwZtO8EJU+SjPZn4B3jxD8NmOXk63j8995FkbSTtGP2rN1XRcU6dQ8pRS9qgY0RRc1t/mTHWZwCtJIwxGKQm1v2hyKFoJW+edCKd9LP60e39xQup7hjGrRUrUfpMKlxk7c2lh1IfhEjmaySSR+wwgDZGP9cD05NfjUCVxtgAbmN4aIxlxRoWpX3ykRp7F11SB6bsuedAfz4u0jdU3iIZ08Dyg4V5ik/9h3zaNlD2M5l11gmolqSeJNN9/TlJzu+1bp3soCRnNLhoJ81+7kNOJYs9T4xe58y4ihxkqahDS7amrDgLohkfo2vDgR0okCetTRQ6YdnQyFiRkioRAVw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";


        protected RSACryptoServiceProvider m_AppPrivateProvider => this[APPPRIVATEKEY].StartsWith("<RSAKeyValue>")
            ? Utils.RSACrypto.FromXmlKey(this[APPPRIVATEKEY])
            : Utils.RSACrypto.FromPEM(this[APPPRIVATEKEY]);

        protected RSACryptoServiceProvider m_AlipayPublicProvider => Utils.RSACrypto.FromXmlKey(this[AliPayPublicKey]);

        public _AliPay() : base()
        {
        }

        public _AliPay(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = APPID, Description = "支付宝开放平台的应用ID（APP_ID）", Regex = @"^\d+$", Requied = true},
                new Setting
                {
                    Name = APPPRIVATEKEY, Description = "支付宝开放平台的开发者应用SHA256私钥（APP_PRIVATE_KEY）",
                    Regex = @"^[\s\w+/=<>-]+$", Requied = true
                },
                new Setting
                {
                    Name = AliPayPublicKey, Description = "支付宝开放平台的支付宝SHA256私钥（Alipay_PUBLIC_KEY）",
                    Regex = @"^[\s\w+/=<>-]+$", Requied = false
                }
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.CNY
            };
        }

        internal Dictionary<string, string> PublicDic(string method, string notify_url = "", string return_url = "",
            string app_auth_token = "")
        {
            var dic = new Dictionary<string, string>();

            dic["app_id"] = this[APPID];
            dic["charset"] = Charset;
            dic["format"] = "json";
            dic["method"] = method;
            dic["version"] = "1.0";
            dic["timestamp"] = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
            dic["sign_type"] = "RSA2";
            if (method == "alipay.trade.app.pay" || method == "alipay.trade.precreate" ||
                method == "alipay.trade.wap.pay" || method == "alipay.trade.page.pay")
                dic["notify_url"] = notify_url;
            if (method == "alipay.trade.wap.pay" || method == "alipay.trade.page.pay")
                dic["return_url"] = return_url;
            if (!string.IsNullOrWhiteSpace(app_auth_token) && (method == "alipay.trade.precreate" ||
                                                               method == "alipay.fund.trans.toaccount.transfer"))
                dic["app_auth_token"] = app_auth_token;
            return dic;
        }
    }
}