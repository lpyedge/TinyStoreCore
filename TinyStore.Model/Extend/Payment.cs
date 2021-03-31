using System;
using System.Collections.Generic;
using System.Text;

namespace TinyStore.Model.Extend
{
    public class Payment
    {
        /// <summary>
        /// 支付方式
        /// </summary>
        public string PaymentType{ get; set; }
        
        /// <summary>
        /// 支付名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 收款帐号
        /// </summary>
        public string Account { get; set; }
        
        /// <summary>
        /// 收款说明
        /// </summary>
        public string Memo { get; set; }
        
        /// <summary>
        /// 收款二维码
        /// </summary>
        public string QRCode { get; set; }
        
        /// <summary>
        /// 手续费率
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        /// 是否展示
        /// </summary>
        public bool IsShow { get; set; } = true;

        /// <summary>
        /// 是否系统支付方式
        /// </summary>
        public bool IsSystem { get; set; } = true;
    }
}
