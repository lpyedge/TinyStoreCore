using System.Collections.Generic;
using System.Linq;
using SqlSugar;


namespace TinyStore.BLL
{
    public class StoreBLL : BaseBLL<Model.StoreModel>
    {
        private static SortDic<Model.StoreModel> SortInitialAsc = new SortDic<Model.StoreModel>()
        {
            [p => p.Initial] = SqlSugar.OrderByType.Asc
        };
        
        public static Model.StoreModel QueryModelByStoreId(string storeid)
        {
            return QueryModel(p => p.StoreId == storeid);
        }

        public static Model.StoreModel QueryModelById(string id)
        {
            return QueryModel(p => p.StoreId == id || p.UniqueId == id);
        }

        public static Model.StoreModel QueryModelByUniqueId(string uniqueid)
        {
            return QueryModel(p => p.UniqueId == uniqueid);
        }
        
        public static List<Model.StoreModel> QueryListByUserId(int userId)
        {
            var where = SqlSugar.Expressionable.Create<Model.StoreModel>()
                .And(p => p.UserId == userId);
            return QueryList(where.ToExpression());
        }
        public static List<Model.StoreModel> QueryListMini()
        {
            using (var conn = DBClient)
            {
                return conn.Queryable<Model.StoreModel>().Select(p => new Model.StoreModel
                {
                    //StoreId = p.StoreId,
                    Name = p.Name,
                    Logo = p.Logo,
                    Initial = p.Initial,
                    UniqueId = p.UniqueId,
                }).ToList();
            }
        }

        public static List<Model.StoreModel> QueryHotList(int top)
        {
            using (var conn = DBClient)
            {
                return conn.Queryable<Model.StoreModel>().Select(p => new Model.StoreModel
                    {
                        //StoreId = p.StoreId,
                        Name = p.Name,
                        Logo = p.Logo,
                        Initial = p.Initial,
                        UniqueId = p.UniqueId,
                    })
                    .OrderBy(p=>p.Sort,OrderByType.Desc)
                    .Take(top)
                    .ToList();
            }
        }
    }
}
