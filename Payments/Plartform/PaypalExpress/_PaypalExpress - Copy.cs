﻿// using System;
// using System.Collections.Generic;
// using System.Collections.Specialized;
// using System.IO;
// using System.Linq;
// using System.Net;
// using System.Text;
// using System.Web;
// using Payments.Utils;
//
// namespace Payments.Plartform.PaypalExpress
// {
//     [PayPlatformAttribute("Paypal", "贝宝", SiteUrl = "https://www.paypal.com", NotifyProxy = true)]
//     public abstract class _PaypalExpress : IPayChannel
//     {
//         private const string FormData =
//             "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:pt=\"http://svcs.paypal.com/types/pt\">"
//             + "<soapenv:Header />"
//             + "<soapenv:Body>"
//             + "<pt:SetTransactionContextRequest>"
//             + "<requestEnvelope>"
//             + "<errorLanguage>en_US</errorLanguage>"
//             + "</requestEnvelope>"
//             + "<trackingId>[token]</trackingId>"
//             + "<senderAccount>"
//             + "<partnerAccount>"
//             + "<email>[email]</email>"
//             + "<phone>[phone]</phone>"
//             + "<firstName>[firstname]</firstName>"
//             + "<lastName>[lastname]</lastName>"
//             + "<createDate>[createdate]</createDate>"
//             + "<transactionCountTotal>[transactioncounttotal]</transactionCountTotal>"
//             + "<transactionCountThreeMonths>[transactioncountthreemonths]</transactionCountThreeMonths>"
//             + "</partnerAccount>"
//             + "</senderAccount>"
//             + "<receiverAccount>"
//             + "</receiverAccount>"
//             + "<subOrders>"
//             + "<subOrder />"
//             + "</subOrders>"
//             + "<device>"
//             + "<userAgent>[useragent]</userAgent>"
//             + "</device>"
//             + "<ipAddress>"
//             + "<ipAddress>[ipaddress]</ipAddress>"
//             + "</ipAddress>"
//             + "<additionalData>"
//             + "<pair>"
//             + "<key>Country</key>"
//             + "<value>[country]</value>"
//             + "</pair>"
//             + "</additionalData>"
//             + "</pt:SetTransactionContextRequest>"
//             + "</soapenv:Body>"
//             + "</soapenv:Envelope>";
//
//         public readonly string Account = "Account";
//         public readonly string Password = "Password";
//         public readonly string Signature = "Signature";
//         public readonly string Username = "Username";
//
//         public _PaypalExpress() : base()
//         {
//         }
//
//         public _PaypalExpress(string p_SettingsJson) : this()
//         {
//             if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//         }
//
//         protected override void Init()
//         {
//
//             Settings = new List<Setting>
//             {
//                 new Setting
//                 {
//                     Name = Account,
//                     Description = "Paypal的商家帐号(Email地址)",
//                     Regex = @"^[\w\.@]+$",
//                     Requied = true
//                 },
//                 new Setting
//                 {
//                     Name = Username,
//                     Description = "Paypal的API Username",
//                     Regex = @"^[\w\.\-]+$",
//                     Requied = true
//                 },
//                 new Setting
//                 {
//                     Name = Password,
//                     Description = "Paypal的API Password",
//                     Regex = @"^[\w\.\-]+$",
//                     Requied = true
//                 },
//                 new Setting
//                 {
//                     Name = Signature,
//                     Description = "Paypal的Signature",
//                     Regex = @"^[\w\.\-]+$",
//                     Requied = true
//                 }
//             };
//
//             Currencies = new List<ECurrency>
//             {
//                 ECurrency.USD,
//                 ECurrency.AUD,
//                 ECurrency.BRL,
//                 ECurrency.CAD,
//                 ECurrency.CZK,
//                 ECurrency.DKK,
//                 ECurrency.EUR,
//                 ECurrency.HKD,
//                 ECurrency.HUF,
//                 ECurrency.ILS,
//                 ECurrency.JPY,
//                 ECurrency.MYR,
//                 ECurrency.MXN,
//                 ECurrency.NOK,
//                 ECurrency.NZD,
//                 ECurrency.PHP,
//                 ECurrency.PLN,
//                 ECurrency.GBP,
//                 ECurrency.RUB,
//                 ECurrency.SGD,
//                 ECurrency.SEK,
//                 ECurrency.CHF,
//                 ECurrency.TWD,
//                 ECurrency.THB,
//                 ECurrency.TRY
//             };
//         }
//
//     }
// }
//
// //https://developer.paypal.com/webapps/developer/docs/classic/express-checkout/integration-guide/ECGettingStarted/
//
// //DoExpressCheckoutPayment
// //https://developer.paypal.com/webapps/developer/docs/classic/api/merchant/DoExpressCheckoutPayment_API_Operation_NVP/#usesessionpaymentdetails
//
// //lpyedge-buyer@gmail.com  12345678