using SqlSugar;
using System;
using System.Collections.Generic;

namespace TinyStore.Model
{
    [Serializable]
    public class OrderModel
    {
        /// <summary>
        ///编号
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, Length = 28)]
        public string OrderId { get; set; }

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
        ///商品编号
        /// </summary>
        [SqlSugar.SugarColumn(Length = 28,IndexGroupNameList = new []{"ProductId"})]
        public string ProductId { get; set; }
        
        /// <summary>
        /// 货源编号
        /// </summary>
        [SqlSugar.SugarColumn(Length = 28)]
        public string SupplyId { get; set; }

        /// <summary>
        ///商品名称
        /// </summary>
        [SqlSugar.SugarColumn(Length = 50)]
        public string Name { get; set; }

        /// <summary>
        ///商品注释
        /// </summary>
        [SqlSugar.SugarColumn(Length = 4000)]
        public string Memo { get; set; } = "";

        /// <summary>
        ///商品单价
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        ///购买数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        ///商品成本
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        ///客户端IP
        /// </summary>
        [SqlSugar.SugarColumn(Length = 45)]
        public string ClientIP { get; set; } = "";

        /// <summary>
        ///客户端信息
        /// </summary>
        [SqlSugar.SugarColumn(Length = 500)]
        public string UserAgent { get; set; }= "";

        /// <summary>
        ///客户端语言
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string AcceptLanguage { get; set; }= "";

        /// <summary>
        ///创建日期
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;
        

        /// <summary>
        ///联系方式 邮箱 或 电话
        /// 用于发送卡密，默认邮箱
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string Contact { get; set; }

        /// <summary>
        ///用户留言 一般留下qq或者其他即时联系方式
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string Message { get; set; } = "";



        /// <summary>
        ///是否付款
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new[] {"IsPay"})]
        public bool IsPay { get; set; } = false;

        /// <summary>
        ///支付类型
        /// </summary>
        [SqlSugar.SugarColumn(Length = 15)]
        public string PaymentType { get; set; } = "";

        /// <summary>
        ///交易编号
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string TranId { get; set; } = "";

        /// <summary>
        ///支付方式费用
        /// </summary>
        public double PaymentFee { get; set; } = 0;
        
        /// <summary>
        /// 结算时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? PaymentDate { get; set; } = null;
        


        /// <summary>
        ///是否发货
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{"IsDelivery"})]
        public bool IsDelivery { get; set; }= false;
        
        /// <summary>
        ///发货日期
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// 卡密列表
        /// </summary>
        [SqlSugar.SugarColumn(IsJson = true, ColumnDataType = "text")]
        public List<Model.Extend.StockOrder> StockList { get; set; } = new List<Model.Extend.StockOrder>();
        

        /// <summary>
        /// 是否结算  => 货源供货商
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new[] {"IsSettle"})]
        public bool IsSettle { get; set; } = false;

        /// <summary>
        /// 结算时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? SettleDate { get; set; } = null;



        /// <summary>
        ///退款金额
        /// </summary>
        public double RefundAmount { get; set; } = 0;

        /// <summary>
        ///退款时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? RefundDate { get; set; } = null;
        
        
        /// <summary>
        /// 最后变更状态时间
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new[] {"LastUpdateDate"})]
        public DateTime LastUpdateDate { get; set; } 
        
        /// <summary>
        /// 提醒时间 用于续费提醒
        /// </summary>
        [SugarColumn(IsNullable = true)] 
        public DateTime? NotifyDate { get; set; } = null;


        [SugarColumn(IsIgnore = true)] 
        public string StoreName { get; set; } = "";

        [SugarColumn(IsIgnore = true)] 
        public string StoreUniqueId { get; set; } = "";


        [SugarColumn(IsIgnore = true)]
        public EState State
        {
            get
            {
                if (!IsPay)
                    return EState.客户下单;
                if (IsPay && !IsDelivery)
                    return EState.等待发货;
                if (IsPay && IsDelivery)
                    return EState.完成订单;
                return EState.客户下单;
            }
        }
    }
}