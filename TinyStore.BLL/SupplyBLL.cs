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
        
        public static List<Model.SupplyModel> QueryListByUserIdIsShow(int userId)
        {
            return QueryList(-1, p => p.IsShow == true && p.UserId == userId , SortSupplyIdDesc);
        }
    }
}
