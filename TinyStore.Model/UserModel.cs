
using SqlSugar;
using System;

namespace TinyStore.Model
{
    [Serializable]
    [SqlSugar.SugarTable(nameof(UserModel),IsDisabledUpdateAll=true)]
    public  class UserModel
    {
        [SqlSugar.SugarColumn(IsIdentity = true,IsPrimaryKey = true)]
        public int UserId { get; set; }
        
        [SqlSugar.SugarColumn(Length = 25,IndexGroupNameList = new []{nameof(UserModel)})]
        public string Account { get; set; }

        [SqlSugar.SugarColumn(Length = 40)]
        public string Password { get; set; }

        [SqlSugar.SugarColumn(Length = 10)]
        public string Salt { get; set; }
        
        [SqlSugar.SugarColumn(Length = 32)]
        public string ClientKey { get; set; }
    }
}
