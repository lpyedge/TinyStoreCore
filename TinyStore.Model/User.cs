
using SqlSugar;
using System;

namespace TinyStore.Model
{
    [Serializable]
    public  class UserModel
    {
        [SqlSugar.SugarColumn(IsIdentity = true,IsPrimaryKey = true)]
        public int UserId { get; set; }
        
        [SqlSugar.SugarColumn(Length = 25,IndexGroupNameList = new []{"Account"})]
        public string Account { get; set; }

        [SqlSugar.SugarColumn(Length = 40)]
        public string Password { get; set; }

        [SqlSugar.SugarColumn(Length = 10)]
        public string Salt { get; set; }
        
        [SqlSugar.SugarColumn(Length = 32)]
        public string ClientKey { get; set; }

        /// <summary>
        /// 金额 可以提现
        /// </summary>
        public double Amount { get; set; }
        
        /// <summary>
        /// 签帐额度 仅可抵扣商品成本
        /// </summary>
        public double AmountCharge { get; set; }
    }
}
