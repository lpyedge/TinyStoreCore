
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyStore.Model
{
    [Serializable]
    [SqlSugar.SugarTable(nameof(WithDrawModel),IsDisabledUpdateAll=true)]
    public class WithDrawModel
    {
        /// <summary>
        /// 提现编号
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true,Length = 28)]
        public string WithDrawId { get; set; }
        
        /// <summary>
        /// 店铺编号
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{nameof(WithDrawModel)})]
        public int UserId { get; set; }
        
        /// <summary>
        /// 注释
        /// </summary>
        [SqlSugar.SugarColumn(Length = 4000)]
        public string Memo { get; set; }
        
        /// <summary>
        /// 提现金额
        /// </summary>
        public double Amount { get; set; }
        
        /// <summary>
        /// 收款方式
        /// </summary>
        public EBankType BankType { get; set; }
        
        /// <summary>
        /// 收款人名称
        /// </summary>
        [SqlSugar.SugarColumn(Length = 25)]
        public string BankPersonName { get; set; }
        
        /// <summary>
        /// 收款账户
        /// </summary>
        [SqlSugar.SugarColumn(Length = 25)]
        public string BankAccount { get; set; }
        
        /// <summary>
        /// 创建日期
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{nameof(WithDrawModel)})]
        public DateTime CreateDate { get; set; }
        
        /// <summary>
        /// 交易编号
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string TranId { get; set; }
        
        /// <summary>
        /// 处理/到账金额
        /// </summary>
        public double AmountFinish { get; set; }
        
        /// <summary>
        /// 已结束
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{nameof(WithDrawModel)})]
        public bool IsFinish { get; set; }
        
        /// <summary>
        /// 结束日期
        /// </summary>
        [SqlSugar.SugarColumn(IsNullable = true)]
        public DateTime? FinishDate { get; set; }
    }
}
