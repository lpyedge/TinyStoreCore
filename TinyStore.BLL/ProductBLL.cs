using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyStore.BLL
{
    public class ProductBLL : BaseBLL<Model.ProductModel>
    {
        private static SortDic<Model.ProductModel> SortAsc = new SortDic<Model.ProductModel>()
        {
            [p => p.Sort] = SqlSugar.OrderByType.Asc,
        };
        public static Model.ProductModel QueryModelByProductId(string productId)
        {
            return QueryModel(p => p.ProductId == productId);
        }
        public static List<Model.ProductModel> QueryListByStoreId(string storeId)
        {
            return QueryList(-1, p=>p.StoreId == storeId, SortAsc);
        }
        
        public static List<Model.ProductModel> QueryListByStoreShow(string storeId)
        {
            List<Model.ProductModel> data;
            using (var conn = DBClient)
            {
                data = conn.Queryable<Model.ProductModel>()
                    .Where(p => p.StoreId == storeId && p.IsShow == true)
                    .OrderBy(p => p.Sort, SqlSugar.OrderByType.Asc)
                    .ToList();

                foreach (var item in data.Where(p => p.DeliveryType != EDeliveryType.卡密))
                {
                    item.StockNumber = -1;
                }

                if (data.Count > 0)
                {
                    var ids = data.Where(p => p.DeliveryType == EDeliveryType.卡密).Select(p => p.SupplyId).ToList();
                    if (ids.Count > 0)
                    {
                        var list = conn.Queryable<Model.StockModel>()
                            .Where(p => ids.Contains(p.SupplyId) && p.IsShow == true && p.IsDelivery == false)
                            .GroupBy(p => p.SupplyId)
                            .Select(p => new
                                {SupplyId = p.SupplyId, StockNumber = SqlSugar.SqlFunc.AggregateCount(p.StockId)})
                            .ToList();

                        foreach (var item in data.Where(p => p.DeliveryType == EDeliveryType.卡密))
                        {
                            var stockCount = list.FirstOrDefault(x => x.SupplyId == item.ProductId);
                            item.StockNumber = stockCount != null ? stockCount.StockNumber : 0;
                        }
                    }
                }
            }

            return data;
        }

        public static void DeleteByIdsAndUserId(List<string> ids,string sotreId)
        {
            Delete(p => ids.Contains(p.ProductId) && p.StoreId == sotreId);
        }
    }
}