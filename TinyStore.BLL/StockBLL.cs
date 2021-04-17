using System.Collections.Generic;

namespace TinyStore.BLL
{
    public class StockBLL : BaseBLL<Model.StockModel>
    {
        private static SortDic<Model.StockModel> SortCreateDateDescAndIsUsedAsc = new SortDic<Model.StockModel>()
        {
            [p => p.CreateDate] = SqlSugar.OrderByType.Desc,
            [P => P.IsDelivery] = SqlSugar.OrderByType.Asc
        };

        private static SortDic<Model.StockModel> SortDeliveryDateDescAndIsUsedAsc = new SortDic<Model.StockModel>()
        {
            [p => p.DeliveryDate] = SqlSugar.OrderByType.Desc,
            [P => P.IsDelivery] = SqlSugar.OrderByType.Asc
        };

        private static SortDic<Model.StockModel> SortCreateDateDesc = new SortDic<Model.StockModel>()
        {
            [p => p.CreateDate] = SqlSugar.OrderByType.Desc
        };

        public static void DeleteByStockId(string stockid, int userId)
        {
            Delete(p => p.UserId == userId && p.StockId == stockid);
        }

        public static void DeleteByStockIds(List<string> stockids, int userId)
        {
            Delete(p => p.UserId == userId && stockids.Contains(p.StockId));
        }


        public static int QueryCountBySupplyIdCanUse(string supplyId)
        {
            return QueryCount(p => p.SupplyId == supplyId && p.IsShow == true && p.IsDelivery == false);
        }

        public static List<Model.StockModel> QueryListBySupplyIdCanUse(string supplyId, int userId)
        {
            return QueryList(-1,
                p => p.UserId == userId && p.SupplyId == supplyId && p.IsShow == true && p.IsDelivery == false,
                SortCreateDateDescAndIsUsedAsc);
        }

        public static PageList<Model.StockModel> QueryPageListByUserSearch(string supplyId, int userId, string keyname,bool? isshow,
            int pageindex, int pagesize)
        {
            var exp = SqlSugar.Expressionable.Create<Model.StockModel>()
                .And(p => p.SupplyId == supplyId && p.UserId == userId)
                .AndIF(isshow != null, p => p.IsShow == (bool) isshow)
                .AndIF(!string.IsNullOrWhiteSpace(keyname), p => p.Name.Contains(keyname) || p.Memo.Contains(keyname));

            return QueryPageList(pageindex, pagesize, exp.ToExpression(),
                SortDeliveryDateDescAndIsUsedAsc);
        }
        public static List<Model.StockModel> QueryListByUserSearch(string supplyId, int userId, string keyname,bool? isshow)
        {
            var exp = SqlSugar.Expressionable.Create<Model.StockModel>()
                .And(p => p.SupplyId == supplyId && p.UserId == userId)
                .AndIF(isshow != null, p => p.IsShow == (bool) isshow)
                .AndIF(!string.IsNullOrWhiteSpace(keyname), p => p.Name.Contains(keyname) || p.Memo.Contains(keyname));

            return QueryList(-1, exp.ToExpression(),
                SortDeliveryDateDescAndIsUsedAsc);
        }

        public static void UpdateIsShowByStockIds(List<string> stockids, int userId, bool isshow)
        {
            Update(p => stockids.Contains(p.StockId) && p.UserId == userId, p => p.IsShow == isshow);
        }
    }
}