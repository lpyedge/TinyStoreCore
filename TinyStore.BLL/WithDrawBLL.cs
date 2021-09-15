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

        public static double QueryFinace()
        {
            using (var db = DBClient)
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