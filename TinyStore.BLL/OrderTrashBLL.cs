using System;
using System.Linq;


namespace TinyStore.BLL
{
    public class OrderTrashBLL : BaseBLL<Model.OrderTrashModel>
    {
        private static SortDic<Model.OrderTrashModel> SortCreateDateDesc = new SortDic<Model.OrderTrashModel>()
        {
            [p => p.DeleteDate] = SqlSugar.OrderByType.Desc,
        };

        public static PageList<Model.OrderTrashModel> QueryPageList(string sid, DateTime begin, DateTime end, int state, int keykind, string key, string storeId, int pageindex, int pagesize)
        {
            var expr = SqlSugar.Expressionable.Create<Model.OrderTrashModel>().And(p => p.DeleteDate >= begin && p.DeleteDate <= end);
            if (!string.IsNullOrEmpty(storeId))
                expr = expr.And(p => p.StoreId == storeId);
            if (state > 0)
            {
                var estate = (EState)state;
                if (estate == EState.用户下单)
                    expr = expr.And(p => p.IsPay == false);
                else if (estate == EState.等待发货)
                    expr = expr.And(p => p.IsPay == true && p.IsDelivery == false);
                else if (estate == EState.完成订单)
                    expr = expr.And(p => p.IsPay == true && p.IsDelivery == true);
            }
            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey)keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                {
                    expr = SqlSugar.Expressionable.Create<Model.OrderTrashModel>().And(p => p.OrderId == key);
                    if (!string.IsNullOrEmpty(storeId))
                        expr = expr.And(p => p.StoreId == storeId);
                }
                else if (ekeykind == EOrderSearchKey.联系方式)
                    expr = expr.And(p => p.Contact.Contains(key));
                else if (ekeykind == EOrderSearchKey.商品名称)
                    expr = expr.And(p => p.Name.Contains(key));
                else if (ekeykind == EOrderSearchKey.商品卡号)
                    expr = expr.And(p => p.StockList.Any(s => s.Name == key));
            }
            if (!string.IsNullOrEmpty(sid))
            {
                expr = expr.And(p => p.SupplyId == sid && p.IsSettle == false);
            }
            return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortCreateDateDesc);
        }
    }
}