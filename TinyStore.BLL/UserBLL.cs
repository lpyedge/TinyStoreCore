using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SqlSugar;
using TinyStore.Model;
using TinyStore.Model.Extend;

namespace TinyStore.BLL
{
    public class UserBLL : BaseBLL<Model.UserModel>
    {
        private static SortDic<Model.UserModel> SortCreateDateDesc = new SortDic<Model.UserModel>()
        {
            [p => p.UserId] = SqlSugar.OrderByType.Desc
        };

        public static Model.UserModel QueryModelByAccount(string account)
        {
            return QueryModel(p => p.Account == account);
        }

        public static Model.UserModel QueryModelById(int userid)
        {
            return QueryModel(p => p.UserId == userid);
        }

        public static PageList<Model.Extend.UserStore> QueryPageListBySearch(string keyname, int pageindex, int pagesize)
        {
            using (var db = DbClient)
            {
                List<UserStore> data = new List<UserStore>();
                int total = 0;
                if (string.IsNullOrWhiteSpace(keyname))
                {
                    // return UserExtendBLL.QueryPageList(pageindex,pagesize,null,
                    //     new Dictionary<Expression<Func<UserExtendModel, object>>, OrderByType>(){[p=>p.UserId]=OrderByType.Desc});
                    data = db.Queryable<Model.UserModel, Model.UserExtendModel>(
                            (u,ue) => new JoinQueryInfos(JoinType.Inner,u.UserId == ue.UserId))
                        .OrderBy((u,ue)  => u.UserId,OrderByType.Desc)
                        .Select((u,ue)=> new Model.Extend.UserStore()
                        {
                            UserId = u.UserId,
                            Account = u.Account,
                            UserExtend = ue,
                        })
                        .ToPageList(pageindex, pagesize,ref total);
                }
                else
                {
                    data = db.Queryable<Model.UserModel, Model.UserExtendModel>(
                            (u,ue) => new JoinQueryInfos(JoinType.Inner,u.UserId == ue.UserId))
                        .Where((u,ue)=>SqlFunc.Subqueryable<Model.UserModel>()
                            .LeftJoin<Model.StoreModel>((x,y) => 
                                x.UserId == y.UserId && (x.Account.Contains(keyname) ||  y.Name.Contains(keyname) && x.UserId == u.UserId)).Any())
                        .OrderBy((u,ue)  => u.UserId,OrderByType.Desc)
                        .Select((u,ue)=> new Model.Extend.UserStore()
                        {
                            UserId = u.UserId,
                            Account = u.Account,
                            UserExtend = ue,
                        })
                        .ToPageList(pageindex, pagesize,ref total);
                    
                    // return UserExtendBLL.QueryPageList(pageindex,pagesize,p=> 
                    //         SqlFunc.Subqueryable<Model.UserModel>()
                    //             .LeftJoin<Model.StoreModel>((u,s) => 
                    //                 u.UserId == s.UserId && (s.Name.Contains(keyname) ||  u.Account.Contains(keyname) && u.UserId == p.UserId)).Any(),
                    //     new Dictionary<Expression<Func<UserExtendModel, object>>, OrderByType>(){[p=>p.UserId]=OrderByType.Desc});
                }
                
                var userids = data.Select(p => p.UserId).ToList();
                var stores = StoreBLL.QueryList(p => userids.Contains(p.UserId));

                foreach (var item in data)
                {
                    item.Stores = stores.Where(p => p.UserId == item.UserId).ToList();
                }
                    
                return new PageList<Model.Extend.UserStore>(data,total);
            }
        }
    }
}