using System;
using System.Collections.Generic;
using System.Linq;


namespace TinyStore.BLL
{
    public class OrderBLL : BaseBLL<Model.OrderModel>
    {
        private static SortDic<Model.OrderModel> SortCreateDateDesc = new SortDic<Model.OrderModel>()
        {
            [p => p.CreateDate] = SqlSugar.OrderByType.Desc,
        };

        private static SortDic<Model.OrderModel> SortLastUpdateDateDesc = new SortDic<Model.OrderModel>()
        {
            [p => p.LastUpdateDate] = SqlSugar.OrderByType.Desc,
        };

        public static List<Model.OrderModel> QueryListIsBalanceNo(DateTime time)
        {
            //return QueryList(-1, p => p.IsDelivery == true && p.IsBalance == false && p.ReturnAmount == 0 && p.DeliveryDate <= time);
            return new List<Model.OrderModel>();
        }

        public static Model.OrderModel QueryModelByTranId(string tranId)
        {
            return QueryModel(p => p.TranId == tranId);
        }

        public static Model.OrderModel QueryModelByOrderId(string orderid)
        {
            return QueryModel(p => p.OrderId == orderid);
        }

        public static Model.OrderModel QueryModelByOrderIdAndStoreId(string orderid, string storeId)
        {
            return QueryModel(p => p.OrderId == orderid && p.StoreId == storeId);
        }

        public static List<Model.OrderModel> QueryListBenifitByStoreId(string suppliyid, DateTime begin, DateTime end,
            string storeId, bool ishasreturn)
        {
            var expr = SqlSugar.Expressionable.Create<Model.OrderModel>().And(p =>
                p.StoreId == storeId && p.IsSettle == true && p.LastUpdateDate >= begin && p.LastUpdateDate <= end);
            if (ishasreturn)
                expr = expr.And(p => p.ReturnAmount > 0);
            if (!string.IsNullOrEmpty(suppliyid))
                expr = expr.And(p => p.SupplyId == suppliyid);
            return QueryList(-1, expr.ToExpression(), SortLastUpdateDateDesc);
        }

        public static PageList<Model.OrderModel> QueryPageListBenifitByStoreId(string supplierid, DateTime begin,
            DateTime end, string storeId, bool ishasreturn, int pageindex, int pagesize)
        {
            var expr = SqlSugar.Expressionable.Create<Model.OrderModel>().And(p =>
                p.StoreId == storeId && p.IsSettle == true && p.LastUpdateDate >= begin && p.LastUpdateDate <= end);
            if (ishasreturn)
                expr = expr.And(p => p.ReturnAmount > 0);
            if (!string.IsNullOrEmpty(supplierid))
                expr = expr.And(p => p.SupplyId == supplierid);
            return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortLastUpdateDateDesc);
        }

        public static PageList<Model.OrderModel> QueryPageListIsPaidByStoreId(string storeId, int pageindex, int pagesize)
        {
            return QueryPageList(pageindex, pagesize, p => p.StoreId == storeId && p.IsPay == true && !p.IsDelivery,
                SortLastUpdateDateDesc);
        }

