using System;

namespace TinyStore.Model
{
    [Serializable]
    public class ProductStatView
    {
        public string Name { get; set; }
        
        public int Count { get; set; }
        
        public double Amount { get; set; }
        
        public double Cost { get; set; }
        
        public int Quantity { get; set; }
        
        public double RefundAmount { get; set; }
        
        public DateTime CreateDate { get; set; }
    }
}