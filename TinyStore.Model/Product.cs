using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyStore.Model
{
    [Serializable]
    public class ProductModel : SupplyModel
    {
        /// <summary>
        /// 商品编号
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, Length = 28)]
        public string ProductId { get; set; }

        [SqlSugar.SugarColumn(Length = 28, IndexGroupNameList = new[] {"SupplyId"})]
        public new string SupplyId { get; set; }

        /// <summary>
        /// 店铺编号
        /// </summary>
        [SqlSugar.SugarColumn(Length = 28, IndexGroupNameList = new[] {"StoreId"})]
        public string StoreId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new[] {"Sort"})]
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