        public static PageList<Model.OrderModel> QueryPageList(DateTime begin, DateTime end, int state, int keykind,
            string key, string storeId, bool ishasreturn, EOrderTimeType timetype, int pageindex, int pagesize)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                    return string.IsNullOrEmpty(storeId)
                        ? QueryPageList(pageindex, pagesize, p => p.OrderId == key)
                        : QueryPageList(pageindex, pagesize, p => p.OrderId == key && p.StoreId == storeId);
                else if (ekeykind == EOrderSearchKey.交易编号)
                    return string.IsNullOrEmpty(storeId)
                        ? QueryPageList(pageindex, pagesize, p => p.TranId == key)
                        : QueryPageList(pageindex, pagesize, p => p.TranId == key && p.StoreId == storeId);
            }

            var expr = SqlSugar.Expressionable.Create<Model.OrderModel>()
                .And(p => p.CreateDate >= begin && p.CreateDate <= end);
            if (timetype == EOrderTimeType.最后变动日期)
                expr = SqlSugar.Expressionable.Create<Model.OrderModel>()
                    .And(p => p.LastUpdateDate >= begin && p.LastUpdateDate <= end);
            //else if (timetype == EOrderTimeType.根据到期时间)
            //    expr = SqlSugar.Expressionable.Create<Model.Order>().And(p => p.DueDate >= begin && p.DueDate <= end);
            if (!string.IsNullOrEmpty(storeId))
                expr = expr.And(p => p.StoreId == storeId);
            if (timetype == EOrderTimeType.付款日期)
                expr = expr.And(p => p.IsPay == true);
            if (state > 0)
            {
                var estate = (EState) state;
                if (estate == EState.客户下单)
                    expr = expr.And(p => p.IsPay == false);
                else if (estate == EState.等待发货)
                    expr = expr.And(p => p.IsPay == true && p.IsDelivery == false);
                else if (estate == EState.完成订单)
                    expr = expr.And(p => p.IsPay == true && p.IsDelivery == true);
            }

            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                {
                    expr = SqlSugar.Expressionable.Create<Model.OrderModel>().And(p => p.OrderId == key);
                    if (!string.IsNullOrEmpty(storeId))
                        expr = expr.And(p => p.StoreId == storeId);
                }
                //else if (ekeykind == EOrderSearchKey.备注)
                //    expr = expr.And(p => p.Remark.Contains(key));
                else if (ekeykind == EOrderSearchKey.联系方式)
                    expr = expr.And(p => p.Contact.Contains(key));
                else if (ekeykind == EOrderSearchKey.商品名称)
                    expr = expr.And(p => p.Name.Contains(key));
                else if (ekeykind == EOrderSearchKey.商品卡号)
                    expr = expr.And(p => p.StockList.Any(s => s.Name == key));
            }

            if (ishasreturn)
            {
                expr = expr.And(p => p.ReturnAmount > 0);
            }

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

        public static PageList<Model.OrderModel> QueryPageListBySid(string sid, int state, int keykind, string key,
            string storeId, bool ishasreturn, int pageindex, int pagesize)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                    return string.IsNullOrEmpty(storeId)
                        ? QueryPageList(pageindex, pagesize, p => p.OrderId == key)
                        : QueryPageList(pageindex, pagesize, p => p.OrderId == key && p.StoreId == storeId);
                else if (ekeykind == EOrderSearchKey.交易编号)
                    return string.IsNullOrEmpty(storeId)
                        ? QueryPageList(pageindex, pagesize, p => p.TranId == key)
                        : QueryPageList(pageindex, pagesize, p => p.TranId == key && p.StoreId == storeId);
            }

            var expr = SqlSugar.Expressionable.Create<Model.OrderModel>().And(p =>
                p.SupplyId == sid && p.IsSettle == false && p.IsPay == true && p.Income > p.ReturnAmount);
            if (!string.IsNullOrEmpty(storeId))
                expr = expr.And(p => p.StoreId == storeId);
            if (state > 0)
            {
                var estate = (EState) state;
                if (estate == EState.等待发货)
                    expr = expr.And(p => p.IsPay == true && p.IsDelivery == false);
                else if (estate == EState.完成订单)
                    expr = expr.And(p => p.IsPay == true && p.IsDelivery == true);
            }

            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                {
                    expr = SqlSugar.Expressionable.Create<Model.OrderModel>().And(p => p.OrderId == key);
                    if (!string.IsNullOrEmpty(storeId))
                        expr = expr.And(p => p.StoreId == storeId);
                }
                //else if (ekeykind == EOrderSearchKey.备注)
                //    expr = expr.And(p => p.Remark.Contains(key));
                else if (ekeykind == EOrderSearchKey.联系方式)
                    expr = expr.And(p => p.Contact.Contains(key));
                else if (ekeykind == EOrderSearchKey.商品名称)
                    expr = expr.And(p => p.Name.Contains(key));
                else if (ekeykind == EOrderSearchKey.商品卡号)
                    expr = expr.And(p => p.StockList.Any(s => s.Name == key));
            }

            if (ishasreturn)
            {
                expr = expr.And(p => p.ReturnAmount > 0);
            }

            //if (ishasduealert)
            //{
            //    expr = expr.And(p => p.DueDate < DateTime.Now.Date.AddDays(1) && p.IsDueAlert == true);
            //}
            return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortLastUpdateDateDesc);
        }

        public static List<Model.OrderModel> QueryList(DateTime begin, DateTime end, int state, int keykind, string key,
            string storeId, bool ishasreturn, EOrderTimeType timetype)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                    return QueryList(-1, p => p.OrderId == key && p.StoreId == storeId);
                else if (ekeykind == EOrderSearchKey.交易编号)
                    return QueryList(-1, p => p.TranId == key && p.StoreId == storeId);
            }

            var expr = SqlSugar.Expressionable.Create<Model.OrderModel>()
                .And(p => p.CreateDate >= begin && p.CreateDate <= end);
            if (timetype == EOrderTimeType.最后变动日期)
                expr = SqlSugar.Expressionable.Create<Model.OrderModel>()
                    .And(p => p.LastUpdateDate >= begin && p.LastUpdateDate <= end);
            //else if (timetype == EOrderTimeType.根据到期时间)
            //    expr = SqlSugar.Expressionable.Create<Model.Order>().And(p => p.DueDate >= begin && p.DueDate <= end);
            if (!string.IsNullOrEmpty(storeId))
                expr = expr.And(p => p.StoreId == storeId);
            if (timetype == EOrderTimeType.付款日期)
                expr = expr.And(p => p.IsPay == true);
            if (state > 0)
            {
                var estate = (EState) state;
                if (estate == EState.客户下单)
                    expr = expr.And(p => p.IsPay == false);
                else if (estate == EState.等待发货)
                    expr = expr.And(p => p.IsPay == true && p.IsDelivery == false);
                else if (estate == EState.完成订单)
                    expr = expr.And(p => p.IsPay == true && p.IsDelivery == true);
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

            if (ishasreturn)
            {
                expr = expr.And(p => p.ReturnAmount > 0);
            }

            //if (ishasduealert)
            //{
            //    expr = expr.And(p => p.DueDate < DateTime.Now.Date.AddDays(1) && p.IsDueAlert == true);
            //}
            if (timetype == EOrderTimeType.最后变动日期)
                return QueryList(-1, expr.ToExpression(), SortLastUpdateDateDesc);
            //if (timetype == EOrderTimeType.根据到期时间)
            //    return QueryList(-1, expr.ToExpression(), SortDueDateDesc);
            return QueryList(-1, expr.ToExpression(), SortCreateDateDesc);
        }

        public static List<Model.OrderModel> QueryListBySid(string sid, int state, int keykind, string key, string storeId,
            bool ishasreturn)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var ekeykind = (EOrderSearchKey) keykind;
                if (ekeykind == EOrderSearchKey.订单号)
                    return QueryList(-1, p => p.OrderId == key && p.StoreId == storeId);
                else if (ekeykind == EOrderSearchKey.交易编号)
                    return QueryList(-1, p => p.TranId == key && p.StoreId == storeId);
            }

            var expr = SqlSugar.Expressionable.Create<Model.OrderModel>().And(p =>
                p.SupplyId == sid && p.IsSettle == false && p.IsPay == true && p.ReturnAmount < p.Income);
            if (!string.IsNullOrEmpty(storeId))
                expr = expr.And(p => p.StoreId == storeId);
            if (state > 0)
            {
                var estate = (EState) state;
                if (estate == EState.等待发货)
                    expr = expr.And(p => p.IsPay == true && p.IsDelivery == false);
                else if (estate == EState.完成订单)
                    expr = expr.And(p => p.IsPay == true && p.IsDelivery == true);
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

            if (ishasreturn)
            {
                expr = expr.And(p => p.ReturnAmount > 0);
            }

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

        //public static void ChangeIsBalance(string orderId, bool isbalance)
        //{
        //    UpdateColumns(p => p.OrderId == orderId, p => p.IsBalance == isbalance);
        //}
        //public static void UpdateIsShouldNotice(string orderId, bool IsShouldNotice)
        //{
        //    UpdateColumns(p => p.OrderId == orderId, p => p.IsShouldNotice == IsShouldNotice);
        //}

        public static void UpdateIsSettle(List<string> ids, string storeId, bool isclose, DateTime closedate)
        {
            var expr = SqlSugar.Expressionable.Create<Model.OrderModel>()
                .And(p => p.IsPay == true && ids.Contains(p.OrderId));
            if (!string.IsNullOrEmpty(storeId))
                expr = expr.And(p => p.StoreId == storeId);
            Update(expr.ToExpression(),
                p => new Model.OrderModel() {IsSettle = isclose, LastUpdateDate = closedate, SettleDate = closedate});
        }
        //public static void UpdateDueDate(List<string> ids, string storeId, DateTime duedate)
        //{
        //    UpdateColumns(p => p.StoreId == storeId && ids.Contains(p.OrderId), p => p.DueDate == duedate);
        //}
        //public static void UpdateIsDueAlert(List<string> ids, string storeId, bool isduealert)
        //{
        //    UpdateColumns(p => p.StoreId == storeId && ids.Contains(p.OrderId), p => p.IsDueAlert == isduealert);
        //}

        //public static void UpdateRemark(List<string> ids, string storeId, string remark)
        //{
        //    UpdateColumns(p => p.StoreId == storeId && ids.Contains(p.OrderId), p => p.Remark == remark);
        //}


        //public static List<Model.Order> QueryListIsShouldNotice()
        //{
        //    return QueryList(-1, p => p.IsPay == true && p.IsShouldNotice == true);
        //}

        public static void UpdateAmountCostIncomme(string orderId, string storeId, double amount, double cost,
            double incomme)
        {
            Update(p => p.OrderId == orderId && p.StoreId == storeId,
                p => new Model.OrderModel {Amount = amount, Cost = cost, Income = incomme});
        }

        public static void UpdateCost(string orderId, double cost)
        {
            Update(p => p.OrderId == orderId, p => p.Cost == cost);
        }

        public static int QueryCountBySuppliersNotClose(string supplyId)
        {
            return QueryCount(p =>
                p.SupplyId == supplyId && p.IsSettle == false && p.IsPay == true && p.ReturnAmount < p.Income);
        }

        public static int QueryCountByOrderIdIsPaid(string orderid, string storeId)
        {
            return QueryCount(p => p.OrderId == orderid && p.StoreId == storeId && p.IsPay == true);
        }
    }
}