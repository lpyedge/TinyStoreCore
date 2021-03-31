﻿
using System.Collections.Generic;
using System.Linq;


namespace TinyStore.BLL
{
    public class UserExtendBLL : BaseBLL<Model.UserExtendModel>
    {
        public static Model.UserExtendModel QueryModelByEmail(string email)
        {
            return QueryModel(p => p.Email == email);
        }
        public static Model.UserExtendModel QueryModelByTelPhone(string telphone)
        {
            return QueryModel(p => p.TelPhone == telphone);
        }


        public static Model.UserExtendModel QueryModelByUserId(int userId)
        {
            return QueryModel(p => p.UserId == userId);
        }

        public static Model.UserExtendModel QueryModelByIdCard(string idcard)
        {
            return QueryModel(p => p.IdCard == idcard);
        }
        internal static List<int> QueryUseridsByKey(string keyword)
        {
            List<int> data;
            using (var conn = DbClient)
            {
                data = conn.Queryable<Model.UserExtendModel>().Where(p => p.Name.Contains(keyword) || p.Email.Contains(keyword) || p.TelPhone.Contains(keyword) || p.QQ.Contains(keyword))
                    .Select(p => p.UserId).ToList();
            }
            return data;
        }
    }
}
