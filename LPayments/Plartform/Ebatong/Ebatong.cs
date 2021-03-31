//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Web;

//namespace LPayments.Plartform
//{
//    [Channel(EChannel.Alipay, EPlartform.Ebatong)]
//    public class Ebatong : IPayment
//    {
//        private const string PID = "PID";
//        private const string Key = "Key";
//        private const string CashKey = "CashKey";

//        private const string GATEWAY = "https://www.ebatong.com/direct/gateway.htm";

//        private static readonly Regex TimestampRegex = new Regex(@"<encrypt_key>([\w=]+)</encrypt_key>",
//            RegexOptions.Compiled | RegexOptions.IgnoreCase);

//        public Ebatong()
//        {
//            Init();
//        }

//        public Ebatong(string p_SettingsJson)
//        {
//            Init();
//            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//        }

//        protected override void Init()
//        {
//            Settings = new List<Setting>
//            {
//                new Setting {Name = PID, Description = "维品支付(易八通)的商户号(PID)", Regex = @"^\d+$", Requied = true},
//                new Setting {Name = Key, Description = "维品支付(易八通)的安全密钥(Key)", Regex = @"^\w+$", Requied = true},
//                new Setting
//                {
//                    Name = CashKey,
//                    Description = "维品支付(易八通)的代发密钥(CashKey)代发时使用",
//                    Regex = @"^\w+$",
//                    Requied = false
//                }
//            };

//            Currencies = new List<Currency>
//            {
//                Currency.CNY
//            };
//        }

//        #region Cash 代发

//        //public NotifyResult CashCheck(SortedDictionary<string, string> dictionary)
//        //{
//        //    bool IsVlidate = false;
//        //    if (dictionary["sign"] != null && dictionary["partner"] == this[PID])
//        //    {
//        //        IsVlidate = string.Equals(Utils.HASHCrypto.Generate( Utils.HASHCrypto.CryptoEnum.MD5).Encrypt(dictionary.Where(p => p.Key != "sign").Aggregate("", (c, p) => c + p.Key + "=" + p.Value + "&").TrimEnd('&') + this[CashKey]), dictionary["sign"], StringComparison.OrdinalIgnoreCase); ;
//        //    }

//        //    if (IsVlidate)
//        //    {
//        //        NotifyResult Result = null;
//        //        if ((dictionary["trade_status"] == "2" | dictionary["trade_status"] == "3" |
//        //             dictionary["trade_status"] == "4"))
//        //        {
//        //            Result = new NotifyResult
//        //            {
//        //                OrderName = dictionary["trade_status"] + "|" + (dictionary["error_message"] ?? string.Empty),
//        //                OrderID = dictionary["out_trade_no"],
//        //                Amount = dictionary["amount_str"].Parse<double>(),
//        //                Tax = 1,
//        //                Currency = Currency.CNY,
//        //                Business = dictionary["partner"],
//        //                TxnID = dictionary["notify_id"],
//        //                PaymentName = this.Name + "_cash",
//        //                PaymentDate = DateTime.UtcNow,

//        //                Customer_Street = "",
//        //                Customer_City = "",
//        //                Customer_State = "",
//        //                Customer_Zip = "",
//        //                Customer_Country = "",
//        //                Customer_Name = "",
//        //                Customer_Email = "",
//        //                Customer_Phone = "",

//        //                Customer_Business = "",
//        //                Customer_Status = dictionary["trade_status"] == "3",

//        //                ReturnTag = dictionary["notify_id"],
//        //            };
//        //        }
//        //        return Result;
//        //    }
//        //    return null;
//        //}

//        //public bool Cash(string p_OrderId, string p_BankId, string p_BankAccountName, string p_BankAccountNo, double p_Amount, string p_Remark = "", string p_NotifyUrl = "")
//        //{
//        //    if (string.IsNullOrEmpty(this[PID])) throw new ArgumentNullException("PID");
//        //    if (string.IsNullOrEmpty(this[CashKey])) throw new ArgumentNullException("CashKey");
//        //    if (!BankCashList.ContainsValue(p_BankId)) throw new ArgumentException("BankId is not allowed!");

