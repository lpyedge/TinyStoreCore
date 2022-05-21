using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyStore.Model
{
    [Serializable]
    [SqlSugar.SugarTable(nameof(ProductModel),IsDisabledUpdateAll=true)]
    public class ProductModel : SupplyModel
    {
        /// <summary>
        /// 商品编号
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, Length = 28)]
        public string ProductId { get; set; }
        

        [SqlSugar.SugarColumn(Length = 28, IndexGroupNameList = new[] {nameof(ProductModel)})]
        public new string SupplyId { get; set; }
        
        /// <summary>
        /// 货源用户编号
        /// 用于区分是否系统货源订单
        /// </summary>
        public int SupplyUserId{ get; set; }
        
        /// <summary>
        /// 店铺编号
        /// </summary>
        [SqlSugar.SugarColumn(Length = 28, IndexGroupNameList = new[] {nameof(ProductModel)})]
        public string StoreId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new[] {nameof(ProductModel)})]
        public int Sort { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string Icon { get; set; }

        /// <summary>
        /// 售价
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// 最低购买数量
        /// </summary>
        public int QuantityMin { get; set; }
    }
}