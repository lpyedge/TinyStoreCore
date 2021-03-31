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

        public static void DeleteByProductIdAndStoreId(string productid, string storeId)
        {
            Delete(p => p.ProductId == productid && p.StoreId == storeId);
        }

        public static List<Model.ProductModel> QueryListByCategoryIdAndStoreIdIsStock(string category, string storeId)
        {
            var where = SqlSugar.Expressionable.Create<Model.ProductModel>()
                .And(p => p.StoreId == storeId)
                .And(p => p.DeliveryType == EDeliveryType.卡密)
                .AndIF(!string.IsNullOrWhiteSpace(category), p => p.Category == category);
            return QueryList(-1, where.ToExpression(), SortAsc);
        }

        public static Model.ProductModel QueryModelByProductIdAndStoreId(string productId, string storeId)
        {
            return QueryModel(p => p.ProductId == productId && p.StoreId == storeId);
        }

        public static Model.ProductModel QueryModelByProductId(string productId)
        {
            return QueryModel(p => p.ProductId == productId);
        }
        
        public static PageList<Model.ProductModel> QueryPageListByStoreId(string storeId, int pageindex, int pagesize)
        {
            return QueryPageList(pageindex, pagesize, p => p.StoreId == storeId, SortAsc);
        }
        
        public static List<Model.ProductModel> QueryListByStoreIdShow(string storeId)
        {
            List<Model.ProductModel> data;
            using (var conn = DbClient)
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

        // public static Model.ProductModel QueryModelByProductIdAndStoreIdOrCategory(string productId, string storeId,
        //     string category)
        // {
        //     var where = SqlSugar.Expressionable.Create<Model.ProductModel>()
        //         .And(p => p.ProductId == productId && p.StoreId == storeId)
        //         .AndIF(!string.IsNullOrWhiteSpace(category), p => p.Category == category);
        //
        //     return QueryModel(where.ToExpression());
        // }
        //
        //
        // public static List<string> QueryCategoryListHasProductShowByStoreId(string storeId)
        // {
        //     using (var conn = DbClient)
        //     {
        //         return conn.Queryable<Model.ProductModel>()
        //             .Where(p => p.StoreId == storeId && p.IsShow == true)
        //             .Select(p => p.Category)
        //             .ToList();
        //     }
        // }
        //
        //
        // public static List<Model.ProductModel> QueryListByIdsAndStoreId(List<string> ids, string storeId)
        // {
        //     return QueryList(-1, p => ids.Contains(p.ProductId) && p.StoreId == storeId, SortAsc);
        // }
        //
        //
        // public static void DeleteByCategoryIdAndStoreId(string category, string storeId)
        // {
        //     Delete(p => p.Category == category && p.StoreId == storeId);
        // }
        //
        // public static List<Model.ProductModel> QueryListByCategoryIdShow(string category, string storeId)
        // {
        //     List<Model.ProductModel> data;
        //
        //     using (var conn = DbClient)
        //     {
        //         data = conn.Queryable<Model.ProductModel>()
        //             .Where(p => p.Category == category && p.StoreId == storeId && p.IsShow == true)
        //             .OrderBy(p => p.Sort, SqlSugar.OrderByType.Asc)
        //             .Select(p => new Model.ProductModel
        //                 {ProductId = p.ProductId,Icon = p.Icon, DeliveryType = p.DeliveryType, Name = p.Name, Amount = p.Amount})
        //             .ToList();
        //     }
        //
        //     return data;
        // }
        //
        //
        //
        // public static List<Model.ProductModel> QueryListByCategoryIdAndStoreId(string category, string storeId)
        // {
        //     var where = SqlSugar.Expressionable.Create<Model.ProductModel>()
        //         .And(p => p.StoreId == storeId)
        //         .AndIF(!string.IsNullOrWhiteSpace(category), p => p.Category == category);
        //     return QueryList(-1, where.ToExpression(), SortAsc);
        // }
    }
}