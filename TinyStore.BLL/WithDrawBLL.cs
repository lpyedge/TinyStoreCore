using System;


namespace TinyStore.BLL
{
    public class WithDrawBLL : BaseBLL<Model.WithDrawModel>
    {
        private static SortDic<Model.WithDrawModel> SortCreateDateDesc = new SortDic<Model.WithDrawModel>()
        {
            [p => p.CreateDate] = SqlSugar.OrderByType.Desc
        };

        public static void DeleteByWithDrawIdAndStoreId(string id, string storeId)
        {
            Delete(p => p.WithDrawId == id && p.StoreId == storeId);
        }
        public static void DeleteByWithDrawId(string id)
        {
            Delete(p => p.WithDrawId == id);
        }
        public static Model.WithDrawModel QueryModelByWithDrawId(string id)
        {
            return QueryModel(p => p.WithDrawId == id);
        }
        public static Model.WithDrawModel QueryModelByWithDrawIdAndStoreId(string id, string storeId)
        {
            return QueryModel(p => p.WithDrawId == id && p.StoreId == storeId);
        }
        public static PageList<Model.WithDrawModel> QueryPageList(string storeId, int state, DateTime begin, DateTime end, int pageindex, int pagesize)
        {
            //var expr = Predicate.Generate<Model.WithDraw>(p => p.CreateDate >= begin && p.CreateDate <= end);
            var expr = SqlSugar.Expressionable.Create<Model.WithDrawModel>().And(p => p.CreateDate >= begin && p.CreateDate <= end);
            if (!string.IsNullOrEmpty(storeId))
                expr = expr.And(p => p.StoreId == storeId);
            if (state > 0)
            {
                var estate = (EWithDrawState)state;
                if (estate == EWithDrawState.未处理)
                    expr = expr.And(p => p.IsFinish == false);
                else if (estate == EWithDrawState.提现成功)
                    expr = expr.And(p => p.IsFinish == true && p.Income > 0);
                else
                    expr = expr.And(p => p.IsFinish == true && p.Income == 0);
            }
            return QueryPageList(pageindex, pagesize, expr.ToExpression(), SortCreateDateDesc);
        }

    }
}
