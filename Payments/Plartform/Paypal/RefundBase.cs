using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Payments.Plartform.Paypal
{
    [PayChannel(EChannel.Paypal, PayType = EPayType.PC)]
    public class RefundBase : _Paypal, IRefund
    {
        public RefundBase() : base()
        {
        }

        public RefundBase(string p_SettingsJson) : base(p_SettingsJson)
        {
        }

        public bool Refund(string p_OrderId, string p_TxnId)
        {
            //todo
            throw new NotImplementedException();
        }
    }
}

//付款参数文档
//https://developer.paypal.com/docs/classic/paypal-payments-standard/integration-guide/formbasics/
//https://developer.paypal.com/docs/classic/paypal-payments-standard/integration-guide/Appx_websitestandard_htmlvariables/
//IPN参数文档
//https://developer.paypal.com/docs/notifications/
//https://developer.paypal.com/docs/classic/ipn/integration-guide/IPNIntro/
//https://developer.paypal.com/docs/classic/ipn/integration-guide/IPNandPDTVariables/

//退款参数文档
//https://developer.paypal.com/docs/integration/direct/payments/refund-payment/

//Webhook Event Validation
//https://github.com/paypal/PayPal-NET-SDK/wiki/Webhook-Event-Validation

//客户设置IPN文档
//https://developer.paypal.com/docs/classic/ipn/integration-guide/IPNSetup/

//支持的货币类型
//https://developer.paypal.com/docs/classic/api/currency_codes/#id09A6G0U0GYK

//PayPal REST
//http://www.cnblogs.com/feiDD/articles/3179515.html

//技术支持网站
//https://cn.paypal-techsupport.com