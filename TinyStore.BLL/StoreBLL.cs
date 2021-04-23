﻿using System.Collections.Generic;
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
        
        
        private static SortDic<Model.StoreModel> SortHotDesc = new SortDic<Model.StoreModel>()
        {
            [p => p.Sort] = SqlSugar.OrderByType.Desc
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
                    Initial = p.Initial,
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
            using (var conn = DbClient)
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