//        //    if (string.IsNullOrWhiteSpace(p_OrderId)) throw new ArgumentException("OrderId 不得为空");
//        //    if (p_OrderId.Length > 64) throw new ArgumentException("OrderId 长度不能大于64");

//        //    if (string.IsNullOrWhiteSpace(p_BankAccountName)) throw new ArgumentException("BankAccountName 不得为空");
//        //    if (p_BankAccountName.Length > 32) throw new ArgumentException("BankAccountName 长度不能大于32");

//        //    if (string.IsNullOrWhiteSpace(p_BankAccountNo)) throw new ArgumentException("BankAccountNo 不得为空");
//        //    if (p_BankAccountNo.Length > 32) throw new ArgumentException("BankAccountName 长度不能大于32");

//        //    if (p_Amount <= 0) throw new ArgumentException("Amount 必须大于0元");
//        //    if (p_Amount > 50000) throw new ArgumentException("Amount 不能大于5万元");

//        //    if (p_Remark.Length > 80) throw new ArgumentException("Remark 长度不能大于80");

//        //    Uri uri = new Uri("https://www.ebatong.com/gateway/agentDistribution.htm");

//        //    //测试请求接口：http://180.168.127.5/gateway/agentDistribution.htm
//        //    //测试商户号：201303101809506780
//        //    //测试商户密钥：6G13P7W8C8NU6J5L5IOIBBZAKNI3ROaqdevz

//        //    //uri = new Uri("http://180.168.127.5/gateway/agentDistribution.htm");
//        //    //this[PID] = "201303101809506780";
//        //    //m_CashKey = "6G13P7W8C8NU6J5L5IOIBBZAKNI3ROaqdevz";

//        //    SortedList<string, string> sPara = new SortedList<string, string>();
//        //    sPara.Add("service", "ebatong_agent_distribution");
//        //    sPara.Add("input_charset", "utf-8");
//        //    sPara.Add("partner", this[PID]);
//        //    sPara.Add("sign_type", "MD5");
//        //    sPara.Add("return_url",p_NotifyUrl);
//        //    sPara.Add("out_trade_no", p_OrderId);
//        //    sPara.Add("bank_name", BankCashList.First(p => p.Value == p_BankId).Key);
//        //    sPara.Add("bank_account_name", p_BankAccountName);
//        //    sPara.Add("bank_account_no", p_BankAccountNo);
//        //    sPara.Add("amount_str", p_Amount.ToString("0.00"));
//        //    sPara.Add("remark", p_Remark);
//        //    sPara.Add("agent_time", DateTime.Now.ToString("yyyyMMddHHmmss"));
//        //    sPara.Add("to_account_mode", "1000");

//        //    Dictionary<string, string> data = sPara.ToDictionary(p => p.Key, p => p.Value);
//        //    data.Add("sign", Utils.HASHCrypto.Generate( Utils.HASHCrypto.CryptoEnum.MD5).Encrypt(sPara.Aggregate("", (c, p) => c + p.Key + "=" + p.Value + "&").TrimEnd('&') + this[CashKey]));
//        //    try
//        //    {
//        //        HttpWebUtility hwu = new HttpWebUtility();
//        //        var source = hwu.Response(uri, HttpWebUtility.HttpMethod.POST,
//        //            data);

//        //        if (source == p_OrderId + "||T")
//        //        {
//        //            return true;
//        //        }
//        //    }
//        //    catch
//        //    { }

//        //    return false;
//        //}

//        #endregion

//        public static Dictionary<string, string> BankList
//        {
//            get
//            {
//                var res = new Dictionary<string, string>();
//                res["工商银行"] = "ICBC_B2C";
//                res["农业银行"] = "ABC_B2C";
//                res["建设银行"] = "CCB_B2C";
//                res["交通银行"] = "COMM_B2C";
//                //res["中国银行"] = "BOCSH_B2C";
//                //res["邮政储蓄银行"] = "POSTGC_B2C";
//                res["招商银行"] = "CMB_B2C";
//                res["民生银行"] = "CMBCD_B2C";
//                res["中信银行"] = "CNCB_B2C";
//                //res["平安银行"] = "PINGAN_B2C";
//                res["光大银行"] = "CEB_B2C";
//                //res["兴业银行"] = "CIB_B2C";
//                res["华夏银行"] = "HXB_B2C";
//                res["浦发银行"] = "SPDB_B2C";
//                //res["广发银行"] = "GDB_B2C";

