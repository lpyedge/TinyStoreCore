

//using Braintree;
//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Web;

//namespace YPayments
//{
//    //快速支付接口设置地址
//    //https://www.paypal.com/businessmanage/credentials/apiAccess

//    //Braintree软件开发工具包集成
//    //https://developer.paypal.com/docs/accept-payments/express-checkout/ec-braintree-sdk/server-side/dotnet/

//    //NVP接口
//    //https://developer.paypal.com/docs/classic/api/#express-checkout
//    public class Braintree : IPayment
//    {
//        //private const string FormData =
//        //    "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:pt=\"http://svcs.paypal.com/types/pt\">"
//        //    + "<soapenv:Header />"
//        //    + "<soapenv:Body>"
//        //    + "<pt:SetTransactionContextRequest>"
//        //    + "<requestEnvelope>"
//        //    + "<errorLanguage>en_US</errorLanguage>"
//        //    + "</requestEnvelope>"
//        //    + "<trackingId>[token]</trackingId>"
//        //    + "<senderAccount>"
//        //    + "<partnerAccount>"
//        //    + "<email>[email]</email>"
//        //    + "<phone>[phone]</phone>"
//        //    + "<firstName>[firstname]</firstName>"
//        //    + "<lastName>[lastname]</lastName>"
//        //    + "<createDate>[createdate]</createDate>"
//        //    + "<transactionCountTotal>[transactioncounttotal]</transactionCountTotal>"
//        //    + "<transactionCountThreeMonths>[transactioncountthreemonths]</transactionCountThreeMonths>"
//        //    + "</partnerAccount>"
//        //    + "</senderAccount>"
//        //    + "<receiverAccount>"
//        //    + "</receiverAccount>"
//        //    + "<subOrders>"
//        //    + "<subOrder />"
//        //    + "</subOrders>"
//        //    + "<device>"
//        //    + "<userAgent>[useragent]</userAgent>"
//        //    + "</device>"
//        //    + "<ipAddress>"
//        //    + "<ipAddress>[ipaddress]</ipAddress>"
//        //    + "</ipAddress>"
//        //    + "<additionalData>"
//        //    + "<pair>"
//        //    + "<key>Country</key>"
//        //    + "<value>[country]</value>"
//        //    + "</pair>"
//        //    + "</additionalData>"
//        //    + "</pt:SetTransactionContextRequest>"
//        //    + "</soapenv:Body>"
//        //    + "</soapenv:Envelope>";

//        public readonly string AccessToken = "AccessToken";
      

//        public Braintree()
//        {
//            Init();
//        }

//        public Braintree(string p_SettingsJson)
//        {
//            Init();
//            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//        }

//        public override string Currency => "USD";

//        protected override void Init()
//        {
//            ProxyEnable = true;

//            Settings = new List<Setting>
//            {
//                new Setting
//                {
//                    Name = AccessToken,
//                    Description = "Braintree的AccessToken",
//                    Regex = @"^[\w\.\$]+$",
//                    Requied = true
//                },
//            };
//        }

//        public override NotifyResult Notify(Microsoft.AspNetCore.Http.HttpContext context)
//        {
//            return base.Notify(context);
//        }

//        public override NotifyResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
//            IDictionary<string, string> head, string body, string notifyip)
//        {
//            NotifyResult result = new NotifyResult
//            {
//                Status = NotifyResult.EStatus.Pending,
//                Message = "fail"
//            };

//            var encoding = Encoding.UTF8;
            
//            return result;
//        }

//        public override PayTicket Pay(string p_OrderId, double p_Amount, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
//            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//        {
//            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");
//            if (string.IsNullOrEmpty(this[AccessToken])) throw new ArgumentNullException("AccessToken");

//            BraintreeGateway gateway = new BraintreeGateway("access_token$sandbox$x676gr7sjpwbh9td$157566d7b622430542b6ac20a8ff4ca4");  //调试使用 沙盒下使用
//            //BraintreeGateway gateway = new BraintreeGateway(this[AccessToken]); // access_token$production$g2cpjh6spjsnddng$01df0828781020e0238a6729970ff1a7

//            TransactionRequest request = new TransactionRequest
//            {
//                Amount = (decimal)p_Amount,
//                MerchantAccountId = Currency,
//                PaymentMethodNonce = extend_params,      //前端传入PaymentMethodNonce
//                OrderId = p_OrderId,
//                Descriptor = new DescriptorRequest
//                {
//                    Name = p_OrderName                                       //  "Descriptor displayed in customer CC statements. 22 char max"
//                },

//                Options = new TransactionOptionsRequest
//                {
//                    //PayPal = new TransactionOptionsPayPalRequest
//                    //{
//                    //    CustomField = "PayPal custom field",
//                    //    Description = "Description for PayPal email receipt",
//                    //},
//                    SubmitForSettlement = true
//                }
//            };

//            Result<Transaction> result = gateway.Transaction.Sale(request);

//            if (result.IsSuccess())
//            {
//                System.Console.WriteLine("Transaction ID: " + result.Transaction.Id);
//                var pt = new PayTicket();
//                //pt.Extra = result.Transaction.Id;
//                pt.Extra = result.Target;
//                return pt;
//            }
//            else
//            {
//                var pt = new PayTicket();
//                pt.Message = result.Message;
//                return pt;
//            }

         
         
//        }

//        public static string ClientToken()
//        {
//            BraintreeGateway gateway = new BraintreeGateway("access_token$sandbox$x676gr7sjpwbh9td$157566d7b622430542b6ac20a8ff4ca4");  //调试使用 沙盒下使用
//            //BraintreeGateway gateway = new BraintreeGateway(this[AccessToken]); // access_token$production$g2cpjh6spjsnddng$01df0828781020e0238a6729970ff1a7
//            return gateway.ClientToken.Generate();
//        }
//    }
//}

////https://developer.paypal.com/webapps/developer/docs/classic/express-checkout/integration-guide/ECGettingStarted/

////DoExpressCheckoutPayment
////https://developer.paypal.com/webapps/developer/docs/classic/api/merchant/DoExpressCheckoutPayment_API_Operation_NVP/#usesessionpaymentdetails

////lpyedge-buyer@gmail.com  12345678