﻿using System;

namespace TinyStore.Model
{
    /// <summary>
    /// 库存表 （发货方式 卡密）
    /// </summary>
    [Serializable]
    [SqlSugar.SugarTable(nameof(StockModel),IsDisabledUpdateAll=true)]
    public class StockModel
    {
        [SqlSugar.SugarColumn(IsPrimaryKey = true, Length = 28)]
        public string StockId { get; set; }

        public int UserId { get; set; } = 0;

        [SqlSugar.SugarColumn(Length = 28,IndexGroupNameList = new []{nameof(StockModel)})] 
        public string SupplyId { get; set; }

        /// <summary>
        /// 卡号 / 名称
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100,IndexGroupNameList = new []{nameof(StockModel)})] 
        public string Name { get; set; }

        /// <summary>
        /// 密码 / 说明
        /// </summary>
        [SqlSugar.SugarColumn(Length = 4000)] 
        public string Memo { get; set; }
        
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{nameof(StockModel)})]
        public bool IsDelivery { get; set; }
        
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{nameof(StockModel)})]
        public DateTime CreateDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{nameof(StockModel)})]
        public bool IsShow { get; set; }
    }
}