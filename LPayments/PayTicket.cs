using System.Collections.Generic;

namespace LPayments
{
    public class PayTicket
    {
        public PayTicket()
        {
            Message = "";
            Uri = "";
            Sync = true;
        }

        public PayTicket(bool isSuccess = true) : this()
        {
            Success = isSuccess;
        }

        /// <summary>
        ///异步通知
        /// </summary>
        public bool Sync { get; set; } = true;

        /// <summary>
        /// 成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 支付跳转网址 / 二维码地址
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// 支付数据
        /// </summary>
        public IDictionary<string, string> Datas { get; set; }

        /// <summary>
        /// 支付数据请求方式
        /// </summary>
        public EAction Action { get; set; }

        /// <summary>
        /// App支付用
        /// </summary>
        public dynamic Token { get; set; }
    }
}