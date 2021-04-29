using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SqlSugar;
using TinyStore.Model;

namespace TinyStore.BLL
{
    public class OrderBLL : BaseBLL<OrderModel>
    {
        private static readonly SortDic<OrderModel> SortCreateDateDesc = new()
        {
            [p => p.CreateDate] = OrderByType.Desc
        };

        private static readonly SortDic<OrderModel> SortLastUpdateDateDesc = new()
        {
            [p => p.LastUpdateDate] = OrderByType.Desc
        };

        public static OrderModel QueryModelByTranId(string tranId)
        {
            return QueryModel(p => p.TranId == tranId);
        }

        public static OrderModel QueryModelByOrderId(string orderid)
        {
            return QueryModel(p => p.OrderId == orderid);
        }

        public static OrderModel QueryModelByOrderIdAndStoreId(string orderid, string storeId)
        {
            return QueryModel(p => p.OrderId == orderid && p.StoreId == storeId);
        }


        public static List<OrderModel> QueryListBenifitByStoreId(string suppliyid, DateTime begin, DateTime end,
            string storeId, bool ishasreturn)
        {
            var expr = Expressionable.Create<OrderModel>().And(p =>
                p.StoreId == storeId && p.IsSettle && p.LastUpdateDate >= begin && p.LastUpdateDate <= end);
            if (ishasreturn)
                expr = expr.And(p => p.RefundAmount > 0);
            if (!string.IsNullOrEmpty(suppliyid))
                expr = expr.And(p => p.SupplyId == suppliyid);
            return QueryList(-1, expr.ToExpression(), SortLastUpdateDateDesc);
        }

        public static PageList<OrderModel> QueryPageListBenifitByStoreId(string supplierid, DateTime begin,
            DateTime end, string storeId, bool ishasreturn, int pageindex, int pagesize)
        {
            var expr = Expressionable.Create<OrderModel>().And(p =>
                p.StoreId == storeId && p.IsSettle && p.LastUpdateDate >= begin && p.LastUpdateDate <= end);
            if (ishasreturn)
                expr = expr.And(p => p.RefundAmount > 0);
            if (!string.IsNullOrEmpty(supplierid))
                expr = expr.And(p => p.SupplyId == supplierid);
            return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortLastUpdateDateDesc);
        }

        public static PageList<OrderModel> QueryPageListIsPaidByStoreId(string storeId, int pageindex, int pagesize)
        {
            return QueryPageList(pageindex, pagesize, p => p.StoreId == storeId && p.IsPay && !p.IsDelivery,
                SortLastUpdateDateDesc);
        }

