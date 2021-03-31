//using System;
//using System.Collections.Generic;
//using System.Net;

//namespace LPayments.Plartform.RemitMoney
//{
//    [Channel(EPlartform.RemitMoney, EChannel.RemitMoney)]
//    public class RemitMoney : IPayment
//    {
//        private const string Info = "Info";

//        public RemitMoney()
//        {
//            Init();
//        }

//        public RemitMoney(string p_SettingsJson)
//        {
//            Init();
//            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//        }

//        protected override void Init()
//        {
//            Settings = new List<Setting>
//            {
//                new Setting
//                {
//                    Name = Info,
//                    Description = "自定义书写付款信息,以下数据代替订单自动生成的信息"
//                                  + "<br/>[OrderName]:表示订单名称"
//                                  + "<br/>[OrderId]:表示订单ID"
//                                  + "<br/>[Amount]:表示订单支付金额"
//                                  + "<br/>[Currency]:表示订单支付货币种类",
//                    Regex = @"^[\w\W]+$",
//                    Requied = true
//                }
//            };

//            Currencies = new List<Currency>();
//            foreach (var name in Enum.GetNames(typeof(Currency)))
//                Currencies.Add((Currency) Enum.Parse(typeof(Currency), name, true));
//        }

//        public override NotifyResult Notify()
//        {
//            return null;
//        }

//        public override NotifyResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
//            IDictionary<string, string> head, string body,string notifyip)
//        {
//            return null;
//        }

//        public override PayTicket Pay(string p_OrderId, double p_Amount,
//            Currency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
//            string p_NotifyUrl = "",
//            string p_CancelUrl = "", dynamic extend_params = null)
//        {
//            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

//            var pt = new PayTicket();
//            pt.FormHtml = this[Info].Replace("[OrderName]", p_OrderName)
//                .Replace("[OrderId]", p_OrderId)
//                .Replace("[Amount]", p_Amount.ToString("0.##"))
//                .Replace("[Currency]", p_Currency.ToString());
//            return pt;
//        }
//    }
//}