//                res["广州银行"] = "GZCB_B2C";
//                res["北京银行"] = "BOB_B2C";
//                //res["北京农村商业银行"] = "BJRCB_B2C";
//                res["上海农村商业银行"] = "SRCB_B2C";
//                //res["富滇银行"] = "FDB_B2C";
//                //res["渤海银行"] = "CBHB_B2C";
//                res["浙商银行"] = "CZB_B2C";
//                res["南京银行"] = "BON_B2C";
//                res["杭州银行"] = "HZCB_B2C";
//                res["上海银行"] = "BOS_B2C";
//                //res["温州银行"] = "WZCB_B2C";
//                res["东亚银行"] = "BEA_B2C";
//                //res["宁波银行"] = "NBCB_B2C";
//                res["徽商银行"] = "HSB_B2C";
//                //res["腾付通快捷"] = "TFTPAY_FP";
//                return res;
//            }
//        }

//        public static Dictionary<string, string> BankCashList
//        {
//            get
//            {
//                var res = new Dictionary<string, string>();
//                res["工商银行"] = "ICBC_B2C";
//                res["农业银行"] = "ABC_B2C";
//                res["建设银行"] = "CCB_B2C";
//                res["交通银行"] = "COMM_B2C";
//                res["中国银行"] = "BOCSH_B2C";
//                res["邮政储蓄银行"] = "POSTGC_B2C";
//                res["招商银行"] = "CMB_B2C";
//                res["民生银行"] = "CMBCD_B2C";
//                res["中信银行"] = "CNCB_B2C";
//                res["平安银行"] = "PINGAN_B2C";
//                res["光大银行"] = "CEB_B2C";
//                res["兴业银行"] = "CIB_B2C";
//                res["华夏银行"] = "HXB_B2C";
//                res["浦发银行"] = "SPDB_B2C";
//                res["广发银行"] = "GDB_B2C";
//                return res;
//            }
//        }

//        public override NotifyResult Notify()
//        {
//            var res = base.Notify();
//            return res;

//            //if (Params["service"] == "ebatong_agen t_distribution")
//            //{
//            //    var res = CashCheck(dic);
//            //    if (res != null)
//            //    {
//            //        HttpContext.Current.Response.Write(res.ReturnTag);
//            //    }
//            //    return res;
//            //}
//        }

//        public override NotifyResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
//            IDictionary<string, string> head, string body,string notifyip)
//        {
//            var dictionary = form;
//            var IsVlidate = false;
//            if (dictionary["sign"] != null && dictionary["seller_id"] == this[PID] &&
//                dictionary["trade_status"] == "TRADE_FINISHED")
//            {
//                IsVlidate =
//                    string.Equals(
//                        Utils.HASHCrypto.Generate( Utils.HASHCrypto.CryptoEnum.MD5).Encrypt(
//                            dictionary.Where(p => p.Key != "sign")
//                                .Aggregate("", (c, p) => c + p.Key + "=" + p.Value + "&")
//                                .TrimEnd('&') + this[Key]), dictionary["sign"], StringComparison.OrdinalIgnoreCase);
//                ;
//            }

