//namespace LPayments.Plartform.AliPay
//{
//    [Channel(EPlartform.OpenAliPay, EChannel.AliPay, EPayType.QRcode)]
//    public class AliPay_QR_Wap : IPayment
//    {
//        public const string APPID = "APPID";
//        public const string APPPRIVATEKEY = "APPPRIVATEKEY";
//        public const string SellerId = "SellerId";
//        private RSACryptoServiceProvider m_PrivateProvider => Utils.RSACrypto.FromRSAPrivatePemKey(this[APPPRIVATEKEY]);

//        public AliPay_QR_Wap()
//        {
//            Init();
//        }

//        public AliPay_QR_Wap(string p_SettingsJson)
//        {
//            Init();
//            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//        }

//        protected override void Init()
//        {
//            ProxyEnable = true;

//            Settings = new List<Setting>
//            {
//                new Setting { Name = APPID, Description = "支付宝开放平台的应用ID（APP_ID）", Regex = @"^\d+$", Requied = true},
//                new Setting { Name = APPPRIVATEKEY,Description = "支付宝开放平台的开发者应用私钥（APP_PRIVATE_KEY）",Regex = @"^[\w\W]+$",Requied = true},
//                new Setting { Name = SellerId, Description = "支付宝收款帐号的Seller_Id（Seller_Id）", Regex = @"^\d+$", Requied = false},
//            };

//            Currencies = new List<Currency>
//            {
//                Currency.CNY
//            };
//        }

//        public override NotifyResult Notify()
//        {
//            var res = base.Notify();
//            if (res != null)
//                HttpContext.Current.Response.Write("success");
//            else
//                HttpContext.Current.Response.Write("fail");
//            return res;
//        }

//        public override NotifyResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
//            IDictionary<string, string> head, string body,string notifyip)
//        {
//            var IsVlidate = false;
//            if (form["app_id"] == this[APPID] && (form["trade_status"] == "TRADE_SUCCESS" || form["trade_status"] == "TRADE_FINISHED"))
//            {
//                var signstr =
//                    form.Where(p => p.Key != "sign" && p.Key != "sign_type").OrderBy(p => p.Key).Aggregate("", (x, y) => x + y.Key + "=" + y.Value + "&").TrimEnd('&');

//                var encoding = Encoding.GetEncoding(form["charset"]);

//                IsVlidate = Utils.RSACrypto.FromRSAPrivatePemKey(_Helper.PublicKey).VerifyData(Utils.HASHCrypto.CryptoEnum.SHA256, Convert.FromBase64String(form["sign"]), encoding.GetBytes(signstr));
//            }

//            if (IsVlidate)
//            {
//                var Result = new NotifyResult
//                {
//                    OrderName = form["subject"],
//                    OrderID = form["out_trade_no"],
//                    Amount = form["total_amount"].Parse<double>(),
//                    Tax = form["total_amount"].Parse<double>() - form["receipt_amount"].Parse<double>(),
//                    Currency = Currency.CNY,
//                    Business = form.ContainsKey("seller_id") ? form["seller_id"] : "",
//                    TxnID = form["trade_no"],
//                    PaymentName = Name + (form.ContainsKey("out_channel_inst") ? form["out_channel_inst"] : ""),
//                    PaymentDate = DateTime.UtcNow,
//                    Info = form.ContainsKey("body") ? form["body"] : "",

//                    Message = "success",

//                    Customer_Business = form.ContainsKey("buyer_id") ? form["buyer_id"] : "",
//                };
//                return Result;
//            }
//            return null;
//        }

//        public override PayTicket Pay(string p_OrderId, double p_Amount,
//            Currency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
//            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//        {
//            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

//            if (string.IsNullOrEmpty(this[APPID])) throw new ArgumentNullException(APPID);
//            if (string.IsNullOrEmpty(this[APPPRIVATEKEY])) throw new ArgumentNullException(APPPRIVATEKEY);
//            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("p_Currency is not allowed!");
//            if (p_OrderName.Length > 256) throw new ArgumentException("OrderName must less than 256!");

//            dynamic biz = new ExpandoObject();
//            var dicbiz = (IDictionary<String, Object>)biz;
//            dicbiz[_Helper.TradeWapPay.product_code.ToString()] = "QUICK_WAP_WAY";
//            dicbiz[_Helper.TradeWapPay.out_trade_no.ToString()] = p_OrderId;
//            dicbiz[_Helper.TradeWapPay.subject.ToString()] = p_OrderName;
//            //金额必须保留两位有效数字
//            dicbiz[_Helper.TradeWapPay.total_amount.ToString()] = p_Amount.ToString("0.00");
//            if (!string.IsNullOrWhiteSpace(this[SellerId]))
//                dicbiz[_Helper.TradeWapPay.seller_id.ToString()] = this[SellerId];

//            var dic = _Helper.PublicDic(this[APPID], "alipay.trade.wap.pay", p_NotifyUrl, p_ReturnUrl);

//            dic[_Helper.PublicParams.biz_content.ToString()] = Utils.Json.Serialize(biz);
//            dic[_Helper.PublicParams.sign.ToString()] = Convert.ToBase64String(m_PrivateProvider.SignData(Utils.HASHCrypto.CryptoEnum.SHA256, Encoding.GetEncoding(_Helper.Charset).GetBytes(dic.LinkStr())));

//            var hwu = new HttpWebUtility();
//            hwu.AllowAutoRedirect = true;
//            hwu.Response(new Uri(_Helper.GetWay + "?charset=utf-8"), HttpWebUtility.HttpMethod.POST, dic);
//            var url = hwu.CurrentUri.ToString();

//            var res = hwu.Response(new Uri("http://dwz.cn/create.php"), HttpWebUtility.HttpMethod.POST,
//                new Dictionary<string, string>()
//                {
//                    ["access_type"] = "web",
//                    ["alias"] = "",
//                    ["url"] = url,
//                });

//            var json = Utils.Json.Deserialize<dynamic>(res);
//            var pt = new PayTicket();
//            if (res.Contains("\"status\":0"))
//            {
//                pt.Url = json.tinyurl;
//                var imgbase64 = Core.QR(pt.Url, Core.APIconBase64);
//                pt.FormHtml = Core.FormQR(imgbase64, p_OrderId, p_Amount, p_OrderName);
//                pt.Extra = imgbase64;
//            }
//            return pt;
//        }
//    }
//}