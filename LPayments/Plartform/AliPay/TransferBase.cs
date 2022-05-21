using System;
using System.Collections.Generic;
using System.Text;

namespace LPayments.Plartform.AliPay
{
    public class TransferBase : _AliPay, ITransfer
    {
        /// <summary>
        /// 转账详情
        /// </summary>
        internal class TransferDetail
        {
            public TransferDetail()
            {
                out_biz_no = DateTime.UtcNow.ToString("yyyyMMddHHmmssfffffff");
            }

            /// <summary>
            ///  订单id
            /// </summary>
            public string out_biz_no { get; set; }

            /// <summary>
            ///     收款账号类型
            /// </summary>
            public string payee_type
            {
                get
                {
                    if (payee_account.Length == 16 & payee_account.StartsWith("2088"))
                    {
                        return "ALIPAY_USERID";
                    }
                    else
                    {
                        return "ALIPAY_LOGONID";
                    }
                }
            }

            /// <summary>
            ///     收款账号或者快捷登录的OpenId
            /// </summary>
            public string payee_account { get; set; }

            /// <summary>
            ///     收款金额
            /// </summary>
            public double amount { get; set; }

            /// <summary>
            ///     付款方姓名
            /// </summary>
            public string payer_show_name { get; set; }

            /// <summary>
            ///     收款方姓名
            /// </summary>
            public string payee_real_name { get; set; }

            /// <summary>
            ///     转账说明
            /// </summary>
            public string remark { get; set; }
        }

        public class PayExtend
        {
            public PayExtend()
            {
                Royaltys = new List<RoyaltyDetail>();
            }

            /// <summary>
            /// 分润列表
            /// </summary>
            public List<RoyaltyDetail> Royaltys { get; set; }

            public class RoyaltyDetail
            {
                /// <summary>
                ///     平台名称
                /// </summary>
                public string AppName { get; set; }

                /// <summary>
                ///     收款账号或者2088开头的16位数字ID
                /// </summary>
                public string Account { get; set; }

                /// <summary>
                ///     收款金额(必须大于等于0.1)
                /// </summary>
                public double Amount { get; set; }

                /// <summary>
                ///     收款方姓名
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                ///     分润说明
                /// </summary>
                public string Remark { get; set; }

                /// <summary>
                /// 交易id（分润成功时返回）
                /// </summary>
                public string TransactionId { get; set; }
            }
        }
        
        public TransferBase() : base()
        {
        }

        public TransferBase(string p_SettingsJson) : base(p_SettingsJson)
        {
        }

        public dynamic Transfer(dynamic extend)
        {
            var payExtend = Utils.Json.Deserialize<PayExtend>(extend);
            foreach (var item in payExtend.Royaltys)
            {
                var transferdetail = new TransferDetail()
                {
                    amount = item.Amount,
                    payee_account = item.Account,
                    payee_real_name = item.Name,
                    payer_show_name = item.AppName,
                    remark = item.Remark
                };
                var tranid = "";


                //执行转账
                var biz = new Dictionary<string, dynamic>();
                biz["out_biz_no"] = transferdetail.out_biz_no;
                biz["payee_account"] = transferdetail.payee_account;
                biz["payee_real_name"] = transferdetail.payee_real_name;
                biz["payee_type"] = transferdetail.payee_type;
                biz["payer_show_name"] = transferdetail.payer_show_name;
                biz["remark"] = transferdetail.remark;
                biz["amount"] = transferdetail.amount;

                var dic = PublicDic("alipay.fund.trans.toaccount.transfer");

                dic["biz_content"] = Utils.Json.Serialize(biz);
                dic["sign"] = Convert.ToBase64String(
                    Utils.RSACrypto.SignData(m_AppPrivateProvider, Utils.HASHCrypto.CryptoEnum.SHA256,
                        Encoding.GetEncoding(Charset).GetBytes(Utils.Core.LinkStr(dic,encode:true)))
                );

                var res = _HWU.PostStringAsync(new Uri(GateWay), Utils.Core.LinkStr( dic,encode:true)).Result;
                var json = Utils.DynamicJson.Parse(res);

                if (res.Contains("\"sign\":") && res.Contains("\"alipay_fund_trans_toaccount_transfer_response\":"))
                {
                    var resdic = new Dictionary<string, string>();
                    resdic["code"] = json.alipay_fund_trans_toaccount_transfer_response.code.ToString();
                    resdic["msg"] = json.alipay_fund_trans_toaccount_transfer_response.msg.ToString();
                    if (resdic["code"] == "10000")
                    {
                        if (string.Equals(json.alipay_fund_trans_toaccount_transfer_response.out_biz_no.ToString(),
                            transferdetail.out_biz_no))
                        {
                            tranid = json.alipay_fund_trans_toaccount_transfer_response.order_id.ToString();
                        }
                    }
                }

                item.TransactionId = tranid;
            }

            return payExtend;
        }
    }
}