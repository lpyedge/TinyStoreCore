using System.Collections.Generic;
using System.Linq;


namespace TinyStore.BLL
{
    public class StoreBLL : BaseBLL<Model.StoreModel>
    {
        private static SortDic<Model.StoreModel> SortInitialAsc = new SortDic<Model.StoreModel>()
        {
            [p => p.Initial] = SqlSugar.OrderByType.Asc
        };
        private static SortDic<Model.StoreModel> SortLevelDesc = new SortDic<Model.StoreModel>()
        {
            [p => p.Level] = SqlSugar.OrderByType.Desc
        };
        public static void ChangeAmount(string storeId, double amount)
        {
            Update(p => p.StoreId == storeId, p => p.Amount == p.Amount + amount);
        }
        public static void ModifyLevel(string storeId, EStoreLevel level)
        {
            Update(p => p.StoreId == storeId, p => p.Level == level);
        }
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
        //public static Model.Store QueryModelByUserId(int userId)
        //{
        //    return QueryModel(p => p.UserId == userId);
        //}
        public static List<Model.StoreModel> QueryListByUserId(int userId)
        {
            var where = SqlSugar.Expressionable.Create<Model.StoreModel>()
                .And(p => p.UserId == userId);
            return QueryList(where.ToExpression());
        }

        internal static List<int> QueryUseridsByKey(string keyword)
        {
            List<int> data;

            using (var conn = DbClient)
            {
                data = conn.Queryable<Model.StoreModel>().Where(p => p.Name.Contains(keyword))
                    .ToList().Select(p => p.UserId).ToList<int>();
            }

            return data;
        }

        public static List<Model.StoreModel> QueryListMini()
        {
            using (var conn = DbClient)
            {
                return conn.Queryable<Model.StoreModel>().Select(p => new Model.StoreModel
                {
                    //StoreId = p.StoreId,
                    Name = p.Name,
                    Logo = p.Logo,
                    UniqueId = p.UniqueId,
                }).ToList();
            }
        }

        public static PageList<Model.StoreModel> QueryPageListByKey(string key, int pageindex, int pagesize)
        {
            var where = SqlSugar.Expressionable.Create<Model.StoreModel>()
                .AndIF(!string.IsNullOrWhiteSpace(key), p => p.Name.Contains(key) || p.UniqueId.Contains(key));
            return QueryPageList(pageindex, pagesize, where.ToExpression());
        }

        public static List<Model.StoreModel> QueryListByStoreIds(List<string> storeids)
        {
            using (var conn = DbClient)
            {
                return conn.Queryable<Model.StoreModel>()
                    .Where(p => storeids.Contains(p.StoreId))
                    .Select(p => new Model.StoreModel { StoreId= p.StoreId, Name= p.Name,UniqueId = p.UniqueId })
                    .ToList();
            }
        }

        public static List<Model.StoreModel> QueryHotList(int top)
        {
            return BaseBLL<Model.StoreModel>.QueryList(top, null, SortLevelDesc);
        }
    }
}
