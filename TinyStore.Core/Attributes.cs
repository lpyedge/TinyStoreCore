using System;

namespace TinyStore
{
    public class PaymentAttribute:Attribute
    {
        public PaymentAttribute(double rate,string subject,string memo="")
        {
            Subject = subject;
            Rate = rate;
            Memo = memo;
        }
        
        public  string Subject { get; set; }
        
        public string Memo { get; set; }
        
        public double Rate { get; set; }
    }
}