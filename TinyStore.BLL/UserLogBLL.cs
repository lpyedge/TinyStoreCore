using System;


namespace TinyStore.BLL
{
    public class UserLogBLL : BaseBLL<Model.UserLogModel>
    {
        private static SortDic<Model.UserLogModel> SortCreateDateDesc = new SortDic<Model.UserLogModel>()
        {
            [p => p.CreateDate] = SqlSugar.OrderByType.Desc
        };

        public static PageList<Model.UserLogModel> QueryPageListByUserIdOrStoreIdOrType(int userId, string storeId, int type, DateTime begin, DateTime end, int pageindex, int pagesize)
        {
            //var where = SqlSugar.Expressionable.Create<Model.UserLog>()
            //    .And(p => p.UserId == userId && p.CreateDate >= begin && p.CreateDate <= end)
            //    .AndIF(type != 0, p => p.UserLogType == (EUserLogType)type)
            //    .OrIF(!string.IsNullOrWhiteSpace(storeId), p => p.StoreId == storeId);

            var where = SqlSugar.Expressionable.Create<Model.UserLogModel>();
            if (type == 0)
            {
                if (string.IsNullOrWhiteSpace(storeId))
                {
                    where = where.And(p => p.UserId == userId && p.CreateDate >= begin && p.CreateDate <= end);
                }
                else
                {
                    where = where.And(p => p.UserId == userId && p.CreateDate >= begin && p.CreateDate <= end)
                        .Or(p => p.StoreId == storeId && p.CreateDate >= begin && p.CreateDate <= end);
                }
            }
            else if (type < 5)
            {
                where = where.And(p => p.UserId == userId && p.CreateDate >= begin && p.CreateDate <= end && p.UserLogType == (EUserLogType)type);
            }
            else
            {
                where = where.And(p => p.UserId == userId && p.CreateDate >= begin && p.CreateDate <= end && p.UserLogType == (EUserLogType)type)
                    .AndIF(!string.IsNullOrWhiteSpace(storeId), p => p.StoreId == storeId);
            }
            return QueryPageList(pageindex, pagesize, where.ToExpression(), SortCreateDateDesc);
        }
    }
}
