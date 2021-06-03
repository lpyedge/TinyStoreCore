using System;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace LPayments.Plartform.Paynet
{
    
    [PayChannel(EChannel.CreditCard)]
    public class Paynet : IPayChannel, IPay
    {
        public const string ClientID = "ClientID";
        public const string Secret = "Secret";

        public Paynet() : base()
        {
            Platform = EPlatform.Paynet;
        }

        public Paynet(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = ClientID, Description = "客户ID", Regex = @"^\w+$", Requied = true},
                new Setting {Name = Secret, Description = "密钥", Regex = @"^\w+$", Requied = true}
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.AUD,
                ECurrency.BRL,
                ECurrency.CAD,
                ECurrency.CZK,
                ECurrency.DKK,
                ECurrency.EUR,
                ECurrency.HKD,
                ECurrency.HUF,
                ECurrency.ILS,
                ECurrency.JPY,
                ECurrency.MYR,
                ECurrency.MXN,
                ECurrency.NOK,
                ECurrency.NZD,
                ECurrency.PHP,
                ECurrency.PLN,
                ECurrency.GBP,
                ECurrency.RUB,
                ECurrency.SGD,
                ECurrency.SEK,
                ECurrency.CHF,
                ECurrency.TWD,
                ECurrency.THB,
                ECurrency.TRY
            };
        }

        public PayResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
            IDictionary<string, string> head, string body, string notifyip)
        {
            PayResult result = new PayResult
            {
                Status = PayResult.EStatus.Pending,
                Message = "fail"
            };

            var Notifydata = Utils.DynamicJson.Parse(body);
            var order = Notifydata.oData.oOrderCreated;
            //if (string.Equals(order.oApplication.sReference.ToString(), this[ClientID], StringComparison.OrdinalIgnoreCase))
            if (string.Equals(order.sOrderExternalReference.ToString(), this[ClientID],
                StringComparison.OrdinalIgnoreCase))
                if (order.ToString().Contains("sState") &&
                    string.Equals(order.sState.ToString(), "paid",
                        StringComparison.OrdinalIgnoreCase))
                {
                    var tax = (double) order.oLastTransaction.fFixFee;
                    if (string.IsNullOrEmpty(order.oLastTransaction.fCommission))
                        tax += (double) order.oLastTransaction.fCommission / 100 *
                               (double) order.oLastTransaction.fAmount;

                    result = new PayResult
                    {
                        OrderName = "",
                        OrderID = order.sDescription.ToString(),
                        Amount = order.oLastTransaction.fAmount,
                        Tax = tax,
                        Currency = Utils.Core.Parse<ECurrency>(order.sCurrencyIsoCode.ToString()),
                        Business = this[ClientID],
                        TxnID = order.sReference.ToString(),
                        PaymentName = Name + "_" + order.oLastTransaction.oPaymentMethod.sType.ToString(),
                        PaymentDate = DateTime.UtcNow,

                        Message = "",

                        Customer = new PayResult._Customer
                        {
                            Email = order.oLastTransaction.oPaymentMethod.oEnduser.sEmail.ToString(),
                            Business = order.oLastTransaction.oPaymentMethod.oEnduser.sEmail.ToString(),
                        }
                    };
                }

            return result;
        }

        public PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[ClientID])) throw new ArgumentNullException("ClientID");
            if (string.IsNullOrEmpty(this[Secret])) throw new ArgumentNullException("Secret");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            var pt = new PayTicket();

            var authorizationUri
#if DEBUG
                = new Uri("https://sandbox-api.payment.net/v1/oauth/token");
#else
                = new Uri("https://api.payment.net/v1/oauth/token");
#endif

            var head = new Dictionary<string, string>();
            var dic = new Dictionary<string, string>
            {
                ["client_id"] = this[ClientID],
                ["client_secret"] = this[Secret],
                ["grant_type"] = "client_credentials"
            };
            var authorizationres = _HWU.Response(authorizationUri, Utils.HttpWebUtility.HttpMethod.Post, dic, head);

            if (authorizationres.Contains("access_token"))
            {
                var authorizationJson = Utils.DynamicJson.Parse(authorizationres);
                var checkoutUri
#if DEBUG
                    = new Uri("https://sandbox-api.payment.net/v1/order");
#else
                    = new Uri("https://api.payment.net/v1/order");
#endif
                head = new Dictionary<string, string>();
                head["Authorization"] = string.Format("Bearer {0}", authorizationJson.access_token);
                dic = new Dictionary<string, string>
                {
                    ["sDescription"] = p_OrderId,
                    ["fAmount"] = p_Amount.ToString("0.##"),
                    ["sCurrencyIsoCode"] = p_Currency.ToString(),
                    ["sOrderExternalReference"] = this[ClientID]
                };
                var checkoutres = _HWU.Response(checkoutUri, Utils.HttpWebUtility.HttpMethod.Post, dic, head);
                if (checkoutres.Contains("success"))
                {
                    var checkoutJson = Utils.DynamicJson.Parse(checkoutres);

                    var gateurl
#if DEBUG
                        = string.Format("https://sandbox-checkout.payment.net/#/payment/{0}/checkout",
                            checkoutJson.oData.sHash);
#else
                    = string.Format("https://checkout.payment.net/#/payment/{0}/checkout", checkoutJson.oData.sHash);
#endif

                    var gatewayUri = new Uri(gateurl +
                                             string.Format("?success_url={0}&cancel_url={1}",
                                                 Utils.HttpWebUtility.UriDataEncode(p_ReturnUrl),
                                                 Utils.HttpWebUtility.UriDataEncode(p_CancelUrl)));

                    // pt.FormHtml = "<script>location.href='" + gatewayUri + "';</script>";
                    // pt.Uri = ;
                    return new PayTicket()
                    {
                        Action = EAction.UrlGet,
                        Uri = gatewayUri.ToString()
                    };
                }

                return new PayTicket(false)
                {
                    Message = checkoutres
                };
            }

            return new PayTicket(false)
            {
                Message = authorizationres
            };
        }
    }
}

