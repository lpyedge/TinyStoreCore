using System.Collections.Generic;

namespace LPayments
{
    public class PayTicket
    {
        public PayTicket()
        {

        }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = "";
        
        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; } = "";


        /// <summary>
        /// 支付数据格式
        /// </summary>
        public EPayDataFormat DataFormat { get; set; }

        /// <summary>
        /// 支付数据
        /// </summary>
        public string DataContent { get; set; } = "";
    }
}