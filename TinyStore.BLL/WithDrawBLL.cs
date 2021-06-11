using System;
using SqlSugar;


namespace TinyStore.BLL
{
    public class WithDrawBLL : BaseBLL<Model.WithDrawModel>
    {
        private static SortDic<Model.WithDrawModel> SortCreateDateDesc = new SortDic<Model.WithDrawModel>()
        {
            [p => p.CreateDate] = SqlSugar.OrderByType.Desc
        };

        public static void DeleteByWithDrawIdAndUserId(string id, int userId)
        {
            Delete(p => p.WithDrawId == id && p.UserId == userId);
        }

        public static void DeleteByWithDrawId(string id)
        {
            Delete(p => p.WithDrawId == id);
        }

        public static Model.WithDrawModel QueryModelByWithDrawId(string id)
        {
            return QueryModel(p => p.WithDrawId == id);
        }

        public static Model.WithDrawModel QueryModelByWithDrawIdAndUserId(string id, int userId)
        {
            return QueryModel(p => p.WithDrawId == id && p.UserId == userId);
        }

        public static PageList<Model.WithDrawModel> QueryPageList(int userId, int state, DateTime begin, DateTime end,
            int pageindex, int pagesize)
        {
            //var expr = Predicate.Generate<Model.WithDraw>(p => p.CreateDate >= begin && p.CreateDate <= end);
            var expr = SqlSugar.Expressionable.Create<Model.WithDrawModel>()
                .And(p => p.CreateDate >= begin && p.CreateDate <= end);
            if (userId != 0)
                expr = expr.And(p => p.UserId == userId);
            if (state > 0)
            {
                var estate = (EWithDrawState) state;
                if (estate == EWithDrawState.未处理)
                    expr = expr.And(p => p.IsFinish == false);
                else if (estate == EWithDrawState.提现成功)
                    expr = expr.And(p => p.IsFinish == true && p.AmountFinish > 0);
                else
                    expr = expr.And(p => p.IsFinish == true && p.AmountFinish == 0);
            }

            return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortCreateDateDesc);
        }

        public static double QueryFinace()
        {
            using (var db = DbClient)
            {
                return db.Queryable<Model.WithDrawModel>()
                    .Where(p => !p.IsFinish)
                    .Select(p => SqlSugar.SqlFunc.AggregateSum(p.Amount))
                    .Single();
            }
        }
        
        public static PageList<Model.WithDrawModel> QueryPageListBySearch(int pageIndex, int pageSize, bool? isFinish, DateTime datefrom, DateTime dateto)
        {
            var expr = Expressionable.Create<Model.WithDrawModel>()
                .AndIF(isFinish!= null, p => p.IsFinish == isFinish)
                .And( p => SqlFunc.Between(p.CreateDate, datefrom, dateto));

            return QueryPageList(pageIndex, pageSize,expr.ToExpression(), SortCreateDateDesc);
        }
    }
}