//            if (IsVlidate)
//            {
//                var Result = new NotifyResult
//                {
//                    OrderName = dictionary["subject"],
//                    OrderID = dictionary["out_trade_no"],
//                    Amount = dictionary["total_fee"].Parse<double>(),
//                    Tax = dictionary["total_fee"].Parse<double>() * 0.003,
//                    Currency = Currency.CNY,
//                    Business = dictionary["seller_id"],
//                    TxnID = dictionary["trade_no"],
//                    PaymentName = Name,
//                    PaymentDate = DateTime.UtcNow,
//                    Customer_Street = "",
//                    Customer_City = "",
//                    Customer_State = "",
//                    Customer_Zip = "",
//                    Customer_Country = "",
//                    Customer_Name = "",
//                    Customer_Email = "",
//                    Customer_Phone = "",
//                    Customer_Business = dictionary.ContainsKey("buyer_id") ? dictionary["buyer_id"] : "",
//                    Customer_Status = true,
//                    ReturnTag = dictionary["notify_id"]
//                };
//                return Result;
//            }
//            return null;
//        }

//        public override PayTicket Pay(string p_OrderId, double p_Amount,
//            Currency p_Currency = Currency.CNY, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
//            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//        {
//            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

//            if (string.IsNullOrEmpty(this[PID])) throw new ArgumentNullException("PID");
//            if (string.IsNullOrEmpty(this[Key])) throw new ArgumentNullException("Key");
//            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

//            var timesamp = "";
//            try
//            {
//                var hwu = new HttpWebUtility();
//                var data = new SortedList<string, string>();
//                data["service"] = "query_timestamp";
//                data["partner"] = this[PID];
//                data["sign_type"] = "MD5";
//                data["input_charset"] = "utf-8";
//                data["sign"] =
//                    Utils.HASHCrypto.Generate( Utils.HASHCrypto.CryptoEnum.MD5).Encrypt(data.Aggregate("", (c, p) => c + p.Key + "=" + p.Value + "&").TrimEnd('&') + this[Key]);
//                var resource = hwu.Response(new Uri("https://www.ebatong.com/gateway.htm"),
//                    HttpWebUtility.HttpMethod.POST, data.ToDictionary(p => p.Key, p => p.Value));
//                if (resource.Contains("encrypt_key"))
//                {
//                    var m = TimestampRegex.Match(resource);
//                    if (m.Success && m.Groups.Count == 2 && m.Groups[1].Success)
//                        timesamp = m.Groups[1].Value;
//                }
//            }
//            catch
//            {
//            }

//            var sPara = new SortedList<string, string>();
//            //构造签名参数数组,签名方法要求按参数名称升序排列

//            //基本参数
//            sPara.Add("sign_type", "MD5");
//            sPara.Add("partner", this[PID]);
//            sPara.Add("service", "create_direct_pay_by_user");
//            sPara.Add("input_charset", "utf-8");
//            sPara.Add("notify_url", p_NotifyUrl);
//            sPara.Add("return_url",p_ReturnUrl);
//            //业务参数
//            sPara.Add("anti_phishing_key", timesamp);
//            sPara.Add("out_trade_no", p_OrderId);
//            sPara.Add("subject", p_OrderName);
//            sPara.Add("exter_invoke_ip", Core.ClientIP);
//            sPara.Add("payment_type", "1");
//            sPara.Add("seller_id", this[PID]);
//            sPara.Add("total_fee", p_Amount.ToString("0.00"));
//            sPara.Add("pay_method", "bankPay");

//            var pe = extend_params as string;
//            if (pe != null)
//                if (!string.IsNullOrWhiteSpace(pe) && BankList.ContainsValue(pe))
//                    sPara.Add("default_bank", pe);

//            var formhtml =
//                new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" + "' action='" +
//                                  GATEWAY + "' method='post' " + (Helper.NewWindow ? "target='_blank'" : "") + ">");
//            foreach (var temp in sPara)
//                formhtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");
//            formhtml.Append("<input type='hidden' name='sign' value='" +
//                            Utils.HASHCrypto.Generate( Utils.HASHCrypto.CryptoEnum.MD5).Encrypt(sPara.Aggregate("", (c, p) => c + p.Key + "=" + p.Value + "&").TrimEnd('&') +
//                                     this[Key]) + "'/>");
//            formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
//            formhtml.Append("</form>");

//            var pt = new PayTicket();
//            pt.FormHtml = formhtml.ToString();
//            return pt;
//        }
//    }
//}