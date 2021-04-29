using System;

namespace TinyStore.Model
{
    [Serializable]
    public class BillModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, Length = 28)]
        public string BillId { get; set; }
        
        /// <summary>
        ///店铺编号
        /// </summary>
        [SqlSugar.SugarColumn(Length = 28,IndexGroupNameList = new []{"StoreId"})]
        public string StoreId { get; set; }

        /// <summary>
        /// 商户编号
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{"UserId"})]
        public int UserId { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public double Amount { get; set; } = 0;

        /// <summary>
        /// 签帐金额变动
        /// </summary>
        public double AmountCharge { get; set; } = 0;
        
        /// <summary>
        /// 账目类型
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new[] {"BillType"})]
        public EBillType BillType { get; set; }
        
        /// <summary>
        /// 发生日期
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new[] {"CreateDate"})]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 附加数据 订单编号？名称？提现编号？
        /// </summary>
        [SqlSugar.SugarColumn(IsJson = true, ColumnDataType = "text")]
        public string Extra { get; set; }
    }
}