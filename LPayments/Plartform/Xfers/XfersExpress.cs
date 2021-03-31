// using System;
// using System.Net;
// using System.Text;
// using System.Web;
//
// namespace LPayments.Plartform.Xfers
// {
//     [PayChannel("Xfers新加坡", EChannel.XfersExpress)]
//     public class XfersExpress : Xfers, IClientScript
//     {
//         public XfersExpress()
//             : base()
//         {
//         }
//
//         public XfersExpress(string p_SettingsJson)
//             : base(p_SettingsJson)
//         {
//         }
//
//         public string ClientScript
//         {
//             get
//             {
//                 return
//                     "<script src='https://www.xfers.io/xfersjsapi'></script><script>function XfersExpress_Pay(api_key, ordername, orderid, amount, return_url, notify_url,cancel_url) {var myXfer = new Xfers();var items = []; items.push(new myXfer.Item(ordername, amount, 1, '', ''));myXfer.makePayment(api_key, orderid, 0, 0, items, amount, 'SGD', '', cancel_url, return_url, notify_url, 'false');}</script>";
//             }
//         }
//
//         public override PayTicket Pay(string p_OrderId, double p_Amount,
//             ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
//             string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");
//
//             if (string.IsNullOrEmpty(this[ApiKey])) throw new ArgumentNullException("ApiKey");
//             if (string.IsNullOrEmpty(this[ApiSecret])) throw new ArgumentNullException("ApiSecret");
//             if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");
//
//             var formhtml = new StringBuilder();
//             //if (Context.AutoSubmit)
//             //    formhtml.AppendFormat("<script>XfersExpress_Pay('{0}','{1}','{2}',{3},'{4}','{5}','{6}');</script>",
//             //        this[ApiKey], p_OrderName, p_OrderId, p_Amount.ToString("0.##"),p_ReturnUrl,p_NotifyUrl, p_CancelUrl);
//             //else
//                 formhtml.AppendFormat(
//                     "<img src=\"https://s3-ap-southeast-1.amazonaws.com/xfers.io/XfersCheckout.png\" alt=\"Click To Pay\" onclick=\"XfersExpress_Pay('{0}','{1}','{2}',{3},'{4}','{5}','{6}')\" />",
//                     this[ApiKey], p_OrderName, p_OrderId, p_Amount.ToString("0.##"),p_ReturnUrl,p_NotifyUrl,p_CancelUrl);
//
//             var pt = new PayTicket();
//             pt.FormHtml = formhtml.ToString();
//             return pt;
//         }
//     }
// }