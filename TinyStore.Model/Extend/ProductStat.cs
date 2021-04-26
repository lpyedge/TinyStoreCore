using System;

namespace TinyStore.Model.Extend
{
    public class ProductStat
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