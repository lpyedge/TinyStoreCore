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

        public static PageList<Model.StockModel> QueryPageListBySupplyId(string supplyId, int userId, bool isshow,
            int pageindex, int pagesize)
        {
            return QueryPageList(pageindex, pagesize,
                p => p.SupplyId == supplyId && p.UserId == userId && p.IsShow == isshow,
                SortDeliveryDateDescAndIsUsedAsc);
        }

        public static void UpdateIsShowByStockIds(List<string> stockids, int userId, bool isshow)
        {
            Update(p => stockids.Contains(p.StockId) && p.UserId == userId, p => p.IsShow == isshow);
        }
    }
}