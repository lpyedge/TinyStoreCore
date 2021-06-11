using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;

namespace TinyStore.BLL
{
    public class BillBLL : BaseBLL<Model.BillModel>
    {
        private static SortDic<Model.BillModel> SortCreateDateDesc = new SortDic<Model.BillModel>()
        {
            [p => p.CreateDate] = SqlSugar.OrderByType.Desc,
        };

        public static PageList<Model.BillModel> QueryPageListBySearch(int pageIndex, int pageSize, int userId,int billType, DateTime datefrom, DateTime dateto)
        {
            var expr = Expressionable.Create<Model.BillModel>()
                .And(p => p.UserId == userId )
                .AndIF(billType!=0, p => p.BillType == (EBillType)billType)
                .And( p => SqlFunc.Between(p.CreateDate, datefrom, dateto));

            return QueryPageList(pageIndex, pageSize,expr.ToExpression(), SortCreateDateDesc);
        }
        
        public static PageList<Model.BillModel> QueryPageListBySearch(int pageIndex, int pageSize, int billType, DateTime datefrom, DateTime dateto)
        {
            var expr = Expressionable.Create<Model.BillModel>()
                .AndIF(billType!=0, p => p.BillType == (EBillType)billType)
                .And( p => SqlFunc.Between(p.CreateDate, datefrom, dateto));

            return QueryPageList(pageIndex, pageSize,expr.ToExpression(), SortCreateDateDesc);
        }
    }
}
