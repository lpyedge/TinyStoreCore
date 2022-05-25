using System;
using System.ComponentModel;

namespace Payments
{
    [Serializable]
    public sealed class PayResult
    {
        public enum EStatus
        {
            /// <summary>
            ///     完成
            /// </summary>
            [Description("完成")] Completed,

            /// <summary>
            ///     创建
            /// </summary>
            [Description("创建")] Created,

            /// <summary>
            ///     等待
            /// </summary>
            [Description("等待")] Pending,

            /// <summary>
            ///     失败
            /// </summary>
            [Description("失败")] Failed,

            /// <summary>
            ///     退款
            /// </summary>
            [Description("退款")] Refunded,

            /// <summary>
            ///     撤款
            /// </summary>
            [Description("撤款")] Reversed,

            /// <summary>
            ///     撤款取消
            /// </summary>
            [Description("撤款取消")] Canceled_Reversal,

            /// <summary>
            ///     过期
            /// </summary>
            [Description("过期")] Expired,

            /// <summary>
            ///     执行
            /// </summary>
            [Description("执行")] Processed,

            /// <summary>
            ///     验证
            /// </summary>
            [Description("验证")] Voided
        }

        public PayResult()
        {
            Status = EStatus.Completed;

            PaymentName = "";
            Business = "";
            OrderID = "";
            OrderName = "";
            Amount = 0;
            Tax = -1;
            Currency = ECurrency.USD;
            TxnID = "";
            PaymentDate = DateTime.MinValue;
            DueDate = DateTime.MinValue;
            Info = "";
            Extend = null;

            Message = "";

            Customer = new _Customer();
        }

        #region 必备属性

        /// <summary>
        ///     付款类型
        /// </summary>
        public string PaymentName { get; set; }

        /// <summary>
        ///     商家帐号
        /// </summary>
        public string Business { get; set; }

        /// <summary>
        ///     订单ID
        /// </summary>
        public string OrderID { get; set; }

        /// <summary>
        ///     订单名称
        /// </summary>
        public string OrderName { get; set; }

        /// <summary>
        ///     付款金额
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        ///     付款税费
        /// </summary>
        public double Tax { get; set; }

        /// <summary>
        ///     货币名称
        /// </summary>
        public ECurrency Currency { get; set; }

        /// <summary>
        ///     支付网站的交易ID
        /// </summary>
        public string TxnID { get; set; }

        /// <summary>
        ///     付款日期
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        ///     到期日期(订阅类使用)
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        ///     验证成功
        /// </summary>
        public EStatus Status { get; set; }

        /// <summary>
        ///     说明文字
        /// </summary>
        public string Info { get; set; }


        /// <summary>
        /// 附加内容
        /// </summary>
        public dynamic Extend { get; set; }

        /// <summary>
        ///     下发内容
        /// </summary>
        public string Message { get; set; }

        #endregion 必备属性

        public _Customer Customer { get; set; }

        /// <summary>
        /// 客户信息
        /// </summary>
        public class _Customer
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
            public string Country { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Business { get; set; }
            public bool Status { get; set; }
        }
    }
}