        public static PageList<OrderModel> QueryPageListBySearch(string storeId, string productId, DateTime? datefrom,
            DateTime? dateto, string keyname, bool? isPay, bool? isDelivery, bool? isSettle, int pageindex,
            int pagesize)
        {
            var expr = Expressionable.Create<OrderModel>()
                .And(p => p.StoreId == storeId)
                .AndIF(!string.IsNullOrWhiteSpace(productId), p => p.ProductId == productId)
                .AndIF(isPay != null, p => p.IsPay == isPay)
                .AndIF(isDelivery != null, p => p.IsDelivery == isDelivery)
                .AndIF(isSettle != null, p => p.IsSettle == isSettle)
                .AndIF(datefrom != null && dateto != null, p => SqlFunc.Between(p.CreateDate, datefrom, dateto))
                .AndIF(!string.IsNullOrWhiteSpace(keyname),
                    p => p.Contact.Contains(keyname) || p.Name.Contains(keyname) || p.Message.Contains(keyname));

            return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortCreateDateDesc);
        }

        public static PageList<OrderModel> QueryPageList(DateTime begin, DateTime end, int state, int keykind,
            string key, string storeId, bool ishasreturn, EOrderTimeType timetype, int pageindex, int pagesize)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                    return string.IsNullOrEmpty(storeId)
                        ? QueryPageList(pageindex, pagesize, p => p.OrderId == key)
                        : QueryPageList(pageindex, pagesize, p => p.OrderId == key && p.StoreId == storeId);
                if (ekeykind == EOrderSearchKey.交易编号)
                    return string.IsNullOrEmpty(storeId)
                        ? QueryPageList(pageindex, pagesize, p => p.TranId == key)
                        : QueryPageList(pageindex, pagesize, p => p.TranId == key && p.StoreId == storeId);
            }

            var expr = Expressionable.Create<OrderModel>()
                .And(p => p.CreateDate >= begin && p.CreateDate <= end);
            if (timetype == EOrderTimeType.最后变动日期)
                expr = Expressionable.Create<OrderModel>()
                    .And(p => p.LastUpdateDate >= begin && p.LastUpdateDate <= end);
            //else if (timetype == EOrderTimeType.根据到期时间)
            //    expr = SqlSugar.Expressionable.Create<Model.Order>().And(p => p.DueDate >= begin && p.DueDate <= end);
            if (!string.IsNullOrEmpty(storeId))
                expr = expr.And(p => p.StoreId == storeId);
            if (timetype == EOrderTimeType.付款日期)
                expr = expr.And(p => p.IsPay);
            if (state > 0)
            {
                var estate = (EState) state;
                if (estate == EState.客户下单)
                    expr = expr.And(p => p.IsPay == false);
                else if (estate == EState.等待发货)
                    expr = expr.And(p => p.IsPay && p.IsDelivery == false);
                else if (estate == EState.完成订单)
                    expr = expr.And(p => p.IsPay && p.IsDelivery);
            }

            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                {
                    expr = Expressionable.Create<OrderModel>().And(p => p.OrderId == key);
                    if (!string.IsNullOrEmpty(storeId))
                        expr = expr.And(p => p.StoreId == storeId);
                }
                //else if (ekeykind == EOrderSearchKey.备注)
                //    expr = expr.And(p => p.Remark.Contains(key));
                else if (ekeykind == EOrderSearchKey.联系方式)
                {
                    expr = expr.And(p => p.Contact.Contains(key));
                }
                else if (ekeykind == EOrderSearchKey.商品名称)
                {
                    expr = expr.And(p => p.Name.Contains(key));
                }
                else if (ekeykind == EOrderSearchKey.商品卡号)
                {
                    expr = expr.And(p => p.StockList.Any(s => s.Name == key));
                }
            }

            if (ishasreturn) expr = expr.And(p => p.RefundAmount > 0);

            //if (ishasduealert)
            //{
            //    expr = expr.And(p => p.DueDate < DateTime.Now.Date.AddDays(1) && p.IsDueAlert == true);
            //}
            if (timetype == EOrderTimeType.付款日期)
                return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortCreateDateDesc);
            //if (timetype == EOrderTimeType.根据到期时间)
            //    return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortDueDateDesc);
            return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortLastUpdateDateDesc);
        }

        public static PageList<OrderModel> QueryPageListBySid(string sid, int state, int keykind, string key,
            string storeId, bool ishasreturn, int pageindex, int pagesize)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                    return string.IsNullOrEmpty(storeId)
                        ? QueryPageList(pageindex, pagesize, p => p.OrderId == key)
                        : QueryPageList(pageindex, pagesize, p => p.OrderId == key && p.StoreId == storeId);
                if (ekeykind == EOrderSearchKey.交易编号)
                    return string.IsNullOrEmpty(storeId)
                        ? QueryPageList(pageindex, pagesize, p => p.TranId == key)
                        : QueryPageList(pageindex, pagesize, p => p.TranId == key && p.StoreId == storeId);
            }

            var expr = Expressionable.Create<OrderModel>().And(p =>
                p.SupplyId == sid && p.IsSettle == false && p.IsPay && p.Amount > p.RefundAmount);
            if (!string.IsNullOrEmpty(storeId))
                expr = expr.And(p => p.StoreId == storeId);
            if (state > 0)
            {
                var estate = (EState) state;
                if (estate == EState.等待发货)
                    expr = expr.And(p => p.IsPay && p.IsDelivery == false);
                else if (estate == EState.完成订单)
                    expr = expr.And(p => p.IsPay && p.IsDelivery);
            }

            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                {
                    expr = Expressionable.Create<OrderModel>().And(p => p.OrderId == key);
                    if (!string.IsNullOrEmpty(storeId))
                        expr = expr.And(p => p.StoreId == storeId);
                }
                //else if (ekeykind == EOrderSearchKey.备注)
                //    expr = expr.And(p => p.Remark.Contains(key));
                else if (ekeykind == EOrderSearchKey.联系方式)
                {
                    expr = expr.And(p => p.Contact.Contains(key));
                }
                else if (ekeykind == EOrderSearchKey.商品名称)
                {
                    expr = expr.And(p => p.Name.Contains(key));
                }
                else if (ekeykind == EOrderSearchKey.商品卡号)
                {
                    expr = expr.And(p => p.StockList.Any(s => s.Name == key));
                }
            }

            if (ishasreturn) expr = expr.And(p => p.RefundAmount > 0);

            //if (ishasduealert)
            //{
            //    expr = expr.And(p => p.DueDate < DateTime.Now.Date.AddDays(1) && p.IsDueAlert == true);
            //}
            return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortLastUpdateDateDesc);
        }

        public static List<OrderModel> QueryListBySid(string sid, int state, int keykind, string key, string storeId,
            bool ishasreturn)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                    return QueryList(-1, p => p.OrderId == key && p.StoreId == storeId);
                if (ekeykind == EOrderSearchKey.交易编号)
                    return QueryList(-1, p => p.TranId == key && p.StoreId == storeId);
            }

            var expr = Expressionable.Create<OrderModel>().And(p =>
                p.SupplyId == sid && p.IsSettle == false && p.IsPay && p.RefundAmount < p.Amount);
            if (!string.IsNullOrEmpty(storeId))
                expr = expr.And(p => p.StoreId == storeId);
            if (state > 0)
            {
                var estate = (EState) state;
                if (estate == EState.等待发货)
                    expr = expr.And(p => p.IsPay && p.IsDelivery == false);
                else if (estate == EState.完成订单)
                    expr = expr.And(p => p.IsPay && p.IsDelivery);
            }

            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.联系方式)
                    expr = expr.And(p => p.Contact.Contains(key));
                //else if (ekeykind == EOrderSearchKey.备注)
                //    expr = expr.And(p => p.Remark.Contains(key));
                else if (ekeykind == EOrderSearchKey.商品名称)
                    expr = expr.And(p => p.Name.Contains(key));
                else if (ekeykind == EOrderSearchKey.商品卡号)
                    expr = expr.And(p => p.StockList.Any(s => s.Name == key));
            }

            if (ishasreturn) expr = expr.And(p => p.RefundAmount > 0);

            //if (ishasduealert)
            //{
            //    expr = expr.And(p => p.DueDate < DateTime.Now.Date.AddDays(1) && p.IsDueAlert == true);
            //}
            return QueryList(-1, expr.ToExpression(), SortLastUpdateDateDesc);
        }

        public static void DeleteByOrderId(string orderId)
        {
            Delete(p => p.OrderId == orderId);
        }


        public static void UpdateIsSettle(List<string> ids, string storeId, bool isclose, DateTime closedate)
        {
            var expr = Expressionable.Create<OrderModel>()
                .And(p => p.IsPay && ids.Contains(p.OrderId));
            if (!string.IsNullOrEmpty(storeId))
                expr = expr.And(p => p.StoreId == storeId);
            Update(expr.ToExpression(),
                p => new OrderModel {IsSettle = isclose, LastUpdateDate = closedate, SettleDate = closedate});
        }

        public static void UpdateCost(string orderId, double cost)
        {
            Update(p => p.OrderId == orderId, p => p.Cost == cost);
        }


        public static List<Model.OrderModel> QueryListStat(string storeId, DateTime? datefrom, DateTime? dateto)
        {
            var expr = Expressionable.Create<OrderModel>()
                .And(p => p.StoreId == storeId && p.IsPay == true)
                .AndIF(datefrom != null && dateto != null, p => SqlFunc.Between(p.CreateDate, datefrom, dateto));

            using (var conn = DbClient)
            {
                return conn.Queryable<Model.OrderModel>()
                    .Where(expr.ToExpression())
                    .OrderBy(p => p.CreateDate, OrderByType.Asc)
                    .Select(p => new Model.OrderModel()
                    {
                        OrderId = p.OrderId,
                        ProductId = p.ProductId,
                        CreateDate = p.CreateDate,
                        Amount = p.Amount,
                        Quantity = p.Quantity,
                        Cost = p.Cost,
                        RefundAmount = p.RefundAmount,
                    })
                    .ToList();
            }
        }

        public static List<Model.OrderModel> QueryOrderListNotify(int userId, int lastDays)
        {
            using (var conn = DbClient)
            {
                var dateNotify = DateTime.Now.AddDays(lastDays);
                return conn.Queryable<Model.OrderModel>()
                    .Where(p => p.UserId == userId &&
                                p.IsPay == true && p.IsDelivery == true && (p.Amount * p.Quantity) != p.RefundAmount &&
                                p.NotifyDate != null && p.NotifyDate <= dateNotify)
                    .OrderBy(p => p.NotifyDate, OrderByType.Asc)
                    .Select(p => new Model.OrderModel
                    {
                        StoreId = p.StoreId,
                        OrderId = p.OrderId,
                        Name = p.Name,
                        Memo = p.Memo,
                        Amount = p.Amount,
                        Quantity = p.Quantity,
                        RefundAmount = p.RefundAmount,
                        Contact = p.Contact,
                        Message = p.Message,
                        CreateDate = p.CreateDate,
                        NotifyDate = p.NotifyDate,
                    })
                    .ToList();
            }
        }
    }
}