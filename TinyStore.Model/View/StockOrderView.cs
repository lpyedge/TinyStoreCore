using System;

namespace TinyStore.Model
{
    [Serializable]
    public class StockOrderView
    {
        public string StockId { get; set; }
        
        public string Name { get; set; }
    }
}
