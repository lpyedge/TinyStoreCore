using System.Collections.Generic;
using System.Linq;

namespace TinyStore.BLL
{
    public class AdminBLL : BaseBLL<Model.AdminModel>
    {
        private static SortDic<Model.AdminModel> SortCreateDateDesc = new SortDic<Model.AdminModel>()
        {
            [p => p.CreateDate] = SqlSugar.OrderByType.Desc,
        };

        public static void DeleteById(int adminid)
        {
            Delete(p => p.AdminId == adminid);
        }

        public static List<Model.AdminModel> QueryListAll()
        {
            using (var db = DbClient)
            {
                var data = db.Queryable<Model.AdminModel>()
                    .Select(p => new Model.AdminModel { Account = p.Account, AdminId = p.AdminId, IsRoot = p.IsRoot, CreateDate = p.CreateDate })
                    .OrderBy(SortCreateDateDesc.First().Key, SortCreateDateDesc.First().Value)
                    .ToList();
                return data;
            }
        }
        public static Model.AdminModel QueryModelByAccount(string account)
        {
            return QueryModel(p => p.Account == account);
        }

        public static Model.AdminModel QueryModelById(int adminid)
        {
            return QueryModel(p => p.AdminId == adminid);
        }

        public static PageList<Model.AdminModel> QueryPageList(int pageindex, int pagesize)
        {
            using (var db = DbClient)
            {
                var data = db.Queryable<Model.AdminModel>()
                    .Select(p => new Model.AdminModel { Account = p.Account, AdminId = p.AdminId, IsRoot = p.IsRoot, CreateDate = p.CreateDate })
                    .OrderBy(SortCreateDateDesc.First().Key, SortCreateDateDesc.First().Value)
                    .ToPageList(pageindex, pagesize);
                return new PageList<Model.AdminModel>(data);
            }
        }

    }
}
