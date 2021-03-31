using SqlSugar;
using System.Collections.Generic;

namespace TinyStore.BLL
{
    public class SupplyBLL : BaseBLL<Model.SupplyModel>
    {
        private static SortDic<Model.SupplyModel> SortSupplyIdDesc = new SortDic<Model.SupplyModel>()
        {
            [p => p.SupplyId] = SqlSugar.OrderByType.Desc,
        };

        public static void DeleteByIds(List<string> ids)
        {
            Delete(p => ids.Contains(p.SupplyId));
        }
        public static List<Model.SupplyModel> QueryListByIds(List<string> ids)
        {
            return QueryList(-1, p => ids.Contains(p.SupplyId), SortSupplyIdDesc);
        }
        
        public static PageList<Model.SupplyModel> QueryPageList(int pageindex, int pagesize)
        {
            return QueryPageList(pageindex, pagesize, null, SortSupplyIdDesc);
        }
        
        public static PageList<Model.SupplyModel> QueryPageListByUserId(int userId, int pageindex, int pagesize)
        {
            return QueryPageList(pageindex, pagesize, p => p.UserId == userId , SortSupplyIdDesc);
        }
        
        
        public static PageList<Model.SupplyModel> QueryPageListByUserIds(List<int> userIds,int pageindex, int pagesize)
        {
            return QueryPageList(pageindex, pagesize, p => userIds.Contains(p.UserId)   , SortSupplyIdDesc);
        }
        
        public static void UpdatePriceStock(List<Model.SupplyModel> list)
        {
            foreach (var item in list)
            {
                Update(p => p.SupplyId == item.SupplyId, p => p.Cost == item.Cost);
            }
        }

    }
}
