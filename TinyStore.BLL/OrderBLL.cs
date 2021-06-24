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

        public static OrderModel QueryModelById(string orderid)
        {
            return QueryModel(p => p.OrderId == orderid);
        }
        
        public static OrderModel QueryModelByPayOrderId(string payorderid)
        {
            return QueryModel(p => p.PayOrderId == payorderid);
        }

        public static PageList<OrderModel> QueryPageListBySearch(string storeId, string productId,DateTime? datefrom,
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
        
        public static PageList<OrderModel> QueryPageListBySearch4SystemSettle(int supplyUserIdSys,DateTime? datefrom,
            DateTime? dateto, string keyname, bool? isSettle, int pageindex,
            int pagesize)
        {
            var expr = Expressionable.Create<OrderModel>()
                .And(p => p.IsPay == true && p.IsDelivery == true && p.SupplyUserId == supplyUserIdSys)
                .AndIF(isSettle != null, p => p.IsSettle == isSettle)
                .AndIF(datefrom != null && dateto != null, p => SqlFunc.Between(p.CreateDate, datefrom, dateto))
                .AndIF(!string.IsNullOrWhiteSpace(keyname),
                    p => p.Contact.Contains(keyname) || p.Name.Contains(keyname) || p.Message.Contains(keyname));

            return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortCreateDateDesc);
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
                        Reduction = p.Reduction,
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
                                p.IsPay == true && p.IsDelivery == true && (p.Amount * p.Quantity - p.Reduction) != p.RefundAmount &&
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
                        Reduction = p.Reduction,
                        Cost = p.Cost,
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