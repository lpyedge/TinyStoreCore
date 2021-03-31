using System;

namespace TinyStore.Model
{
    [Serializable]
    public class AdminModel
    {
        [SqlSugar.SugarColumn(IsIdentity = true,IsPrimaryKey = true)]
        public int AdminId { get; set; }
        
        [SqlSugar.SugarColumn(Length = 25,IndexGroupNameList = new []{"Account"})]
        public string Account { get; set; }
        
        [SqlSugar.SugarColumn(Length = 40)]
        public string Password { get; set; }
        
        
        [SqlSugar.SugarColumn(Length = 10)]
        public string Salt { get; set; }
        
        
        [SqlSugar.SugarColumn(Length = 32)]
        public string ClientKey { get; set; }
        
        public DateTime CreateDate { get; set; }
        
        public bool IsRoot { get; set; }
    }
}