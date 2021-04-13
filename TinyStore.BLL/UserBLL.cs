using System.Linq;

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

        public static PageList<Model.UserModel> QueryPageListByKey(string key, int pageindex, int pagesize)
        {
            using (var db = DbClient)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    var data = db.Queryable<Model.UserModel>()
                        .Select(p => new Model.UserModel {Account = p.Account, UserId = p.UserId})
                        .OrderBy(SortCreateDateDesc.First().Key, SortCreateDateDesc.First().Value)
                        .ToPageList(pageindex, pagesize);
                    return new PageList<Model.UserModel>(data);
                }
                else
                {
                    var userids = UserExtendBLL.QueryUseridsByKey(key);
                    userids.AddRange(StoreBLL.QueryUseridsByKey(key));

                    var data = db.Queryable<Model.UserModel>()
                        .Select(p => new Model.UserModel {Account = p.Account, UserId = p.UserId})
                        .Where(p => p.Account.Contains(key) || userids.Contains(p.UserId))
                        .OrderBy(SortCreateDateDesc.First().Key, SortCreateDateDesc.First().Value)
                        .ToPageList(pageindex, pagesize);
                    return new PageList<Model.UserModel>(data);
                }
            }
        }


    }
}