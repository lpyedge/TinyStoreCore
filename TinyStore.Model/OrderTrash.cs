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

        static OrderTrashModel()
        {
            Nelibur.ObjectMapper.TinyMapper.Bind<Model.OrderModel, Model.OrderTrashModel>();
        }
        
        public static Model.OrderTrashModel Map(Model.OrderModel orderModel)
        {
            var data = Nelibur.ObjectMapper.TinyMapper.Map<Model.OrderTrashModel>(orderModel);
            data.DeleteDate = DateTime.Now;
            return data;
        }
        
        public static List<Model.OrderTrashModel> Map(List<Model.OrderModel> orderModelList)
        {
            List<Model.OrderTrashModel> data = new List<OrderTrashModel>();
            foreach (var item in orderModelList)
            {
                data.Add(Map(item));
            }
            return data;
        }
    }
}