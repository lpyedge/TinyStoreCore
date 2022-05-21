using System;
using System.Collections.Generic;

namespace TinyStore.Model
{
    [Serializable]
    public class UserStoreView
    {
        public int UserId { get; set; }
        public string Account { get; set; }
        
        public string Password { get; set; }
        public UserExtendModel UserExtend { get; set; }
        public  List<StoreModel> Stores { get; set; }
    }
}