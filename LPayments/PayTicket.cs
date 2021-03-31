namespace LPayments
{
    public class PayTicket
    {
        public PayTicket()
        {
            FormHtml = "";
            Message = "";
            Url = "";
            Extra = "";
            Sync = true;
        }

        /// <summary>
        ///异步通知
        /// </summary>
        public bool Sync { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 支付跳转网址 / 二维码地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Web支付Form内容
        /// </summary>
        public string FormHtml { get; set; }

        /// <summary>
        /// App支付用
        /// </summary>
        public dynamic Extra { get; set; }
    }
}