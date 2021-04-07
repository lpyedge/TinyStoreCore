using System;

namespace TinyStore
{
    public class PaymentAttribute:Attribute
    {
        public PaymentAttribute(double rate,string memo)
        {
            Rate = rate;
            Memo = memo;
        }
        
        public string Memo { get; set; }
        
        public double Rate { get; set; }
    }
}