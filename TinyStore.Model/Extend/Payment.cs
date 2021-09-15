using System;
using System.Collections.Generic;
using System.Text;

namespace TinyStore.Model.Extend
{
    [Serializable]
    public class Payment
    {
        /// <summary>
        /// 是否系统支付方式
        /// </summary>
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; } = true;

        /// <summary>
        /// 手续费率
        /// </summary>
        public double Rate { get; set; }
        
        /// <summary>
        /// 标题
        /// </summary>
        public string Subject { get; set; }
        
        /// <summary>
        /// 收款说明
        /// </summary>
        public string Memo { get; set; }
        
        /// <summary>
        /// 收款二维码
        /// </summary>
        public string QRCode { get; set; }
        
        /// <summary>
        /// 收款方式
        /// </summary>
        public EBankType BankType { get; set; }
        
        /// <summary>
        /// 收款帐号
        /// </summary>
        public string Account { get; set; }
        
        /// <summary>
        /// 收款名称
        /// </summary>
        public string Name { get; set; }
    }
}