//{
//  "sDate": "2016-11-16 17:34:23",
//  "sData": {
//    "oOrderCreated": {
//      "sType": "Order",
//      "oLastTransaction": {
//        "sState": "verified",
//        "fAmount": 100,
//        "oPaymentMethod": {
//          "sType": "CreditCard",
//          "oEnduser": {
//            "sEmail": "customer.email@gmail.com",
//            "sLocale": "en_US"
//          },
//          "sCreditCardHolderName": "TEST TEST"
//        },
//        "fFixFee": 1,
//        "fCommission": 10,
//        "fRetentionPercentage": 60,
//        "sRetentionRunOut": "2017-01-03",
//        "sReference": "BFUKIDIAMADKP",
//        "sCreationDate": "2016-11-04 10:48:45",
//        "sCurrencyIsoCode": "EUR"
//      },
//      "sState": "accepted",
//      "oApplication": {
//        "sReference": "362e8d26-a010-11e6-b79d-f079596743e8",
//        "sName": "New Website",
//        "sWebsiteUrl": "http://www.mysite.com",
//        "sRedirectUrl": "https://www.mysite.com/myoffer",
//        "bActive": true,
//        "bHookEnabled": true
//      },
//      "sReference": "18270392941716697",
//      "sCreationDate": "2016-11-04 10:48:17",
//      "fAmount": 100,
//      "sDescription": "asdf",
//      "sCurrencyIsoCode": "EUR"
//    },
//    "oMerchantOrderData": {
//      "oOrderInfo": {
//        "sInvoiceNumber": "1234567890",
//        "sDeliveryNumber": "0987654321",
//        "sOrderDescription": "order descr",
//        "iItemsCount": 2,
//        "fTotalToPay": 100,
//        "fTaxAmount": 9,
//        "fShippingCost": 8
//      },
//      "oOrderCustomer": {
//        "iCustomerId": 123,
//        "sCustomerType": "type",
//        "sEmail": "customer@mail.example",
//        "sTitle": "Mr",
//        "sFamilyName": "Example",
//        "sGivenName": "Name",
//        "sCompanyName": "Test",
//        "sRegistrationNumber": "123456",
//        "sDateOfBirth": "2006-10-31",
//        "sDateOfIncorporation": "2016-10-31",
//        "sCountryCode": "FR",
//        "sAddressLine1": "Address 1",
//        "sAddressLine2": "Address 2",
//        "sCity": "Paris",
//        "sPostalCode": "12345",
//        "sState": "State",
//        "sRegistrationDate": "2016-10-31",
//        "sFirstOrderDate": "2016-10-31 21:00:00",
//        "fFirstOrderAmount": 10,
//        "sFirstOrderCurrency": "EUR",
//        "sLastOrderDate": "2016-11-01 21:00:00",
//        "fLastOrderAmount": 15,
//        "sLastOrderCurrency": "EUR",
//        "iCompletedOrdersCount": 2,
//        "fCompletedOrdersAmount": 25,
//        "iFailedOrdersCount": 90,
//        "iTotalOrdersCount": 0,
//        "iChargebacksCount": 0,
//        "sTrustableCustomerStatus": "OK"
//      },
//      "aCartItems": [
//        {
//          "sName": "cart1",
//          "sSku": "Sku1",
//          "fPrice": 50,
//          "bIsTaxIncluded": true,
//          "fDiscountAmount": 45,
//          "iDiscountPercentage": 10,
//          "sCurrency": "EUR",
//          "iQuantity": 1,
//          "sDescription": "item1",
//          "iTaxPercentage": 3
//        },
//        {
//          "sName": "cart2",
//          "sSku": "sku2",
//          "fPrice": 50,
//          "bIsTaxIncluded": false,
//          "fDiscountAmount": 0,
//          "iDiscountPercentage": 0,
//          "sCurrency": "EUR",
//          "iQuantity": 1,
//          "sDescription": "item2",
//          "iTaxPercentage": 2
//        }
//      ]
//    }
//  }
//}