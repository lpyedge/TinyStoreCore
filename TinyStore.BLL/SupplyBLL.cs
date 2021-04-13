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

        public static void DeleteByIdsAndUserId(List<string> ids,int userId)
        {
            Delete(p => ids.Contains(p.SupplyId) && p.UserId == userId);
        }
        
        public static List<Model.SupplyModel> QueryListByIds(List<string> ids)
        {
            return QueryList(-1, p => ids.Contains(p.SupplyId), SortSupplyIdDesc);
        }
        
        public static List<Model.SupplyModel> QueryListByUserId(int userId)
        {
            return QueryList(-1, p => p.IsShow == true && p.UserId == userId , SortSupplyIdDesc);
        }
        
        public static PageList<Model.SupplyModel> QueryPageListByUserId(int userId, int pageindex, int pagesize)
        {
            return QueryPageList(pageindex, pagesize, p => p.UserId == userId , SortSupplyIdDesc);
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
