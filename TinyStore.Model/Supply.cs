using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;

namespace TinyStore.Model
{
    [Serializable]
    public class SupplyModel
    {
        /// <summary>
        /// 货源编号
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true,Length = 28)]
        public string SupplyId { get; set; }

        /// <summary>
        /// 货源名称
        /// </summary>
        [SqlSugar.SugarColumn(Length = 50)]
        public string Name { get; set; }

        /// <summary>
        /// 说明文字
        /// </summary>
        [SqlSugar.SugarColumn(Length = 500)]
        public string Memo { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        [SqlSugar.SugarColumn(Length = 50)]
        public string Category { get; set; }

        /// <summary>
        /// 面值 (刊列价)
        /// </summary>
        public double FaceValue { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        /// 是否上架 仅配合userId为0 即系统货源情况下使用
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{"IsShow"})]
        public bool IsShow { get; set; }

        /// <summary>
        /// 发货方式
        /// </summary>
        public EDeliveryType DeliveryType { get; set; }

        /// <summary>
        /// 商户编号 (0 表示系统货源)
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{"UserId"})]
        public int UserId { get; set; }
        
        [SugarColumn(IsIgnore = true)] 
        public int StockNumber { get; set; }
    }
}