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
        public string Memo { get; set; }

        /// <summary>
        ///价格
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        ///数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        ///折扣
        /// </summary>
        public double Discount { get; set; }

        /// <summary>
        ///进货成本
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        ///客户端IP
        /// </summary>
        [SqlSugar.SugarColumn(Length = 45)]
        public string ClientIP { get; set; }

        /// <summary>
        ///客户端信息
        /// </summary>
        [SqlSugar.SugarColumn(Length = 500)]
        public string UserAgent { get; set; }

        /// <summary>
        ///客户端语言
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string AcceptLanguage { get; set; }

        /// <summary>
        ///创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        ///支付类型
        /// </summary>
        [SqlSugar.SugarColumn(Length = 15)]
        public string PaymentType { get; set; }

        /// <summary>
        ///交易编号
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string TranId { get; set; }

        /// <summary>
        ///到账金额-notify过来的金额
        /// </summary>
        public double Income { get; set; }

        /// <summary>
        ///发货日期
        /// </summary>
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        ///联系方式 QQ或电话
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string Contact { get; set; }

        /// <summary>
        ///通知账户 电话或邮箱，用来给客户发卡密
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string NoticeAccount { get; set; }

        /// <summary>
        ///支付方式费用
        /// </summary>
        public double PaymentFee { get; set; }

        /// <summary>
        ///是否付款
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{"IsPay"})]
        public bool IsPay { get; set; }

        /// <summary>
        ///是否发货
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{"IsDelivery"})]
        public bool IsDelivery { get; set; }

        /// <summary>
        ///退款金额
        /// </summary>
        public double ReturnAmount { get; set; }

        /// <summary>
        ///退款时间
        /// </summary>
        public DateTime ReturnDate { get; set; }

        /// <summary>
        /// 卡密列表
        /// </summary>
        [SqlSugar.SugarColumn(IsJson = true,ColumnDataType = "text")]
        public List<Model.Extend.StockOrder> StockList { get; set; }

        /// <summary>
        /// 是否结算  => 货源供货商
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{"IsSettle"})]
        public bool IsSettle { get; set; }

        /// <summary>
        /// 结算时间
        /// </summary>
        public DateTime SettleDate { get; set; }



        [SugarColumn(IsIgnore = true)] 
        public string StoreName { get; set; }

        [SugarColumn(IsIgnore = true)] 
        public string StoreUniqueId { get; set; }

        /// <summary>
        /// 最后变更状态时间
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new[] {"LastUpdateDate"})]
        public DateTime LastUpdateDate { get; set; }


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