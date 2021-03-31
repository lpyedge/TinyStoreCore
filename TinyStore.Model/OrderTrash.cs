using SqlSugar;
using System;
using System.Collections.Generic;

namespace TinyStore.Model
{
    [Serializable]
    public class OrderTrashModel : OrderModel
    {
        /// <summary>
        ///编号
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true,Length = 28)]
        public new string OrderId { get; set; }
        
        public  DateTime DeleteDate { get; set; }
    }
}