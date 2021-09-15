
using System.Collections.Generic;
using System.Linq;
using TinyStore.Model;


namespace TinyStore.BLL
{
    public class UserExtendBLL : BaseBLL<Model.UserExtendModel>
    {
        public static Model.UserExtendModel QueryModelById(int userId)
        {
            return QueryModel(p => p.UserId == userId);
        }

        public static Model.UserExtendModel QueryFinace()
        {
            using (var db = DBClient)
            {
                return db.Queryable<Model.UserExtendModel>()
                    .Select(p => new UserExtendModel()
                    {
                        Amount =  SqlSugar.SqlFunc.AggregateSum(p.Amount) ,
                        AmountCharge = SqlSugar.SqlFunc.AggregateSum(p.AmountCharge)
                    } )
                    .Single();
            }
        }
    }
}
