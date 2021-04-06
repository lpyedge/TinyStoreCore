﻿using System;
using System.Collections.Generic;


namespace TinyStore.Model
{
    [Serializable]
    public class StoreModel
    {
        /// <summary>
        /// 店铺编号
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true,Length = 28)]
        public string StoreId { get; set; }

        /// <summary>
        /// 商户编号
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{"UserId"})]
        public int UserId { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [SqlSugar.SugarColumn(Length = 50)] 
        public string TelPhone { get; set; } = "";

        /// <summary>
        /// 联系邮箱
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)] 
        public string Email { get; set; } = "";

        /// <summary>
        /// 联系QQ
        /// </summary>
        [SqlSugar.SugarColumn(Length = 50)] 
        public string QQ { get; set; } = "";

        /// <summary>
        /// 店铺名称
        /// </summary>
        [SqlSugar.SugarColumn(Length = 50)] 
        public string Name { get; set; }

        /// <summary>
        /// 店铺Logo
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)] 
        public string Logo { get; set; }

        /// <summary>
        ///  店铺名称 首字母
        /// </summary>
        [SqlSugar.SugarColumn(Length = 1,IndexGroupNameList = new []{"Initial"})] 
        public string Initial { get; set; }

        /// <summary>
        /// 店铺说明
        /// </summary>
        [SqlSugar.SugarColumn(Length = 500)] 
        public string Memo { get; set; } = "";

        /// <summary>
        /// 店铺模版 单页面方式 呈现商品
        /// </summary>
        public bool IsSingle { get; set; } = true;
        /// <summary>
        /// 店铺模版
        /// </summary>
        public EStoreTemplate Template { get; set; } = EStoreTemplate.模板一;

        /// <summary>
        /// 顶级域名
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)] 
        public string DomainTop { get; set; } = "";

        /// <summary>
        /// 店铺自定义收款方式
        /// </summary>
        [SqlSugar.SugarColumn(IsJson = true,ColumnDataType = "text")]
        public List<Model.Extend.Payment> PaymentList { get; set; }

        /// <summary>
        /// 店铺标识
        /// </summary>
        [SqlSugar.SugarColumn(Length = 25,IndexGroupNameList = new []{"CreateDate"})]
        public string UniqueId { get; set; }


        /// <summary>
        /// 热门排序 默认值0，越大越靠前
        /// </summary>
        public int Sort { get; set; } = 0;

    }
}