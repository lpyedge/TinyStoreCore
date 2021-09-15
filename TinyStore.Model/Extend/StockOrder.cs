using System;

namespace TinyStore.Model.Extend
{
    [Serializable]
    public class StockOrder
    {
        public string StockId { get; set; }
        
        public string Name { get; set; }
    }
}
